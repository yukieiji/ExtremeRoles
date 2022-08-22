using System.Reflection;

using UnityEngine;

using HarmonyLib;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionShowerPatch
    {
        public static void Postfix(VersionShower __instance)
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
            credentials.SetText(
                string.Concat(
                    Helper.Translation.GetString("developer"),"yukieiji"));
            credentials.alignment = TMPro.TextAlignmentOptions.Center;
            credentials.fontSize *= 0.85f;

            var translator = Object.Instantiate(
                credentials, credentials.transform);
            translator.transform.localPosition = new Vector3(0, -0.35f, 0);
            translator.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            if ((SupportedLangs)SaveManager.LastLanguage != SupportedLangs.Japanese)
            {
                translator.gameObject.SetActive(true);
                translator.SetText(
                    string.Concat(
                        Helper.Translation.GetString("langTranslate"),
                        Helper.Translation.GetString("translatorMember")));
                translator.alignment = TMPro.TextAlignmentOptions.Center;
                translator.fontSize *= 0.85f;

                credentials.transform.localPosition = new Vector3(0, -0.4f, 0);
                credentials.transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);
            }
            else
            {
                credentials.transform.localPosition = new Vector3(0, -0.5f, 0);
                credentials.transform.localScale = new Vector3(0.9f, 0.9f, 1.0f);
                translator.gameObject.SetActive(false);
            }

        }
    }
}
