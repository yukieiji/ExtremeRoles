using ExtremeRoles.Roles.API;
using UnityEngine;

namespace ExtremeRoles.GhostRoles.API;

public sealed record GhostRoleCore(
	string Name, ExtremeGhostRoleId Id,
	Color Color, 
	ExtremeRoleType DefaultTeam, 
	OptionTab Tab = OptionTab.GeneralTab)
{
	public bool IsVanillaRole() => this.Id == ExtremeGhostRoleId.VanillaRole;

	public static GhostRoleCore CreateCrewmate(ExtremeGhostRoleId id, Color color)
		=> new GhostRoleCore(id.ToString(), id, color, ExtremeRoleType.Crewmate, OptionTab.GhostCrewmateTab);

	public static GhostRoleCore CreateNeutral(ExtremeGhostRoleId id, Color color)
		=> new GhostRoleCore(id.ToString(), id, color, ExtremeRoleType.Neutral, OptionTab.GhostNeutralTab);
	public static GhostRoleCore CreateImpostor(ExtremeGhostRoleId id)
		=> new GhostRoleCore(id.ToString(), id, Palette.ImpostorRed, ExtremeRoleType.Impostor, OptionTab.GhostImpostorTab);
}