using ExtremeRoles.GameMode;
using HarmonyLib;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(FungleShipStatus), nameof(FungleShipStatus.OnEnable))]
public static class FungleShipStatusOnEnablePatch
{
	public static void Postfix(FungleShipStatus __instance)
	{
		ExtremeGameModeManager.Instance.ShipOption.Emergency.ChangeTime(__instance);
	}
}

