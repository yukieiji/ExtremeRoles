using HarmonyLib;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
    class KillAnimationCoPerformKillPatch
    {
        public static bool HideNextAnimation = false;
        public static void Prefix(
            KillAnimation __instance,
            [HarmonyArgument(0)] ref PlayerControl source,
            [HarmonyArgument(1)] ref PlayerControl target)
        {
            if (HideNextAnimation)
            {
                source = target;
            }
            HideNextAnimation = false;
        }
    }
}
