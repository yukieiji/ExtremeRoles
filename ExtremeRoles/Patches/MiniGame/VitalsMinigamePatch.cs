using HarmonyLib;

namespace ExtremeRoles.Patches.MiniGame
{
    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
    public class VitalsMinigamePatch
    {
        public static bool Prefix(
            VitalsMinigamePatch __instance)
        {
            return Roles.ExtremeRoleManager.GameRole[
                PlayerControl.LocalPlayer.PlayerId].CanUseVital;
        }
    }
}
