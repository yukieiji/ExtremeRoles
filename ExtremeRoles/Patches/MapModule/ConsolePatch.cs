using HarmonyLib;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;

namespace ExtremeRoles.Patches.MapModule
{
    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    public static class ConsoleCanUsePatch
    {
        public static bool Prefix(
            ref float __result, Console __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo pc,
            [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            __result = float.MaxValue;
            PlayerControl player = PlayerControl.LocalPlayer;
            PlayerTask task = __instance.FindTask(player);

            if (task == null) { return true; }
            if (__instance == null) { return true; }
            if (ExtremeRoleManager.GameRole.Count == 0 ||
                !RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

            TaskTypes taskType = task.TaskType;
            var role = ExtremeRoleManager.GameRole[pc.PlayerId];

            if (ExtremeRolesPlugin.Compat.IsModMap &&
                ExtremeRolesPlugin.Compat.ModMap.IsCustomSabotageTask(taskType))
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
}
