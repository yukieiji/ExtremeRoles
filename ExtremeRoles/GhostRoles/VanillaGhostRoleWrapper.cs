using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GhostRoles.API.Interface;

namespace ExtremeRoles.GhostRoles;

public sealed class VanillaGhostRoleVisual(GhostRoleCore core, RoleTypes roleId) : IGhostRoleVisual
{
	private readonly GhostRoleCore core = core;
	private readonly RoleTypes roleId = roleId;
	public string ColoredRoleName => DefaultGhostRoleVisual.GetDefaultColoredRoleName(core);

	public string ImportantText
	{
		get
		{
			string addText = roleId switch
			{
				RoleTypes.GuardianAngel or RoleTypes.CrewmateGhost =>
					Tr.GetString("crewImportantText"),
				RoleTypes.ImpostorGhost =>
					Tr.GetString("impImportantText"),
				_ => string.Empty,
			};
			return Helper.Design.ColoredString(
				this.core.Color,
				$"{this.ColoredRoleName}: {addText}");
		}
	}
}

public sealed class VanillaGhostRoleWrapper : GhostRoleBase
{
    private RoleTypes vanillaRoleId;

    public VanillaGhostRoleWrapper(
        RoleTypes vanillaRoleId) : base(
			vanillaRoleId is not RoleTypes.ImpostorGhost,
			createCore(vanillaRoleId))
    {
		this.vanillaRoleId = vanillaRoleId;
		this.Visual = new VanillaGhostRoleVisual(this.Core, this.vanillaRoleId);
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

    protected override void UseAbility(
        RPCOperator.RpcCaller caller)
    {
        throw new System.Exception("Don't call this class method!!");
    }
	private static GhostRoleCore createCore(RoleTypes vanillaRoleId)
		=> vanillaRoleId switch
		{ 
			RoleTypes.ImpostorGhost => new GhostRoleCore(vanillaRoleId.ToString(), ExtremeGhostRoleId.VanillaRole, Palette.ImpostorRed, ExtremeRoleType.Impostor),
			_ => new GhostRoleCore(vanillaRoleId.ToString(), ExtremeGhostRoleId.VanillaRole, Palette.White, ExtremeRoleType.Crewmate)
		};
}
