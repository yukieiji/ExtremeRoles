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
            modTitle.transform.position = new Vector3(0, 4.0f, 0);
            modTitle.SetText($"Extreme Roles v{Assembly.GetExecutingAssembly().GetName().Version}");
            modTitle.alignment = TMPro.TextAlignmentOptions.Center;
            modTitle.fontSize *= 2.0f;

            var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(modTitle);
            credentials.transform.position = new Vector3(0, -0.25f, 0);
            credentials.SetText($"Developed by yukieiiji");
            credentials.alignment = TMPro.TextAlignmentOptions.Center;
            credentials.fontSize *= 0.75f;

            modTitle.transform.SetParent(amongUsLogo.transform);
            credentials.transform.SetParent(amongUsLogo.transform);
        }
    }
}
