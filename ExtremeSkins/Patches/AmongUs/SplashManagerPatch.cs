using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using HarmonyLib;

using ExtremeSkins.SkinManager;

using BepInEx.Unity.IL2CPP.Utils;
using ExtremeSkins.SkinLoader;
using ExtremeSkins.Module;
using ExtremeSkins.Core;

namespace ExtremeSkins.Patches.AmongUs;

[HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
public static class SplashManagerStartPatch
{
    public static void Postfix(SplashManager __instance)
    {

        if (ExtremeRoles.Compat.BepInExUpdater.IsUpdateRquire()) { return; }
        __instance.StartCoroutine(loadSkin());
    }

    private static IEnumerator loadSkin()
    {
        SplashManagerUpdatePatch.SetSkinLoadMode(true);

		if (!CreatorModeManager.Instance.IsEnable)
		{
			yield return ExtremeSkinLoader.Instance.Fetch();
		}

        ExtremeSkinsPlugin.Logger.LogInfo("------------------------------ Skin Load Start!! ------------------------------");
#if WITHHAT
		new SkinContainer<CustomHat>(
			ExtremeSkinLoader.Instance.Load<CustomHat>());
#endif
#if WITHNAMEPLATE
		new SkinContainer<CustomNamePlate>(
			ExtremeSkinLoader.Instance.Load<CustomNamePlate>());
#endif
#if WITHVISOR
		new SkinContainer<CustomVisor>(
			ExtremeSkinLoader.Instance.Load<CustomVisor>());
#endif
		ExtremeSkinsPlugin.Logger.LogInfo("------------------------------ All Skin Load Complete!! ------------------------------");
        SplashManagerUpdatePatch.SetSkinLoadMode(false);
    }
}
[HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
public static class SplashManagerUpdatePatch
{
    public static bool IsSkinLoad = true;

    public static void SetSkinLoadMode(bool modeOn)
    {
		IsSkinLoad = modeOn;
    }

    public static bool Prefix()
    {
        return !IsSkinLoad;
    }
}

