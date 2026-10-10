using System;

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
	private static FullScreenFlasher? currentFlasher;

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

		Vector2 pos = localPlayer.GetTruePosition();

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.FlasherFlash))
		{
			caller.WriteByte(localPlayer.PlayerId);
			caller.WriteFloat(pos.x);
			caller.WriteFloat(pos.y);
			caller.WriteFloat(chargeGauge);
			caller.WriteFloat(this.Button?.Behavior is IActivatingBehavior act ? act.ActiveTime : 0.0f);
			caller.WriteFloat(this.effectRadius);
			caller.WriteBoolean(this.affectTeammates);
		}

		return true;
	}

	public static void RpcFlash(in MessageReader reader)
	{
		byte callerId = reader.ReadByte();
		float x = reader.ReadSingle();
		float y = reader.ReadSingle();
		float chargeGauge = reader.ReadSingle();
		float activeTime = reader.ReadSingle();
		float range = reader.ReadSingle();
		bool affectTeammates = reader.ReadBoolean();

		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer.IsInValid() ||
			localPlayer.Data == null ||
			localPlayer.Data.IsDead)
		{
			return;
		}

		if (localPlayer.PlayerId == callerId)
		{
			return;
		}

		Vector2 flasherPos = new Vector2(x, y);
		Vector2 localPos = localPlayer.GetTruePosition();

		if (Vector2.Distance(localPos, flasherPos) > range)
		{
			return;
		}

		if (!affectTeammates &&
			localPlayer.Data.Role != null &&
			localPlayer.Data.Role.IsImpostor)
		{
			return;
		}

		float aftereffectTime = MaxAftereffectTime * Mathf.Clamp01(chargeGauge);
		float fadeInTime = 0.01f;
		float holdTime = activeTime;
		float fadeOutTime = Mathf.Max(0.01f, aftereffectTime);

		if (currentFlasher != null)
		{
			currentFlasher.Hide();
		}

		currentFlasher = new FullScreenFlasher(
			Color.white,
			maxAlpha: 1.0f,
			fadeInTime: fadeInTime,
			fadeOutTime: fadeOutTime,
			holdTime: holdTime);
		currentFlasher.Flash();

		var followerCamera = Camera.main != null ? Camera.main.GetComponent<FollowerCamera>() : null;
		if (followerCamera == null &&
			HudManager.Instance != null &&
			HudManager.Instance.transform != null &&
			HudManager.Instance.transform.parent != null)
		{
			followerCamera = HudManager.Instance.transform.parent.GetComponent<FollowerCamera>();
		}

		if (followerCamera != null)
		{
			float shakeAmount = Mathf.Lerp(0.2f, 1.0f, chargeGauge);
			followerCamera.shakeAmount = shakeAmount;
		}
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
	}

	public void ResetOnMeetingStart()
	{
		if (currentFlasher != null)
		{
			currentFlasher.Hide();
		}
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		if (currentFlasher != null)
		{
			currentFlasher.Hide();
		}
	}
}
