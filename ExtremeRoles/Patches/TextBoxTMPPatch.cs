using HarmonyLib;

using AmongUs.Data;

namespace ExtremeRoles.Patches
{
	[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
	public static class HiddenTextPatch
	{
		public static void Postfix(TextBoxTMP __instance)
		{
			if (DataManager.Settings.Gameplay.StreamerMode &&
				(__instance.name == "ipTextBox" ||
				 __instance.name == "portTextBox"))
			{
				__instance.outputText.text = new string('*', __instance.text.Length);
			}
		}
	}
}
