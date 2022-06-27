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
            if (amongUsLogo == null) { return; }

            var modTitle = Object.Instantiate(
                __instance.text, amongUsLogo.transform);
            modTitle.transform.localPosition = new Vector3(0, -2.1575f, 0);
            modTitle.transform.localScale = new Vector3(1.075f, 1.075f, 1.0f);
            modTitle.SetText(
                string.Concat(
                    Helper.Translation.GetString("version"),
                    Assembly.GetExecutingAssembly().GetName().Version));
            modTitle.alignment = TMPro.TextAlignmentOptions.Center;
            modTitle.fontSize *= 1.5f;

            var credentials = Object.Instantiate(
                modTitle, modTitle.transform);
            credentials.transform.localPosition = new Vector3(0, -0.5f, 0);
            credentials.transform.localScale = new Vector3(0.9f, 0.9f, 1.0f);
            credentials.SetText(
                string.Concat(
                    Helper.Translation.GetString("developer"),"yukieiji"));
            credentials.alignment = TMPro.TextAlignmentOptions.Center;
            credentials.fontSize *= 0.85f;
        }
    }
}
