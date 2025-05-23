using ExtremeRoles.GameMode;
using HarmonyLib;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.UpdateSystem))]
public static class HeliSabotageSystemPatch
{
	public static void Postfix(HeliSabotageSystem __instance)
	{
		if (__instance.Countdown == 90.0f)
		{
			__instance.Countdown = ExtremeGameModeManager.Instance.ShipOption.Emergency.AirshipHeliTime;
		}
	}
}
