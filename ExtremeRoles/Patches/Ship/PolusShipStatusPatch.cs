using HarmonyLib;

using ExtremeRoles.GameMode;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(PolusShipStatus), nameof(PolusShipStatus.OnEnable))]
public static class PolusShipStatusOnEnablePatch
{
	public static void Postfix(PolusShipStatus __instance)
	{
		ExtremeGameModeManager.Instance.ShipOption.Emergency.ChangeTime(__instance);
	}
}

