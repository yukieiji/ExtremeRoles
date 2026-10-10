using System;
using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance.Il2Cpp;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Flasher : SingleRoleBase, IRoleAutoBuildAbility
{
	public enum FlasherOption
	{
		ChargeTime,
		EffectRadius,
		AffectTeammates,
	}

	public const float MaxAftereffectTime = 1.0f;

	private float effectRadius;
	private bool affectTeammates;
	private FlasherScreenEffect? flasherEffect;

	public ExtremeAbilityButton? Button { get; set; }

	public Flasher() : base(
		RoleArgs.BuildImpostor(ExtremeRoleId.Flasher))
	{ }

	public void CreateAbility()
	{
		var behavior = new ChargingAndActivatingCountBehaviour(
			Tr.GetString("FlasherAbility"),
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Flasher),
			isUse: (isCharge, gage) =>
			{
				if (isCharge)
				{
					return IsAbilityUse() && gage > 0.0f;
				}
				return IRoleAbility.IsCommonUse();
			},
			ability: UseChargedAbility,
			onCharge: UseAbility,
			reduceTiming: ChargingAndActivatingCountBehaviour.ReduceTiming.OnActive);

		this.Button = new ExtremeAbilityButton(
			behavior,
			new RoleButtonActivator(),
			KeyCode.F);

		((IRoleAbility)this).RoleAbilityInit();

		behavior.ChargeTime = this.Loader.GetValue<FlasherOption, float>(FlasherOption.ChargeTime);
	}

	public bool IsAbilityUse()
	{
		return IRoleAbility.IsCommonUse();
	}

	public bool UseAbility()
	{
		return true;
	}

	public bool UseChargedAbility(float chargeGauge)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null || localPlayer.Data == null)
		{
			return false;
		}

		List<byte> targetPlayerIds = getTargetsInRange(localPlayer);

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.FlasherFlash))
		{
			caller.WriteByte(localPlayer.PlayerId);
			caller.WriteFloat(chargeGauge);
			caller.WriteFloat(this.Button?.Behavior is IActivatingBehavior act ? act.ActiveTime : 0.0f);
			caller.WritePackedInt(targetPlayerIds.Count);
			foreach (byte targetId in targetPlayerIds)
			{
				caller.WriteByte(targetId);
			}
		}

		return true;
	}

	public static void RpcFlash(in MessageReader reader)
	{
		byte callerId = reader.ReadByte();
		float chargeGauge = reader.ReadSingle();
		float activeTime = reader.ReadSingle();
		int targetCount = reader.ReadPackedInt32();

		HashSet<byte> targetPlayerIds = new HashSet<byte>();
		for (int i = 0; i < targetCount; ++i)
		{
			targetPlayerIds.Add(reader.ReadByte());
		}

		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer.IsInValid() ||
			localPlayer.Data == null ||
			localPlayer.Data.IsDead)
		{
			return;
		}

		if (!targetPlayerIds.Contains(localPlayer.PlayerId))
		{
			return;
		}

		float aftereffectTime = MaxAftereffectTime * Mathf.Clamp01(chargeGauge);
		float fadeOutTime = Mathf.Max(0.01f, aftereffectTime);

		if (ExtremeRoleManager.TryGetSafeCastedRole<Flasher>(callerId, out var flasherRole) &&
			flasherRole.flasherEffect != null)
		{
			flasherRole.flasherEffect.Flash(fadeOutTime);
		}
		else
		{
			float holdTime = activeTime > 0.0f ? activeTime : 3.0f;
			var fallbackEffect = new FlasherScreenEffect(Color.white, holdTime, 0.5f);
			fallbackEffect.Flash(fadeOutTime);
		}
	}

	private List<byte> getTargetsInRange(PlayerControl sourcePlayer)
	{
		List<byte> targetIds = new List<byte>();
		Vector2 truePosition = sourcePlayer.GetTruePosition();

		foreach (NetworkedPlayerInfo playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
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
			factory,
			defaultAbilityCount: 3,
			maxAbilityCount: 10,
			defaultActiveTime: 3.0f);

		factory.CreateFloatOption(
			FlasherOption.ChargeTime,
			3.0f, 0.5f, 10.0f, 0.5f,
			format: OptionUnit.Second);

		factory.CreateFloatOption(
			FlasherOption.EffectRadius,
			10.0f, 1.0f, 50.0f, 0.5f);

		factory.CreateBoolOption(
			FlasherOption.AffectTeammates,
			false);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.effectRadius = loader.GetValue<FlasherOption, float>(FlasherOption.EffectRadius);
		this.affectTeammates = loader.GetValue<FlasherOption, bool>(FlasherOption.AffectTeammates);

		float activeTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime);
		if (activeTime <= 0.0f)
		{
			activeTime = 3.0f;
		}

		this.flasherEffect = new FlasherScreenEffect(Color.white, activeTime, 0.5f);
	}

	public void ResetOnMeetingStart()
	{
		if (this.flasherEffect != null)
		{
			this.flasherEffect.Hide();
		}
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		if (this.flasherEffect != null)
		{
			this.flasherEffect.Hide();
		}
	}
}
