using Il2CppSystem.Collections.Generic;
using HarmonyLib;

using ExtremeVoiceEngine.Translation;

namespace ExtremeVoiceEngine.Patches;

[HarmonyPatch(typeof(LanguageUnit), nameof(LanguageUnit.ParseTSV))]
public static class LanguageUnitParseTSVPatch
{
    public static void Postfix(
        LanguageUnit __instance,
        [HarmonyArgument(0)] string tsvText,
        [HarmonyArgument(1)] ref Dictionary<string, string> allStrings,
        [HarmonyArgument(2)] ref Dictionary<StringNames, QuickChatSentenceVariantSet> allQuickChatVariantSets)
    {
        TranslatorManager.AddTranslationData(__instance.languageID, allStrings);
    }
}
