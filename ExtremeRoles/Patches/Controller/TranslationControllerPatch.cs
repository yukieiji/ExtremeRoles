using System;
using HarmonyLib;

using Il2CppInterop.Runtime.InteropTypes.Arrays;

using static ExtremeRoles.Extension.Manager.ServerManagerExtension;

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
            if (id != StringNames.NoTranslation)
            { 
                return true;
            }
            else if (defaultStr.Equals(FullCustomServerName))
            {
                __result = Helper.Translation.GetString(FullCustomServerName);
                return false;
            }
            else if (defaultStr.Equals(ExROfficialServerTokyoManinName))
            {
                __result = Helper.Translation.GetString(ExROfficialServerTokyoManinName);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
