using HarmonyLib;

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
            if (__instance.AllowImpostor) { return true; }
            if (Roles.ExtremeRoleManager.GameRole[pc.PlayerId].HasTask) { return true; }

            return false;
        }
    }
    [HarmonyPatch(typeof(Console), nameof(Console.Use))]
    public static class ConsoleUsePatch
    {
        public static bool Prefix(Console __instance)
        {

            if (__instance == null) { return true; }

            PlayerControl player = PlayerControl.LocalPlayer;

            switch (__instance.FindTask(player).TaskType)
            {
                case TaskTypes.FixLights:
                case TaskTypes.FixComms:
                case TaskTypes.StopCharles:
                case TaskTypes.ResetSeismic:
                case TaskTypes.ResetReactor:
                case TaskTypes.RestoreOxy:
                    return Roles.ExtremeRoleManager.GameRole[
                        player.PlayerId].CanRepairSabotage;
                default:
                    return true;
            }
        }
    }
}
