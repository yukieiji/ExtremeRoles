using Il2CppSystem.Collections.Generic;
using HarmonyLib;

using AmongUs.Data;
using ExtremeRoles.Translation;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(LanguageUnit), nameof(LanguageUnit.ParseTSV))]
public static class LanguageUnitParseTSVPatch
{
    public static void Postfix(
        [HarmonyArgument(1)] ref Dictionary<string, string> allStrings)
    {
		var lang = DataManager.Settings.Language.CurrentLanguage;
		TranslatorManager.AddTranslationData(lang, allStrings);
	}
}
