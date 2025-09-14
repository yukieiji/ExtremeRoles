using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

using ExtremeRoles.Extension.VentModule;
using ExtremeRoles.Module.Ability.Factory;

#nullable enable

namespace ExtremeRoles.GhostRoles.Impostor.Ventgeist;

public sealed class VentgeistRole : GhostRoleBase
{
    public enum Option
    {
        Range,
    }

    private float range;
    private Vent? targetVent;

    public VentgeistRole() : base(
        false,
        ExtremeRoleType.Impostor,
        ExtremeGhostRoleId.Ventgeist,
        ExtremeGhostRoleId.Ventgeist.ToString(),
        Palette.ImpostorRed)
    { }

    public static void VentAnime(int ventId)
    {
        RPCOperator.StartVentAnimation(ventId);
    }

    public override void CreateAbility()
    {
        this.Button = GhostRoleAbilityFactory.CreateCountAbility(
            AbilityType.VentgeistVentAnime,
            HudManager.Instance.ImpostorVentButton.graphic.sprite,
            this.IsReportAbility(),
            this.isPreCheck,
            this.isAbilityUse,
            this.UseAbility,
            abilityCall, true);
        this.ButtonInit();
    }

    public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

    public override void Initialize()
    {
        this.range = this.Loader.GetValue<Option, float>(Option.Range);
    }

    protected override void OnMeetingEndHook()
    {
        return;
    }

    protected override void OnMeetingStartHook()
    {
        this.targetVent = null;
    }

    protected override void UseAbility(RPCOperator.RpcCaller caller)
    {
        caller.WriteInt(targetVent!.Id);
    }

    private bool isPreCheck() => this.targetVent != null;

    private bool isAbilityUse()
    {
        this.targetVent = null;

        ShipStatus ship = ShipStatus.Instance;

        if (ship == null ||
            !ship.enabled) { return false; }

        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();

        foreach (Vent vent in ship.AllVents)
        {
            if (vent.IsModed() && !vent.gameObject.active)
            {
                continue;
            }
            float distance = Vector2.Distance(vent.transform.position, truePosition);
            if (distance <= this.range)
            {
                this.targetVent = vent;
                break;
            }
        }

        return IsCommonUse() && this.targetVent != null;
    }
    private void abilityCall()
    {
        RPCOperator.StartVentAnimation(this.targetVent!.Id);
        this.targetVent = null;
    }
}
