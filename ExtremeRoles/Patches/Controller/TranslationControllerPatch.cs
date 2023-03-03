using System;
using HarmonyLib;

using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ExtremeRoles.Patches.Controller
{

    [HarmonyPatch(
        typeof(TranslationController),
        nameof(TranslationController.GetStringWithDefault),
        new Type[]
        { 
            typeof(StringNames),
            typeof(string),
            typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
        })]
    public static class TranslationControllerGetStringWithDefaultPatch
    {
        public static bool Prefix(
            ref string __result,
            [HarmonyArgument(0)] StringNames id,
            [HarmonyArgument(1)] string defaultStr)
        {
            if (id == StringNames.NoTranslation && 
                defaultStr.Equals("custom"))
            {
                __result = Helper.Translation.GetString(
                    "custom");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
