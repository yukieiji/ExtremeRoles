using HarmonyLib;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.SystemType;

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
				 ExtremeRoleManager.TryGetRole(__instance.PlayerId, out var exiledPlayerRole)
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

		var role = ExtremeRoleManager.GetLocalPlayerRole();
		if (role is IRoleExilHook hookRole)
		{
			hookRole.HookExil(__instance);
		}
		if (role is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is IRoleExilHook multiHookRole)
		{
			multiHookRole.HookExil(__instance);
		}

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
