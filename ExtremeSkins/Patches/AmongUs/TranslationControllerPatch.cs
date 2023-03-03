using HarmonyLib;

using Il2CppInterop.Runtime.InteropTypes.Arrays;

using ExtremeSkins.SkinManager;

namespace ExtremeSkins.Patches.AmongUs
{
    [HarmonyPatch(
        typeof(TranslationController), 
        nameof(TranslationController.GetString),
        new[]
        {
                typeof(StringNames),
                typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
        })]
    public static class ColorStringPatch
    {
        public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
        {
            if ((int)name >= 50000)
            {
                __result = Helper.Translation.GetString(
                    ExtremeColorManager.LangData[name]);
                return false;
            }
            return true;
        }
    }
}
