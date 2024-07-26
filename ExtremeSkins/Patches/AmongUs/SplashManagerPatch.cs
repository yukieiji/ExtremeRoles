using System.Collections;

using HarmonyLib;

using BepInEx.Unity.IL2CPP.Utils;
using ExtremeSkins.Loader;
using ExtremeSkins.Module;

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
			yield return ExtremeCosmicLoader.Instance.Fetch();
		}

        ExtremeSkinsPlugin.Logger.LogInfo("------------------------------ Skin Load Start!! ------------------------------");
#if WITHHAT
		new CosmicStorage<CustomHat>(
			ExtremeCosmicLoader.Instance.Load<CustomHat>());
#endif
#if WITHNAMEPLATE
		new CosmicStorage<CustomNamePlate>(
			ExtremeCosmicLoader.Instance.Load<CustomNamePlate>());
#endif
#if WITHVISOR
		new CosmicStorage<CustomVisor>(
			ExtremeCosmicLoader.Instance.Load<CustomVisor>());
#endif
		// 使わないので消す
		ExtremeCosmicLoader.TryDestroy();

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

