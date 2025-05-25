using System;
using HarmonyLib;

using Il2CppInterop.Runtime.InteropTypes.Arrays;

using static ExtremeRoles.Extension.Manager.IRegionInfoExtension;

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
            if (id is not StringNames.NoTranslation)
            {
                return true;
            }

			string key = defaultStr switch
			{
				FullCustomServerName => FullCustomServerName,
				ExROfficialServerTokyoManinName => ExROfficialServerTokyoManinName,
				_ => "",
			};

			if (string.IsNullOrEmpty(key))
			{
				return true;
			}

			__result = Tr.GetString(key);
			return false;
        }
    }
}
