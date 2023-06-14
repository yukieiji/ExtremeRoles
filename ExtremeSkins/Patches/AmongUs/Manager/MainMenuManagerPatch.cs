using HarmonyLib;
using UnityEngine;

namespace ExtremeSkins.Patches.AmongUs.Manager;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class MainMenuManagerStartPatch
{
	[HarmonyPostfix, HarmonyPriority(Priority.Last)]
	public static void Postfix()
    {
		var exrLogo = GameObject.Find("bannerLogoExtremeRoles");
		if (exrLogo == null) { return; }

        var exsLogo = new GameObject("bannerLogoExtremeSkins");
		exsLogo.transform.parent = exrLogo.transform;
		exsLogo.transform.position = Vector3.up;
		exsLogo.transform.transform.localPosition = new Vector3(1.275f, -0.75f, -1.0f);
        var renderer = exsLogo.AddComponent<SpriteRenderer>();
        renderer.sprite = Module.Loader.GetTitleLog();
    }
}
