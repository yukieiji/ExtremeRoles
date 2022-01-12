using HarmonyLib;

using TMPro;
using Twitch;

using UnityEngine;

namespace ExtremeRoles.Patches.Manager
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MainMenuManagerStartPatch
    {
        static void Postfix(MainMenuManager __instance)
        {
            DestroyableSingleton<ModManager>.Instance.ShowModStamp();

            var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
            if (amongUsLogo != null)
            {
                amongUsLogo.transform.localScale *= 0.9f;
                amongUsLogo.transform.position += Vector3.up * 0.25f;
            }

            var torLogo = new GameObject("bannerLogoExtremeRoles");
            torLogo.transform.position = Vector3.up;
            var renderer = torLogo.AddComponent<SpriteRenderer>();
            renderer.sprite = Helper.Resources.LoadSpriteFromResources(
                Resources.ResourcesPaths.TitleBurner, 300f);

            var tmp = __instance.Announcement.transform.Find(
                "Title_Text").gameObject.GetComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.transform.localPosition += Vector3.left * 0.2f;
            ExtremeRolesPlugin.TextPrefab = Object.Instantiate(tmp);
            Object.Destroy(ExtremeRolesPlugin.TextPrefab.GetComponent<
                TextTranslatorTMP>());
            ExtremeRolesPlugin.TextPrefab.gameObject.SetActive(false);
            Object.DontDestroyOnLoad(ExtremeRolesPlugin.TextPrefab);

            if (Option.OptionsMenuBehaviourStartPatch.PropPrefab == null)
            {
                TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
                Option.OptionsMenuBehaviourStartPatch.PropPrefab = Object.Instantiate(man.TwitchPopup);
                Object.DontDestroyOnLoad(
                    Option.OptionsMenuBehaviourStartPatch.PropPrefab);
                Option.OptionsMenuBehaviourStartPatch.PropPrefab.name = "propForInEx";
                Option.OptionsMenuBehaviourStartPatch.PropPrefab.gameObject.SetActive(false);
            }

        }
    }
}
