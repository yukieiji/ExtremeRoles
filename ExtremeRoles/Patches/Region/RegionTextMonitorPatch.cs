using System;
using System.Collections;

using HarmonyLib;

using TMPro;
using BepInEx.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Patches.Region
{
    [HarmonyPatch(typeof(RegionTextMonitor), nameof(RegionTextMonitor.Start))]
    internal class RegionTextMonitorStartPatch
    {
        public static bool Prefix(
            RegionMenu __instance)
        {
            __instance.StartCoroutine(
                GetWrapedRegionText(__instance).WrapToIl2Cpp());
            return false;
        }

        private static IEnumerator GetWrapedRegionText(RegionMenu __instance)
        {
            while (DestroyableSingleton<ServerManager>.Instance.CurrentRegion == null)
            {
                yield return null;
            }

            string printString;
            if (DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name == CustomServer.Id)
            {
                printString = Helper.Translation.GetString(
                    CustomServer.TranslationKey);
            }
            else
            {
                printString = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(
                    DestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName,
                    DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name,
                    Array.Empty<Il2CppSystem.Object>());
            }


            __instance.GetComponent<TextMeshPro>().text = printString;
            yield break;
        }

    }
}
