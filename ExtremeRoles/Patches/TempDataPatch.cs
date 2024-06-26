using ExtremeRoles.Module;
using HarmonyLib;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(GameData), nameof(GameData.OnGameEnd))]
public static class GameDataOnGameEndPatch
{
	public static void Prefix()
	{
		ExtremeGameResult.Instance.CreateTaskInfo();
	}
}
