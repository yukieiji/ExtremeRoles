using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Extension.Ship;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;



#nullable enable

using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory;
using ExtremeRoles.Extension.VentModule;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;

namespace ExtremeRoles.GhostRoles.Impostor;

public sealed class Ventgeist : GhostRoleBase
{
    public enum Option
    {
        Range,
    }

    private float range;
    private Vent? targetVent;

    public Ventgeist() : base(
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

    protected override void CreateSpecificOption(AutoParentSetBuilder factory)
    {
		factory.CreateFloatOption(
            Option.Range, 1.0f,
            0.2f, 3.0f, 0.1f);
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 2, 10);
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
