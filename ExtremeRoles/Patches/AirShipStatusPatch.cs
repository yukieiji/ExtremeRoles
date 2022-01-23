using HarmonyLib;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(AirshipStatus), nameof(AirshipStatus.PrespawnStep))]
    public static class AirshipStatusPrespawnStepPatch
    {
        public static bool Prefix(AirshipStatus __instance)
        {
            return !ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger;
        }
	}
}
