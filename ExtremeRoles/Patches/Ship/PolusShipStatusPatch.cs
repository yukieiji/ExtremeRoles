using HarmonyLib;

namespace ExtremeRoles.Patches.Ship;

[HarmonyPatch(typeof(PolusShipStatus), nameof(PolusShipStatus.OnEnable))]
public static class PolusShipStatusOnEnablePatch
{
	public static void Postfix(PolusShipStatus __instance)
	{
		ShipStatusOnEnablePatchBody.StaticPostfix(__instance);
	}
}

