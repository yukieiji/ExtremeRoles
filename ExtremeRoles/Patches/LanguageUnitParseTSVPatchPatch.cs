using AmongUs.Data;

using HarmonyLib;

using Il2CppSystem.Collections.Generic;

using ExtremeRoles.Translation;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(LanguageUnitParseTSVPatch), nameof(LanguageUnitParseTSVPatch.Postfix))]
public static class LanguageUnitParseTSVPatchPatch
{
	public static void Postfix([HarmonyArgument(0)] ref Dictionary<string, string> allStrings)
	{
		TranslatorManager.AddTranslationData(
			DataManager.Settings.Language.CurrentLanguage, allStrings);
	}
}

