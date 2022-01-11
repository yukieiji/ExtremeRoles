using HarmonyLib;

namespace ExtremeRoles.Patches.MiniGame
{
    [HarmonyPatch(typeof(SwitchMinigame), nameof(SwitchMinigame.Begin))]
    public class SwitchMinigamePatch
    {
        public static bool Prefix(
            SwitchMinigame __instance)
        {
            return Roles.ExtremeRoleManager.GameRole[
                PlayerControl.LocalPlayer.PlayerId].CanUseSecurity;
        }
    }
}

