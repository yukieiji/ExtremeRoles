using HarmonyLib;

using ExtremeRoles.Module.CustomMonoBehaviour.View;

namespace ExtremeRoles.Patches.Option;

[HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Awake))]
public static class LobbyViewSettingsPanePatch
{
	public static void Postfix(LobbyViewSettingsPane __instance)
	{
		__instance.gameObject.AddComponent<ExtremeLobbyViewSettingsTabView>();
	}
}
