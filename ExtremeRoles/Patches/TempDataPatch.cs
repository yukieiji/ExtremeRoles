using ExtremeRoles.Module;
using HarmonyLib;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(TempData), nameof(TempData.OnGameEnd))]
public static class TempDataOnGameEndPatch
{
	public static void Postfix()
	{
		ExtremeGameResult.Instance.CreateTaskInfo();
	}
}
