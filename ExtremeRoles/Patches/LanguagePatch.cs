using HarmonyLib;

#nullable enable

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(LanguageSetter), nameof(LanguageSetter.SetLanguage))]
public static class SetLanguagepPatch
{
    public static void Postfix()
    {
		Compat.CompatModMenu.UpdateTranslation();
	}
}
