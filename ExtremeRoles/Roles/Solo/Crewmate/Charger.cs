using System;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.State;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Charger :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleAwake<RoleTypes>
{
	public enum ChargerOption
	{
		AwakeTaskGage,
		ChargeRange,
		AddAbilityCount,
		RestoreKillCooldown,
	}

	public bool IsAwake
	{
		get
		{
			return GameSystem.IsLobby || this.awakeRole;
		}
	}

	public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

	public string GetFakeOptionString() => "";

	public ExtremeAbilityButton? Button { get; set; }

	private bool awakeRole = false;
	private float awakeTaskGage;
	private float chargeRange;
	private int addAbilityCount;
	private bool restoreKillCooldown;
	private bool awakeHasOtherVision;
	private PlayerControl? tmpPlayer;
	private PlayerControl? currentTargetPlayer;

	public Charger() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Charger,
			ColorPalette.ChargerElectricYellow))
	{ }

	public static void Charged(byte chargerId, byte targetId)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer != null &&
			localPlayer.PlayerId == targetId &&
			Player.TryGetPlayerControl(targetId, out var targetPlayer) &&
			targetPlayer.IsAlive() &&
			ExtremeRoleManager.TryGetSafeCastedRole<Charger>(chargerId, out var chargerRole))
		{
			ApplyChargeEffect(
				chargerRole.addAbilityCount,
				chargerRole.restoreKillCooldown);
		}
	}

	public static void ApplyChargeEffect(int addCount, bool restoreKillCool)
	{
		var (ability1, ability2) = ExtremeRoleManager.GetInterfaceCastedLocalRole<IRoleAbility>();

		if (ability1?.Button != null)
		{
			ability1.Button.SetCooldownTimer(0.1f);
			if (ability1.Button.Behavior is ICountBehavior countBehavior)
			{
				countBehavior.SetAbilityCount(countBehavior.AbilityCount + addCount);
			}
		}

		if (ability2?.Button != null)
		{
			ability2.Button.SetCooldownTimer(0.1f);
			if (ability2.Button.Behavior is ICountBehavior countBehavior)
			{
				countBehavior.SetAbilityCount(countBehavior.AbilityCount + addCount);
			}
		}

		var localRole = ExtremeRoleManager.GetLocalPlayerRole();
		if (restoreKillCool && localRole.CanKill())
		{
			PlayerControl.LocalPlayer.killTimer = 0.1f;
		}
		// SEを後で付ける
	}

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"charge",
			UnityObjectLoader.LoadSpriteFromResources(ObjectPath.OverLoaderOverLoad),
			checkAbility: CheckAbility,
			abilityOff: CleanUp,
			forceAbilityOff: () => { },
			isReduceOnActive: true);
		this.Button?.SetLabelToCrewmate();
	}

	public bool IsAbilityUse()
		=> this.IsAwake &&
			IRoleAbility.IsCommonUse() &&
			Player.TryGetClosestPlayerInRange(this, this.chargeRange, out this.tmpPlayer);

	public bool UseAbility()
	{
		this.currentTargetPlayer = this.tmpPlayer;
		return true;
	}

	public bool CheckAbility()
		=> this.currentTargetPlayer.IsAlive() &&
			Player.IsPlayerInRangeAndDrawOutLine(
				PlayerControl.LocalPlayer,
				this.currentTargetPlayer,
				this,
				this.chargeRange);

	public void CleanUp()
	{
		if (this.currentTargetPlayer != null && PlayerControl.LocalPlayer != null)
		{
			byte chargerId = PlayerControl.LocalPlayer.PlayerId;
			byte targetId = this.currentTargetPlayer.PlayerId;

			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.ChargerCharge))
			{
				caller.WriteByte(chargerId);
				caller.WriteByte(targetId);
			}
		}
		ResetTarget();
	}

	private void ResetTarget()
	{
		this.currentTargetPlayer = null;
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (this.awakeRole)
		{
			return;
		}

		this.Button?.SetButtonShow(false);

		if (Player.GetPlayerTaskGage(rolePlayer) < this.awakeTaskGage)
		{
			return;
		}

		this.awakeRole = true;
		this.HasOtherVision = this.awakeHasOtherVision;
		this.Button?.SetButtonShow(true);
	}

	public void ResetOnMeetingStart()
	{
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public override string GetColoredRoleName(bool isTruthColor = false)
	{
		if (isTruthColor || IsAwake)
		{
			return base.GetColoredRoleName();
		}
		else
		{
			return Design.ColoredString(
				Palette.White, Tr.GetString(RoleTypes.Crewmate.ToString()));
		}
	}

	public override string GetFullDescription()
	{
		if (IsAwake)
		{
			return Tr.GetString($"{this.Core.Id}FullDescription");
		}
		else
		{
			return Tr.GetString($"{RoleTypes.Crewmate}FullDescription");
		}
	}

	public override string GetImportantText(bool isContainFakeTask = true)
	{
		if (IsAwake)
		{
			return base.GetImportantText(isContainFakeTask);
		}
		else
		{
			return Design.ColoredString(
				Palette.White,
				$"{this.GetColoredRoleName()}: {Tr.GetString("crewImportantText")}");
		}
	}

	public override string GetIntroDescription()
	{
		if (IsAwake)
		{
			return base.GetIntroDescription();
		}
		else
		{
			return Design.ColoredString(
				Palette.CrewmateBlue,
				PlayerControl.LocalPlayer.Data.Role.Blurb);
		}
	}

	public override Color GetNameColor(bool isTruthColor = false)
	{
		if (isTruthColor || IsAwake)
		{
			return base.GetNameColor(isTruthColor);
		}
		else
		{
			return Palette.White;
		}
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateIntOption(
			ChargerOption.AwakeTaskGage,
			50, 0, 100, 10,
			format: OptionUnit.Percentage);

		IRoleAbility.CreateAbilityCountOption(
			factory,
			defaultAbilityCount: 3,
			maxAbilityCount: 10,
			defaultActiveTime: 3.0f);

		factory.CreateFloatOption(
			ChargerOption.ChargeRange,
			0.75f, 0.25f, 3.5f, 0.25f);

		factory.CreateIntOption(
			ChargerOption.AddAbilityCount,
			1, 1, 10, 1,
			format: OptionUnit.Shot);

		factory.CreateBoolOption(
			ChargerOption.RestoreKillCooldown,
			false);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.awakeTaskGage = loader.GetValue<ChargerOption, int>(ChargerOption.AwakeTaskGage) / 100.0f;
		this.chargeRange = loader.GetValue<ChargerOption, float>(ChargerOption.ChargeRange);
		this.addAbilityCount = loader.GetValue<ChargerOption, int>(ChargerOption.AddAbilityCount);
		this.restoreKillCooldown = loader.GetValue<ChargerOption, bool>(ChargerOption.RestoreKillCooldown);

		this.awakeHasOtherVision = this.HasOtherVision;

		if (this.awakeTaskGage <= 0.0f)
		{
			this.awakeRole = true;
			this.HasOtherVision = this.awakeHasOtherVision;
		}
		else
		{
			this.awakeRole = false;
			this.HasOtherVision = false;
		}
	}
}
