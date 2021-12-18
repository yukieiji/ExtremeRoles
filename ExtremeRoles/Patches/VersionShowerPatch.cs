using System.Reflection;

using UnityEngine;

using HarmonyLib;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionShowerPatch
    {
        static void Postfix(VersionShower __instance)
        {
            var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
            if (amongUsLogo == null) return;

            var modTitle = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
            modTitle.transform.position = new Vector3(0, 0.3f, 0);
            modTitle.SetText($"Version:{Assembly.GetExecutingAssembly().GetName().Version}");
            modTitle.alignment = TMPro.TextAlignmentOptions.Center;
            modTitle.fontSize *= 1.5f;

            var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(modTitle);
            credentials.transform.position = new Vector3(0, -0.25f, 0);
            credentials.SetText($"Developed by yukieiji");
            credentials.alignment = TMPro.TextAlignmentOptions.Center;
            credentials.fontSize *= 0.85f;

            modTitle.transform.SetParent(amongUsLogo.transform);
            credentials.transform.SetParent(amongUsLogo.transform);
        }
    }
}
