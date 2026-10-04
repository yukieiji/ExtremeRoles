using System;
using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Extension.Player;

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
		RoleArgs.BuildLiberalDove(ExtremeRoleId.Delta))
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
		if (Player.TryGetClosestPlayerInRange(PlayerControl.LocalPlayer, this, this.range, out var target))
		{
			this.currentTarget = target.PlayerId;
		}

		return IRoleAbility.IsCommonUse() && this.currentTarget != byte.MaxValue;
	}

	public bool UseAbility()
	{
		if (this.currentTarget == byte.MaxValue)
		{
			return false;
		}
		byte targetId = this.currentTarget;

		ExecuteAbility(PlayerControl.LocalPlayer, targetId);
		this.currentTarget = byte.MaxValue;
		return true;
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
			0.75f, 0.25f, 3.5f, 0.25f);
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

	public void ExecuteAbility(PlayerControl deltaPlayer, byte targetPlayerId)
	{
		if (deltaPlayer.IsInValid())
		{
			return;
		}

		byte deltaPlayerId = deltaPlayer.PlayerId;
		var deltaInfo = deltaPlayer.Data;

		NetworkedPlayerInfo? targetInfo = GameData.Instance.GetPlayerById(targetPlayerId);

		if (targetInfo == null)
		{
			ExplodeDelta(deltaPlayerId);
			return;
		}

		int deltaCompleted = GetCompletedTaskCount(deltaInfo);
		int targetCompleted = GetCompletedTaskCount(targetInfo);

		bool isSameTeam = ExtremeRoleManager.TryGetRole(targetPlayerId, out var targetRole) && IsSameTeam(targetRole);

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
			float earnedMoney = diff * this.moneyPerTaskDiff;
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
		if (playerInfo == null || 
			playerInfo.Tasks == null || 
			playerInfo.Tasks.Count == 0)
		{
			return 0;
		}

		int count = 0;
		foreach (var task in playerInfo.Tasks.GetFastEnumerator())
		{
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
