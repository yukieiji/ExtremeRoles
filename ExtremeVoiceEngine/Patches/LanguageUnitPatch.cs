using Il2CppSystem.Collections.Generic;
using HarmonyLib;

using AmongUs.Data;

using ExtremeVoiceEngine.Translation;

namespace ExtremeVoiceEngine.Patches;

[HarmonyPatch(typeof(LanguageUnit), nameof(LanguageUnit.ParseTSV))]
public static class LanguageUnitParseTSVPatch
{
    public static void Postfix(
        LanguageUnit __instance,
        [HarmonyArgument(0)] string tsvText,
        [HarmonyArgument(1)] ref Dictionary<string, string> allStrings)
    {
        TranslatorManager.AddTranslationData(
            DataManager.Settings.Language.CurrentLanguage, allStrings);
    }
}
