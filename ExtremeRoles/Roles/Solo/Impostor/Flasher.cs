using System;
using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using Hazel;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Flasher : SingleRoleBase, IRoleAutoBuildAbility
{
	public enum Option
	{
		HoldTimeMin,
		HoldTimeMax,
		EffectRadius,
		AffectTeammates,
	}

	private float holdTimeMin = 1.0f;
	private float holdTimeMax = 5.0f;
	private float effectRadius = 8.0f;
	private bool affectTeammates = false;

	private FlasherScreenEffect? currentEffect;

	public ExtremeAbilityButton Button { get; set; } = null!;

	public Flasher() : base(
		RoleArgs.BuildImpostor(ExtremeRoleId.Flasher))
	{
	}

	protected override void RoleSpecificInit()
	{
		var cate = this.Loader;
		this.holdTimeMin = cate.GetValue<Option, float>(Option.HoldTimeMin);
		this.holdTimeMax = cate.GetValue<Option, float>(Option.HoldTimeMax);
		this.effectRadius = cate.GetValue<Option, float>(Option.EffectRadius);
		this.affectTeammates = cate.GetValue<Option, bool>(Option.AffectTeammates);
	}

	public void CreateAbility()
	{
		var behavior = new ChargingAndReclickCountBehavior(
			Tr.GetString("FlasherAbility"),
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Flasher),
			(isCharge, gage) =>
			{
				if (isCharge)
				{
					return this.IsAbilityUse() && gage > 0.0f;
				}
				return IRoleAbility.IsCommonUse();
			},
			this.UseAbility,
			this.UseChargedAbility,
			reduceOnCharge: false);

		this.Button = new ExtremeAbilityButton(
			behavior,
			new RoleButtonActivator(),
			KeyCode.F);

		((IRoleAbility)this).RoleAbilityInit();
	}

	public bool IsAbilityUse()
	{
		return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.CanMove && IRoleAbility.IsCommonUse();
	}

	public bool UseAbility() => true;

	public bool UseChargedAbility(float chargeGauge)
	{
		if (PlayerControl.LocalPlayer == null)
		{
			return false;
		}

		float clampedGauge = Mathf.Clamp01(chargeGauge);
		float activeTime = Mathf.Lerp(this.holdTimeMin, this.holdTimeMax, clampedGauge);

		List<byte> targets = this.getTargetsInRange(PlayerControl.LocalPlayer);

		MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
			PlayerControl.LocalPlayer.NetId,
			(byte)RPCOperator.Command.FlasherFlash,
			SendOption.Reliable);

		writer.Write(PlayerControl.LocalPlayer.PlayerId);
		writer.Write(clampedGauge);
		writer.Write(activeTime);
		writer.WritePacked(targets.Count);
		foreach (byte targetId in targets)
		{
			writer.Write(targetId);
		}

		AmongUsClient.Instance.FinishRpcImmediately(writer);

		this.TriggerFlashEffect(
			targets,
			PlayerControl.LocalPlayer.PlayerId,
			clampedGauge,
			activeTime);

		return true;
	}

	public static void RpcFlash(MessageReader reader)
	{
		byte callerId = reader.ReadByte();
		float chargeGauge = reader.ReadSingle();
		float activeTime = reader.ReadSingle();

		int count = reader.ReadPackedInt32();
		List<byte> targets = new List<byte>(count);
		for (int i = 0; i < count; i++)
		{
			targets.Add(reader.ReadByte());
		}

		if (ExtremeRoleManager.TryGetRole(callerId, out var callerRole) && callerRole is Flasher flasherRole)
		{
			flasherRole.TriggerFlashEffect(targets, callerId, chargeGauge, activeTime);
		}
		else
		{
			if (PlayerControl.LocalPlayer != null && targets.Contains(PlayerControl.LocalPlayer.PlayerId))
			{
				var effect = new FlasherScreenEffect(Color.white, activeTime, 0.5f);
				effect.Flash(chargeGauge);
			}
		}
	}

	public void TriggerFlashEffect(
		List<byte> targetIds,
		byte callerId,
		float chargeGauge,
		float activeTime)
	{
		if (PlayerControl.LocalPlayer == null)
		{
			return;
		}

		byte localId = PlayerControl.LocalPlayer.PlayerId;

		if (targetIds.Contains(localId))
		{
			this.currentEffect?.Hide();
			this.currentEffect = new FlasherScreenEffect(Color.white, activeTime, 0.5f);
			this.currentEffect.Flash(chargeGauge);
		}
	}

	public void ResetOnMeetingStart()
	{
		this.currentEffect?.Hide();
		FlasherScreenEffect.Current?.Hide();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		this.currentEffect?.Hide();
		FlasherScreenEffect.Current?.Hide();
	}

	private List<byte> getTargetsInRange(PlayerControl sourcePlayer)
	{
		List<byte> targetIds = new List<byte>();
		Vector2 truePosition = sourcePlayer.GetTruePosition();

		var allPlayers = GameData.Instance.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			NetworkedPlayerInfo playerInfo = allPlayers[i];
			if (playerInfo == null ||
				playerInfo.IsDead ||
				playerInfo.Disconnected ||
				playerInfo.Object == null)
			{
				continue;
			}

			PlayerControl target = playerInfo.Object;
			if (target.PlayerId == sourcePlayer.PlayerId ||
				target.inVent ||
				target.inMovingPlat ||
				target.onLadder)
			{
				continue;
			}

			if (!ExtremeRoleManager.TryGetRole(playerInfo.PlayerId, out var targetRole))
			{
				continue;
			}

			if (this.IsSameTeam(targetRole) && !this.affectTeammates)
			{
				continue;
			}

			Vector2 vector = target.GetTruePosition() - truePosition;
			float magnitude = vector.magnitude;

			if (magnitude <= this.effectRadius)
			{
				targetIds.Add(playerInfo.PlayerId);
			}
		}

		return targetIds;
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(
			factory, defaultAbilityCount: 3, maxAbilityCount: 10, minAbilityCount: 1);

		factory.CreateFloatOption(
			Option.HoldTimeMin, 1.0f, 0.5f, 10.0f, 0.5f, format: OptionUnit.Second);

		factory.CreateFloatOption(
			Option.HoldTimeMax, 5.0f, 1.0f, 15.0f, 0.5f, format: OptionUnit.Second);

		factory.CreateFloatOption(
			Option.EffectRadius, 8.0f, 2.0f, 20.0f, 1.0f);

		factory.CreateBoolOption(
			Option.AffectTeammates, false);
	}
}
