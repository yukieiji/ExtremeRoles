using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Resources;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.SystemType.Roles;

using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory;


#nullable enable

namespace ExtremeRoles.GhostRoles.Impostor;

public sealed class Doppelganger : GhostRoleBase
{
	private FakerDummySystem.FakePlayer? fake;
	private ShapeShiftMinigameWrapper? minigame;
	private byte? target;

	private const AbilityType ability = AbilityType.DoppelgangerDoppel;

	public Doppelganger() : base(
		false,
		ExtremeRoleType.Impostor,
		ExtremeGhostRoleId.Doppelganger,
		ExtremeGhostRoleId.Doppelganger.ToString(),
		Palette.ImpostorRed)
	{ }

	public static void Doppl(byte rolePlayer, byte targetPlayer)
	{
		var rolePc = Player.GetPlayerControlById(rolePlayer);
		var targetPc = Player.GetPlayerControlById(targetPlayer);

		var ghostRole = ExtremeGhostRoleManager.GetSafeCastedGhostRole<Doppelganger>(rolePlayer);
		if (ghostRole is null)
		{
			return;
		}

		if (ghostRole.fake is null)
		{
			SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
			ghostRole.fake = new FakerDummySystem.FakePlayer(
				rolePc, targetPc,
				role.IsImpostor() || role.Core.Id == ExtremeRoleId.Marlin);

			ghostRole.fake.Body.transform.SetParent(rolePc.transform);
			var pet = ghostRole.fake.Body.GetComponentInChildren<PetBehaviour>();
			if (pet != null)
			{
				// 何かわからないけどペットが移動方向と全く別の方向に行く、後で調整する
				Object.Destroy(pet.gameObject);
			}
			ghostRole.fake.Body.layer = targetPc.gameObject.layer;
		}
		else
		{
			ghostRole.fake.Clear();
			ghostRole.fake = null;
		}
		ghostRole.target = null;
	}

	public override void CreateAbility()
	{
		bool isReportAbility = this.IsReportAbility();
		var behavior = new ChargingAndActivatingCountBehaviour(
			text: Tr.GetString($"{ability}Button"),
			img: UnityObjectLoader.LoadFromResources(
				ExtremeRoleId.Faker,
				ObjectPath.FakerDummyPlayer),
			isUse: isAbilityUse,
			canActivating: isActivating,
			ability: (_) =>
			{
				if (this.target == null)
				{
					return false;
				}

				using (var caller = RPCOperator.CreateCaller(
					RPCOperator.Command.UseGhostRoleAbility))
				{
					caller.WriteByte((byte)ability);
					caller.WriteBoolean(isReportAbility);
					this.UseAbility(caller);
				}

				Doppl(PlayerControl.LocalPlayer.PlayerId, this.target.Value);

				if (isReportAbility)
				{
					MeetingReporter.Instance.AddMeetingStartReport(
						Tr.GetString(ability.ToString()));
				}

				return true;
			},
			onCharge: openUI,
			isCharge: () => this.minigame is not null && this.minigame.IsOpen,
			reduceTiming: ChargingAndActivatingCountBehaviour.ReduceTiming.OnActive,
			abilityOff: abilityOff,
			forceAbilityOff: abilityOff);

		behavior.ChargeTime = float.MaxValue;

		this.Button = new ExtremeAbilityButton(
			behavior,
			new GhostRoleButtonActivator(),
			KeyCode.F
		);
		this.ButtonInit();
	}

	public override HashSet<ExtremeRoleId> GetRoleFilter() => [];

	public override void Initialize()
	{

	}

	protected override void OnMeetingEndHook()
	{
		return;
	}

	protected override void OnMeetingStartHook()
	{
		this.minigame?.Reset();
	}

	protected override void CreateSpecificOption(OptionFactory factory)
	{
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 2, 10, 5.0f);
	}

	protected override void UseAbility(RPCOperator.RpcCaller caller)
	{
		byte playerId = PlayerControl.LocalPlayer.PlayerId;
		caller.WriteByte(playerId);
		caller.WriteByte(this.target!.Value);
	}

	private bool openUI()
	{
		this.minigame ??= new ShapeShiftMinigameWrapper();
		return this.minigame.IsOpen || this.minigame.OpenUi(this.abilityCall);
	}

	private bool isAbilityUse(bool isCharge, float _)
	{
		bool isSab = PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
			PlayerControl.LocalPlayer);
		if (isSab)
		{
			if (isCharge && Minigame.Instance != null)
			{
				Minigame.Instance.ForceClose();
			}
			return false;
		}
		if (isCharge)
		{
			return IsCommonUseWithMinigame();
		}

		return IsCommonUse();
	}

	private void abilityOff()
	{
		if (this.fake is null)
		{
			return;
		}

		byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;
		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.UseGhostRoleAbility))
		{
			caller.WriteByte((byte)ability);
			caller.WriteBoolean(false);
			caller.WriteByte(localPlayerId);
			caller.WriteByte(byte.MinValue);
		}

		Doppl(localPlayerId, byte.MinValue);
	}

	private bool isActivating()
		=> !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
			PlayerControl.LocalPlayer);

	private void abilityCall(PlayerControl target)
	{
		if (this.Button is null ||
			!this.Button.Transform.TryGetComponent<PassiveButton>(out var button))
		{
			return;
		}

		this.target = target.PlayerId;
		button.OnClick.Invoke();
	}
}
