using HarmonyLib;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Compat;

namespace ExtremeRoles.Patches.MapModule;

[HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
public static class ConsoleCanUsePatch
{
    public static bool Prefix(
        ref float __result, Console __instance,
        [HarmonyArgument(0)] NetworkedPlayerInfo pc,
        [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
    {
        canUse = couldUse = false;
        __result = float.MaxValue;

		PlayerControl player = PlayerControl.LocalPlayer;
		if (player == null || __instance == null)
		{
			return true;
		}

        PlayerTask task = __instance.FindTask(player);
        if (task == null ||
			!RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return true;
		}

        if (!ExtremeRoleManager.TryGetRole(pc.PlayerId, out var role))
        {
            return false;
        }

		var taskType = task.TaskType;

		if (CompatModManager.Instance.TryGetModMap(out var modMap) &&
			modMap.IsCustomSabotageTask(taskType))
        {
            return role.CanRepairSabotage();
        }

        switch (taskType)
        {
            case TaskTypes.FixLights:
            case TaskTypes.FixComms:
            case TaskTypes.StopCharles:
            case TaskTypes.ResetSeismic:
            case TaskTypes.ResetReactor:
            case TaskTypes.RestoreOxy:
                return role.CanRepairSabotage();
            default:
                if (role.HasTask())
                {
                    if (role.IsImpostor())
                    {
                        __instance.AllowImpostor = true;
                    }
                    return true;
                }
                return false;
        }
    }
}
