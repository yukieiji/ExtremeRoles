using HarmonyLib;

using ExtremeRoles.Patches.Option;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(LanguageSetter), nameof(LanguageSetter.SetLanguage))]
    public static class SetLanguagepPatch
    {
        public static void Postfix()
        {
            OptionsMenuBehaviourStartPatch.UpdateMenuTranslation();
            Compat.CompatModMenu.UpdateTranslation();
        }
    }
}
