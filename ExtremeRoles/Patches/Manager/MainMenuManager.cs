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
            Module.Prefab.Text = Object.Instantiate(tmp);
            Object.Destroy(Module.Prefab.Text.GetComponent<
                TextTranslatorTMP>());
            Module.Prefab.Text.gameObject.SetActive(false);
            Object.DontDestroyOnLoad(Module.Prefab.Text);

            if (Module.Prefab.Prop == null)
            {
                TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
                Module.Prefab.Prop = Object.Instantiate(man.TwitchPopup);
                Object.DontDestroyOnLoad(
                    Module.Prefab.Prop);
                Module.Prefab.Prop.name = "propForInEx";
                Module.Prefab.Prop.gameObject.SetActive(false);
            }

        }
    }
}
