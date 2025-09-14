using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.GhostRoles;

public sealed class VanillaGhostRoleWrapper : GhostRoleBase
{
    private RoleTypes vanillaRoleId;

    public VanillaGhostRoleWrapper(
        RoleTypes vanillaRoleId) : base(
			vanillaRoleId is not RoleTypes.ImpostorGhost,
			vanillaRoleId is RoleTypes.ImpostorGhost ? ExtremeRoleType.Impostor : ExtremeRoleType.Crewmate,
			createCore(vanillaRoleId))
    {
		this.vanillaRoleId = vanillaRoleId;
    }

    public override string GetImportantText()
    {
        string addText = this.vanillaRoleId switch
        {
            RoleTypes.GuardianAngel or RoleTypes.CrewmateGhost =>
                Tr.GetString("crewImportantText"),
            RoleTypes.ImpostorGhost =>
                Tr.GetString("impImportantText"),
            _ => string.Empty,
        };
        return Helper.Design.ColoredString(
            this.Core.Color,
            $"{this.GetColoredRoleName()}: {addText}");
    }

    public override string GetFullDescription()
    {
        return Tr.GetString(
            $"{this.vanillaRoleId}FullDescription");
    }


    public override HashSet<Roles.ExtremeRoleId> GetRoleFilter() => new HashSet<Roles.ExtremeRoleId> ();

    protected override void OnMeetingEndHook()
    {
        return;
    }

    protected override void OnMeetingStartHook()
    {
        return;
    }

    public override void CreateAbility()
    {
        throw new System.Exception("Don't call this class method!!");
    }

    public override void Initialize()
    {
        return;
    }

    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
    {
        throw new System.Exception("Don't call this class method!!");
    }

    protected override void UseAbility(
        RPCOperator.RpcCaller caller)
    {
        throw new System.Exception("Don't call this class method!!");
    }
	private static GhostRoleCore createCore(RoleTypes vanillaRoleId)
		=> vanillaRoleId switch
		{ 
			RoleTypes.ImpostorGhost => new GhostRoleCore(vanillaRoleId.ToString(), ExtremeGhostRoleId.VanillaRole, Palette.ImpostorRed),
			_ => new GhostRoleCore(vanillaRoleId.ToString(), ExtremeGhostRoleId.VanillaRole, Palette.White)
		};
}
