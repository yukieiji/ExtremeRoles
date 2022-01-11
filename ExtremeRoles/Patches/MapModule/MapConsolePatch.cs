using HarmonyLib;

namespace ExtremeRoles.Patches.MiniGame
{
    [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.CanUse))]
    public class MapConsolePatch
    {
        public static bool Prefix(
            ref float __result, MapConsole __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo pc,
            [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            __result = float.MaxValue;

            if (Roles.ExtremeRoleManager.GameRole[
                PlayerControl.LocalPlayer.PlayerId].CanUseAdmin) { return true; }

            return false;
        }
    }
}