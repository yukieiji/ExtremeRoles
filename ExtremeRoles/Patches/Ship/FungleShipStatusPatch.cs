using HarmonyLib;

namespace ExtremeRoles.Patches.Ship;

[HarmonyPatch(typeof(FungleShipStatus), nameof(FungleShipStatus.OnEnable))]
public static class FungleShipStatusOnEnablePatch
{
	public static void Postfix(FungleShipStatus __instance)
	{
		ShipStatusOnEnablePatchBody.StaticPostfix(__instance);
	}
}

