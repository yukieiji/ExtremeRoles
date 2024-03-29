﻿using HarmonyLib;

using ExtremeRoles.Module;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
public static class PingTrackerUpdatePatch
{
    public static void Postfix(PingTracker __instance)
    {
		StatusTextShower.Instance.RebuildPingString(__instance);
    }
}
