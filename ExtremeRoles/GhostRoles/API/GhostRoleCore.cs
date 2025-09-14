using ExtremeRoles.Roles.API;
using UnityEngine;

namespace ExtremeRoles.GhostRoles.API;

public sealed record GhostRoleCore(
	string Name, ExtremeGhostRoleId Id,
	Color Color, 
	OptionTab Tab=OptionTab.GeneralTab)
{
	public bool IsVanillaRole() => this.Id == ExtremeGhostRoleId.VanillaRole;
}