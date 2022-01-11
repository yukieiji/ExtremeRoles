using HarmonyLib;

namespace ExtremeRoles.Patches.MiniGame
{
    [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.CanUse))]
    public class MapConsolePatch
    {
        public static bool Prefix(
            VitalsMinigamePatch __instance)
        {
            return Roles.ExtremeRoleManager.GameRole[
                PlayerControl.LocalPlayer.PlayerId].CanUseAdmin;
        }
    }
}