using HarmonyLib;

namespace ExtremeRoles.Patches
{
	[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
	public static class HiddenTextPatch
	{
		private static void Postfix(TextBoxTMP __instance)
		{
			bool flag = OptionsHolder.JsonConfig.StreamerMode.Value && 
				(__instance.name == "GameIdText" || __instance.name == "IpTextBox" || __instance.name == "PortTextBox");
			if (flag)
			{
				__instance.outputText.text = new string('*', __instance.text.Length);
			}
		}
	}
}
