using HarmonyLib;

using ExtremeSkins.SkinManager;

namespace ExtremeSkins.Patches.AmongUs;

[HarmonyPatch(typeof(LanguageSetter), nameof(LanguageSetter.SetLanguage))]
public static class LanguageSetterPatch
{
    public static void Postfix()
    {
#if WITHHAT
        // ExtremeHatManager.UpdateTranslation();
#endif
#if WITHNAMEPLATE
        ExtremeNamePlateManager.UpdateTranslation();
#endif
#if WITHVISOR
        ExtremeVisorManager.UpdateTranslation();
#endif
    }
}
