using System;

using ExtremeRoles.GameMode;

using HarmonyLib;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
public static class ConstantsShouldHorseAroundPatch
{
    // 一応隠しておく、エイプリルフール終わったあと消す
    public static bool IsAprilFoolEnd
    {
        get
        {
            try
            {
                DateTime utcNow = DateTime.UtcNow;
                DateTime endTime = new DateTime(
                    utcNow.Year, 4, 3, 6, 59, 0, 0, DateTimeKind.Utc);
                if (utcNow > endTime)
                {
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }
    }

    public static bool Prefix(ref bool __result)
    {
        if (!IsAprilFoolEnd)
        {
            return true;
        }
        __result =
            ExtremeGameModeManager.Instance is not null &&
            ExtremeGameModeManager.Instance.ShipOption.EnableHorseMode;
        return false;
    }
}
