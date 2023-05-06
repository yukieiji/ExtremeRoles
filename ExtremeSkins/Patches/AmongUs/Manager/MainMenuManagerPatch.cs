using HarmonyLib;
using UnityEngine;

namespace ExtremeSkins.Patches.AmongUs.Manager;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class MainMenuManagerStartPatch
{
    public static void Postfix(MainMenuManager __instance)
    {
        var exrLogo = new GameObject("bannerLogoExtremeSkins");
        exrLogo.transform.position = Vector3.up;
        exrLogo.transform.position += new Vector3(3.0f, -0.75f, 0.0f);
        var renderer = exrLogo.AddComponent<SpriteRenderer>();
        renderer.sprite = Module.Loader.GetTitleLog();
    }
}
