using System;
using HarmonyLib;
using UnhollowerBaseLib;


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
    public class TranslationControllerGetStringWithDefaultPatch
    {
        public static bool Prefix(
            ref string __result,
            [HarmonyArgument(0)] StringNames id,
            [HarmonyArgument(1)] string defaultStr)
        {
            if (id == StringNames.NoTranslation && 
                defaultStr.Equals(Region.CustomServer.Id))
            {
                __result = Helper.Translation.GetString(
                    Region.CustomServer.TranslationKey);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
