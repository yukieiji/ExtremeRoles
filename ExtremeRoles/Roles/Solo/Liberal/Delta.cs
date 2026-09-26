using System;
using Hazel;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class Delta : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate
{
	public enum DeltaOption
	{
		Range,
		MoneyPerTaskDiff
	}

	public ExtremeAbilityButton Button { get; set; } = null!;

	private float range;
	private int moneyPerTaskDiff;
	private byte currentTarget = byte.MaxValue;

	private DoveCommonAbilityHandler? handler;

	public Delta() : base(
		RoleArgs.BuildLiberalMilitant(ExtremeRoleId.Delta))
	{
	}

	public override string GetRoleTag() => "Dl";

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"STRMISS",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.AgencyTakeTask));
		this.Button.SetLabelToCrewmate();
	}

	public bool IsAbilityUse()
	{
		this.currentTarget = byte.MaxValue;

		PlayerControl? target = GetClosestTarget(PlayerControl.LocalPlayer, this.range, this);
		if (target != null)
		{
			this.currentTarget = target.PlayerId;
		}

		return IRoleAutoBuildAbility.IsCommonUse() && this.currentTarget != byte.MaxValue;
	}

	public bool UseAbility()
	{
		if (this.currentTarget != byte.MaxValue)
		{
			byte deltaId = PlayerControl.LocalPlayer.PlayerId;
			byte targetId = this.currentTarget;

			ExecuteAbility(deltaId, targetId);
			this.currentTarget = byte.MaxValue;
			return true;
		}
		return false;
	}

	public void ResetOnMeetingStart()
	{
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void Update(PlayerControl rolePlayer)
	{
		this.handler?.Update(rolePlayer);
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		this.handler?.ClearTask(rolePlayer);
	}

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		this.handler?.ClearTask(rolePlayer);
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 2, 5);
		factory.CreateFloatOption(
			DeltaOption.Range,
			1.5f, 0.5f, 5.0f, 0.25f);
		factory.CreateIntOption(
			DeltaOption.MoneyPerTaskDiff,
			10, 1, 100, 1);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		var liberalOption = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptionLoader>();
		LiberalSettingOverrider.OverrideDefault(this, liberalOption);
		this.handler = new DoveCommonAbilityHandler(liberalOption);

		this.range = loader.GetValue<DeltaOption, float>(DeltaOption.Range);
		this.moneyPerTaskDiff = loader.GetValue<DeltaOption, int>(DeltaOption.MoneyPerTaskDiff);
	}

	public static PlayerControl? GetClosestTarget(
		PlayerControl sourcePlayer,
		float range,
		SingleRoleBase role)
	{
		if (!ShipStatus.Instance)
		{
			return null;
		}

		Vector2 truePosition = sourcePlayer.GetTruePosition();
		PlayerControl? closest = null;
		float closestDist = float.MaxValue;

		foreach (PlayerControl pc in PlayerCache.AllPlayerControl)
		{
			if (pc == null || pc.IsInValid() || pc.PlayerId == sourcePlayer.PlayerId || !pc.IsAlive())
			{
				continue;
			}

			if (pc.inVent || pc.inMovingPlat || pc.onLadder)
			{
				continue;
			}

			Vector2 vector = pc.GetTruePosition() - truePosition;
			float magnitude = vector.magnitude;
			if (magnitude <= range &&
				!PhysicsHelpers.AnyNonTriggersBetween(
					truePosition, vector.normalized,
					magnitude, Constants.ShipAndObjectsMask))
			{
				if (magnitude < closestDist)
				{
					closestDist = magnitude;
					closest = pc;
				}
			}
		}

		if (closest != null)
		{
			PlayerOutLine.SetOutline(closest, role.GetNameColor());
		}

		return closest;
	}

	public static void RpcOps(MessageReader reader)
	{
		byte deltaPlayerId = reader.ReadByte();
		byte targetPlayerId = reader.ReadByte();

		ExecuteAbility(deltaPlayerId, targetPlayerId);
	}

	public static void ExecuteAbility(byte deltaPlayerId, byte targetPlayerId)
	{
		if (!ExtremeRoleManager.TryGetSafeCastedRole<Delta>(deltaPlayerId, out var deltaRole))
		{
			return;
		}

		NetworkedPlayerInfo? deltaInfo = GameData.Instance.GetPlayerById(deltaPlayerId);
		NetworkedPlayerInfo? targetInfo = GameData.Instance.GetPlayerById(targetPlayerId);

		if (deltaInfo == null || targetInfo == null)
		{
			ExplodeDelta(deltaPlayerId);
			return;
		}

		int deltaCompleted = GetCompletedTaskCount(deltaInfo);
		int targetCompleted = GetCompletedTaskCount(targetInfo);

		bool isSameTeam = false;
		if (ExtremeRoleManager.TryGetRole(targetPlayerId, out var targetRole))
		{
			isSameTeam = deltaRole.IsSameTeam(targetRole);
		}

		bool shouldExplode =
			deltaCompleted == 0 ||
			targetCompleted == 0 ||
			deltaCompleted == targetCompleted ||
			isSameTeam;

		if (shouldExplode)
		{
			ExplodeDelta(deltaPlayerId);
		}
		else
		{
			int diff = Math.Abs(targetCompleted - deltaCompleted);
			float earnedMoney = diff * deltaRole.moneyPerTaskDiff;
			if (earnedMoney > 0f)
			{
				LiberalMoneyBankSystem.RpcUpdateSystem(
					deltaPlayerId,
					LiberalMoneyHistory.Reason.STRMISS,
					earnedMoney);
			}
		}
	}

	public static int GetCompletedTaskCount(NetworkedPlayerInfo? playerInfo)
	{
		if (playerInfo == null || playerInfo.Tasks == null || playerInfo.Tasks.Count == 0)
		{
			return 0;
		}

		int count = 0;
		for (int i = 0; i < playerInfo.Tasks.Count; i++)
		{
			var task = playerInfo.Tasks[i];
			if (task != null && task.Complete)
			{
				count++;
			}
		}
		return count;
	}

	private static void ExplodeDelta(byte deltaPlayerId)
	{
		Player.RpcUncheckMurderPlayer(deltaPlayerId, deltaPlayerId, byte.MinValue);
		ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
			deltaPlayerId,
			ExtremeShipStatus.PlayerStatus.Explosion);
	}
}
