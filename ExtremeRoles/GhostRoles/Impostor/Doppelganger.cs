using System.Collections.Generic;

using ExtremeRoles.Helper;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;

using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.SystemType.Roles;


#nullable enable

using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory;

namespace ExtremeRoles.GhostRoles.Impostor;

public sealed class Doppelganger : GhostRoleBase
{
    public enum Option
    {
        Range,
    }

	private FakerDummySystem.FakePlayer? fake;

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
	}

    public override void CreateAbility()
    {
        this.Button = GhostRoleAbilityFactory.CreateCountAbility(
            AbilityType.VentgeistVentAnime,
            FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.graphic.sprite,
            this.isReportAbility(),
            this.isPreCheck,
            this.isAbilityUse,
            this.UseAbility,
            abilityCall, true);
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

    }

    private bool isPreCheck() => true;

    private bool isAbilityUse()
    {
		return IsCommonUse();
    }
    private void abilityCall()
    {

    }
}
