using AmongUs.Data;

using HarmonyLib;

using Il2CppSystem.Collections.Generic;

using ExtremeRoles.Patches;

using ExtremeVoiceEngine.Translation;

namespace ExtremeVoiceEngine.Patches.ExtremeRoles;

[HarmonyPatch(typeof(LanguageUnitParseTSVPatch), nameof(LanguageUnitParseTSVPatch.Postfix))]
public static class LanguageUnitParseTSVPatchPatch
{
	public static void Postfix([HarmonyArgument(0)] ref Dictionary<string, string> allStrings)
	{
		TranslatorManager.AddTranslationData(
			DataManager.Settings.Language.CurrentLanguage, allStrings);
	}
}

