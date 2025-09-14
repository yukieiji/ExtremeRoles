using ExtremeRoles.Roles.API;
using UnityEngine;

namespace ExtremeRoles.GhostRoles.API;

public sealed record GhostRoleCore(
	string Name, ExtremeGhostRoleId Id,
	Color Color, 
	ExtremeRoleType DefaultTeam, 
	OptionTab Tab=OptionTab.GeneralTab)
{
	public bool IsVanillaRole() => this.Id == ExtremeGhostRoleId.VanillaRole;
}