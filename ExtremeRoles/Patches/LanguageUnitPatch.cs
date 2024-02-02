using Il2CppSystem.Collections.Generic;
using HarmonyLib;

using AmongUs.Data;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(LanguageUnit), nameof(LanguageUnit.ParseTSV))]
public static class LanguageUnitParseTSVPatch
{
    public static void Postfix(
        [HarmonyArgument(1)] ref Dictionary<string, string> allStrings)
    {
		Beta.BetaContentManager.AddContentText(
			DataManager.Settings.Language.CurrentLanguage, allStrings);
    }
}
