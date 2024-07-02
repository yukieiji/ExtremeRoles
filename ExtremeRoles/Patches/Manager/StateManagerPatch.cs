using HarmonyLib;

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
public static class StatsManagerAmBannedPatch
{
    public static void Postfix(out bool __result)
    {
        __result = false;
    }
}
