using HarmonyLib;
using TMPro;

using ExtremeRoles.Patches;

namespace ExtremeSkins.Patches.AmongUs;

[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
public static class VersionShowerStartPatch
{
	private static string defaultStr = string.Empty;
    public static void Postfix()
    {
		if (MainMenuTextInfoPatch.InfoText)
		{
			defaultStr = MainMenuTextInfoPatch.InfoText.text;
			UpdateText();
		}
    }
    public static void UpdateText()
    {
		if (MainMenuTextInfoPatch.InfoText)
		{
			string stateString = CreatorModeManager.Instance.StatusString;
			MainMenuTextInfoPatch.InfoText.text =
				defaultStr == string.Empty ?
				stateString : $"{defaultStr}\n{stateString}";
		}
    }
}
