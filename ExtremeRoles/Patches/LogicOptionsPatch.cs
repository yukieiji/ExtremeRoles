using HarmonyLib;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(LogicOptions), nameof(LogicOptions.SyncOptions))]
public static class LogicOptionsSyncOptionsPatch
{
    public static bool Prefix()
    {
		if (AmongUsClient.Instance == null ||
			!AmongUsClient.Instance.AmHost ||
			GameManager.Instance == null ||
			GameManager.Instance.LogicOptions == null)
		{
			return false;
		}
		var syncer = GameManager.Instance.gameObject.TryAddComponent<LazyOptionSyncer>();
		syncer.SyncOption();

		return false;
	}
}
