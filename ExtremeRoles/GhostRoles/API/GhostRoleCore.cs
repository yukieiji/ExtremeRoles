using ExtremeRoles.Roles.API;
using UnityEngine;

namespace ExtremeRoles.GhostRoles.API;

public readonly record struct GhostRoleCore(
	string Name, ExtremeGhostRoleId Id,
	Color Color, ExtremeRoleType Team, OptionTab Tab=OptionTab.GeneralTab);