#if DEBUG
using HarmonyLib;

namespace ExtremeRoles.Patches.DebugPatch.RemoveAccount
{
    [HarmonyPatch(
        typeof(AchievementManager),
        nameof(AchievementManager.UpdateAchievementProgress))]
    public static class AchievementManagerUpdateAchievementProgressPatch
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(
        typeof(AchievementManager),
        nameof(AchievementManager.UnlockAchievement))]
    public static class AchievementManagerUnlockAchievementPatch
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}
#endif
