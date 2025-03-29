using System.Collections.Generic;


using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;

using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory;

#nullable enable

namespace ExtremeRoles.GhostRoles.Impostor;

public sealed class SaboEvil : GhostRoleBase
{

    public SaboEvil() : base(
        false,
        ExtremeRoleType.Impostor,
        ExtremeGhostRoleId.SaboEvil,
        ExtremeGhostRoleId.SaboEvil.ToString(),
        Palette.ImpostorRed)
    { }

    public static void ResetCool()
    {
        var sabSystem = CachedShipStatus.Systems[SystemTypes.Sabotage].TryCast<SabotageSystemType>();
        if (sabSystem != null)
        {
            sabSystem.Timer = 0.0f;
        }
    }

    public override void CreateAbility()
    {
        this.Button = GhostRoleAbilityFactory.CreateCountAbility(
            AbilityType.SaboEvilResetSabotageCool,
             HudManager.Instance.SabotageButton.graphic.sprite,
            this.IsReportAbility(),
            this.isPreCheck,
            this.isAbilityUse,
            this.UseAbility,
            abilityCall, true);
        this.ButtonInit();
    }

    public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

    public override void Initialize()
    { }

    protected override void OnMeetingEndHook()
    {
        return;
    }

    protected override void OnMeetingStartHook()
    {
        return;
    }

    protected override void CreateSpecificOption(OptionFactory factory)
    {
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 3, 20);
    }

    protected override void UseAbility(RPCOperator.RpcCaller caller)
    { }

    private bool isPreCheck() => IsCommonUse();

    private bool isAbilityUse() => IsCommonUse();

    private void abilityCall()
    {
        ResetCool();
    }
}
