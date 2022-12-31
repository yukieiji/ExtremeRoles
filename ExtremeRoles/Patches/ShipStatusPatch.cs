using HarmonyLib;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatusAwakePatch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix(ShipStatus __instance)
        {
            CachedShipStatus.SetUp(__instance);
            ExtremeRolesPlugin.Compat.SetUpMap(__instance);   
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
            return ExtremeGameManager.Instance.Vison.TryComputeVison(
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
            ExtremeRolesPlugin.Compat.RemoveMap();
        }
    }
}
