using HarmonyLib;

using UnityEngine;

namespace ExtremeRoles.Patches.Manager
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class LogoPatch
    {
        static void Postfix(PingTracker __instance)
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
        }
    }
}
