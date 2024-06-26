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

[HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.ChangeTab))]
public static class LobbyViewSettingsPaneChangeTabPatch
{
	public static void Prefix(LobbyViewSettingsPane __instance)
	{
		if (__instance.TryGetComponent<ExtremeLobbyViewSettingsTabView>(out var view))
		{
			view.ChangeTabPostfix();
		}
	}
}
