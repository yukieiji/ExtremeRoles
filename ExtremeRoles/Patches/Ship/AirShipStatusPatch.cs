
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using HarmonyLib;

namespace ExtremeRoles.Patches.Ship;

[HarmonyPatch(typeof(AirshipStatus), nameof(AirshipStatus.PrespawnStep))]
public static class AirshipStatusPrespawnStepPatch
{
    public static bool Prefix()
    {
        return !OnemanMeetingSystemManager.IsActive;
    }
}
