using HarmonyLib;

using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;

namespace ExtremeRoles.Patches.Player;

#nullable enable

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
public static class PlayerControlExiledPatch
{
	public static void Postfix(
		PlayerControl __instance)
	{
		if (!(
				GameProgressSystem.IsGameNow &&
				 ExtremeRoleManager.TryGetRole(__instance.PlayerId, out var exiledPlayerRole) &&
				 __instance.Data.IsDead
			))
		{
			return;
		}

		if (ExtremeRoleManager.IsDisableWinCheckRole(exiledPlayerRole))
		{
			ExtremeRolesPlugin.ShipState.SetDisableWinCheck(true);
		}

		ExtremeRolesPlugin.ShipState.AddDeadInfo(
			__instance, DeathReason.Exile, null);

		exiledPlayerRole.ExiledAction(__instance);
		if (exiledPlayerRole is MultiAssignRoleBase multiAssignExiledPlayerRole)
		{
			multiAssignExiledPlayerRole.AnotherRole?.ExiledAction(__instance);
		}

		if (!exiledPlayerRole.HasTask())
		{
			__instance.ClearTasks();
		}

		ExtremeRolesPlugin.ShipState.SetDisableWinCheck(false);
	}
}
