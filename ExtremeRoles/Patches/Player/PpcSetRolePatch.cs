using AmongUs.GameOptions;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Patches.Player;


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
public static class PlayerControlPpcSetRolePatch
{
	public static void Prefix(
		PlayerControl __instance,
		[HarmonyArgument(0)] RoleTypes role,
		[HarmonyArgument(0)] bool canOverride)
	{
		if (RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return;
		}
		VanillaRoleAssignData.Instance.Add(__instance, role);
	}
}
