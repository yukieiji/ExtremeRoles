
using HarmonyLib;

using ExtremeRoles.Patches;

namespace ExtremeVoiceEngine.Patches;

[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
public static class VersionShowerStartPatch
{
    public static void Postfix(VersionShower __instance)
    {
		if (MainMenuTextInfoPatch.InfoText)
		{
			string stateStr = ExtremeVoiceEnginePlugin.Instance.ToString();
			string curStr = MainMenuTextInfoPatch.InfoText.text;
			MainMenuTextInfoPatch.InfoText.text =
				MainMenuTextInfoPatch.InfoText.text == string.Empty ?
				stateStr : $"{curStr}\n{stateStr}";
		}
	}
}
