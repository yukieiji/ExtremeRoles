using HarmonyLib;

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
            if (__instance == null) { return true; }
            if (ExtremeRoleManager.GameRole.Count == 0 ||
                !ExtremeRolesPlugin.ShipState.IsRoleSetUpEnd) { return true; }

            var role = ExtremeRoleManager.GameRole[pc.PlayerId];

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
    [HarmonyPatch(typeof(Console), nameof(Console.Use))]
    public static class ConsoleUsePatch
    {
        public static bool Prefix(Console __instance)
        {

            if (__instance == null) { return true; }
            if (!ExtremeRolesPlugin.ShipState.IsRoleSetUpEnd) { return true; }
            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

            PlayerControl player = PlayerControl.LocalPlayer;
            PlayerTask task = __instance.FindTask(player);

            if (task == null) { return true; }

            switch (task.TaskType)
            {
                case TaskTypes.FixLights:
                case TaskTypes.FixComms:
                case TaskTypes.StopCharles:
                case TaskTypes.ResetSeismic:
                case TaskTypes.ResetReactor:
                case TaskTypes.RestoreOxy:
                    return ExtremeRoleManager.GameRole[
                        player.PlayerId].CanRepairSabotage();
                default:
                    return true;
            }
        }
    }
}
