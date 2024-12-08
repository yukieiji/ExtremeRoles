using System.Linq;
using System.Collections.Generic;

using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Extension.Il2Cpp;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.CustomMonoBehaviour.Overrider;
using ExtremeRoles.Module.SystemType.Roles;

using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory;

#nullable enable

namespace ExtremeRoles.GhostRoles.Impostor;

public sealed class Doppelganger : GhostRoleBase
{
	public enum Option
	{
		Range,
	}

	private FakerDummySystem.FakePlayer? fake;
	private ShapeshifterMinigame? minigamePrefab;
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
		var rolePlyaer = Player.GetPlayerControlById(rolePlayer);
		var targetPlyaer = Player.GetPlayerControlById(targetPlayer);

		var ghostRole = ExtremeGhostRoleManager.GetSafeCastedLocalPlayerRole<Doppelganger>();
		if (ghostRole is null)
		{
			return;
		}

		if (ghostRole.fake is null)
		{
			SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
			ghostRole.fake = new FakerDummySystem.FakePlayer(
				rolePlyaer, targetPlyaer,
				role.IsImpostor() || role.Id == ExtremeRoleId.Marlin);
		}
		else
		{
			ghostRole.fake.Clear();
		}
		ghostRole.target = null;
	}

	public override void CreateAbility()
	{
		bool isReportAbility = this.IsReportAbility();
		var behavior = new ChargingAndActivatingCountBehaviour(
			text: Tr.GetString($"{ability}Button"),
			img: FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.graphic.sprite,
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
			reduceTiming: ChargingAndActivatingCountBehaviour.ReduceTiming.OnActive,
			abilityOff: abilityOff,
			forceAbilityOff: abilityOff);

		behavior.ChargeTime = 1.0f;

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

	}

	protected override void CreateSpecificOption(OptionFactory factory)
	{
		factory.CreateFloatOption(
			Option.Range, 1.0f,
			0.2f, 3.0f, 0.1f);
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 2, 10);
	}

	protected override void UseAbility(RPCOperator.RpcCaller caller)
	{
		byte playerId = PlayerControl.LocalPlayer.PlayerId;
		caller.WriteByte(playerId);
		caller.WriteByte(this.target!.Value);
	}

	private bool openUI()
	{
		if (this.minigamePrefab == null)
		{
			var shapeShifterBase = FastDestroyableSingleton<RoleManager>.Instance.AllRoles.FirstOrDefault(
				x => x.Role is RoleTypes.Shapeshifter);
			if (!shapeShifterBase.IsTryCast<ShapeshifterRole>(out var shapeShifter))
			{
				return false;
			}
			this.minigamePrefab = Object.Instantiate(
				shapeShifter.ShapeshifterMenu,
				PlayerControl.LocalPlayer.transform);
			this.minigamePrefab.gameObject.SetActive(false);
		}

		var game = MinigameSystem.Open(this.minigamePrefab);
		var overider = game.gameObject.TryAddComponent<ShapeshifterMinigameShapeshiftOverride>();
		overider.Add(this.abilityCall);
		return true;
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

		return IsCommonUse() && (
			Minigame.Instance == null ||
			(isCharge && Minigame.Instance != null));
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
