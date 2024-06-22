using ExtremeRoles.Module.CustomMonoBehaviour.View;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Patches.Option;

[HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Awake))]
public static class LobbyViewSettingsPanePatch
{
	public static void Postfix(LobbyViewSettingsPane __instance)
	{
		__instance.gameObject.AddComponent<ExtremeLobbyViewSettingsTabView>();
	}
}
