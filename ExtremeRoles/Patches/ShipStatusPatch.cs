using HarmonyLib;

using ExtremeRoles.Compat;
using ExtremeRoles.Performance;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
public static class ShipStatusAwakePatch
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void Postfix(ShipStatus __instance)
    {
		CachedShipStatus.SetUp(__instance);
        CompatModManager.Instance.SetUpMap(__instance);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
public static class ShipStatusCalculateLightRadiusPatch
{
    public static bool Prefix(
        ref float __result,
        ShipStatus __instance,
        [HarmonyArgument(0)] GameData.PlayerInfo playerInfo)
    {
        return VisionComputer.Instance.IsVanillaVisionAndGetVision(
            __instance, playerInfo, out __result);
    }

}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
public static class ShipStatusOnDestroyPatch
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void Postfix()
    {
        CachedShipStatus.Destroy();
		CompatModManager.Instance.RemoveMap();
		ExtremeSystemTypeManager.Instance.Reset();
	}
}
