using System.Collections.Generic;


using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.GhostRoles.API.Interface;

#nullable enable

namespace ExtremeRoles.GhostRoles.Impostor.SaboEvil;

public sealed class SaboEvilRole : GhostRoleBase
{

    public SaboEvilRole(IGhostRoleCoreProvider provider) : base(
        false,
		provider.Get(ExtremeGhostRoleId.SaboEvil))
    { }

    public static void ResetCool()
    {
        if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Sabotage, out var system) &&
			system.IsTryCast<SabotageSystemType>(out var sabSystem))
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

    protected override void OnMeetingEndHook()
    {
        return;
    }

    protected override void OnMeetingStartHook()
    {
        return;
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
