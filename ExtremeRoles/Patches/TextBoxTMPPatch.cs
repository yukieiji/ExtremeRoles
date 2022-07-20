using HarmonyLib;

namespace ExtremeRoles.Patches
{
	[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
	public static class HiddenTextPatch
	{
		public static void Postfix(TextBoxTMP __instance)
		{
			bool flag = 
				SaveManager.StreamerMode && 
					(__instance.name == "ipTextBox" || 
					 __instance.name == "portTextBox");
			if (flag)
			{
				__instance.outputText.text = new string('*', __instance.text.Length);
			}
		}
	}
}
