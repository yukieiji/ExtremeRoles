using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate.Invisible;

public sealed class InvisibleRole : SingleRoleBase, IRoleAutoBuildAbility, IRoleSpecialReset
{
	public enum Option
	{
		SpeedMod,
		HasHitbox,
	}

	public override IStatusModel? Status => this.status;
	public ExtremeAbilityButton? Button { get; set; }

	private InvisibleStatusModel? status;
	private InvisibleAbilityHandler? ability;

	public InvisibleRole() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Invisible,
			ColorPalette.InvisibleWhite))
	{
	}

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"invisibleAbility",
			UnityObjectLoader.LoadSpriteFromResources(ObjectPath.TestButton),
			abilityOff: CleanUp,
			forceAbilityOff: CleanUp,
			isReduceOnActive: true);
		this.Button?.SetLabelToCrewmate();
	}

	public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse();

	public bool UseAbility()
	{
		var local = PlayerControl.LocalPlayer;
		if (local == null)
		{
			return false;
		}

		return this.ability?.Execute(local.PlayerId) ?? false;
	}

	public void CleanUp()
	{
		var local = PlayerControl.LocalPlayer;
		if (local == null)
		{
			return;
		}

		this.ability?.CleanUp(local.PlayerId);
	}

	public void ResetOnMeetingStart()
	{
		var local = PlayerControl.LocalPlayer;
		if (local != null)
		{
			this.ability?.CleanUp(local.PlayerId);
		}
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void AllReset(PlayerControl rolePlayer)
	{
		if (rolePlayer != null)
		{
			this.ability?.CleanUp(rolePlayer.PlayerId);
		}
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(
			factory,
			defaultAbilityCount: 3,
			maxAbilityCount: 10,
			defaultActiveTime: 10.0f);

		factory.CreateFloatOption(
			Option.SpeedMod,
			1.0f, 0.1f, 3.0f, 0.1f);

		factory.CreateBoolOption(
			Option.HasHitbox,
			true);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		float speedMod = loader.GetValue<Option, float>(Option.SpeedMod);
		bool hasHitbox = loader.GetValue<Option, bool>(Option.HasHitbox);

		var invisibleSystem = InvisibleSystem.Get();
		var speedSystem = PlayerSpeedSystem.Get();

		this.status = new InvisibleStatusModel(speedMod, hasHitbox);
		this.ability = new InvisibleAbilityHandler(invisibleSystem, speedSystem, this.status);
		AbilityClass = this.ability;
	}
}
