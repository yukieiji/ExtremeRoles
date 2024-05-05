using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using HarmonyLib;

using ExtremeSkins.SkinManager;

using BepInEx.Unity.IL2CPP.Utils;
using ExtremeSkins.SkinLoader;
using ExtremeSkins.Module;

namespace ExtremeSkins.Patches.AmongUs;

[HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
public static class SplashManagerStartPatch
{
    public static void Postfix(SplashManager __instance)
    {

        if (ExtremeRoles.Compat.BepInExUpdater.IsUpdateRquire()) { return; }

        bool creatorMode = CreatorModeManager.Instance.IsEnable;

        List<IEnumerator> dlTask = new List<IEnumerator>();

#if WITHNAMEPLATE
        if (!ExtremeNamePlateManager.IsLoaded)
        {
            if (!creatorMode && ExtremeNamePlateManager.IsUpdate())
            {
                dlTask.Add(ExtremeNamePlateManager.InstallData());
            }
        }
#endif
        __instance.StartCoroutine(loadSkin(dlTask));
    }

    private static IEnumerator loadSkin(List<IEnumerator> dlTask)
    {
        SplashManagerUpdatePatch.SetSkinLoadMode(true);

		yield return ExtremeSkinLoader.Instance.Fetch();

        foreach (IEnumerator task in dlTask)
        {
            yield return task;
        }

        ExtremeSkinsPlugin.Logger.LogInfo("------------------------------ Skin Load Start!! ------------------------------");
#if WITHHAT
		new SkinContainer<CustomHat>(
			ExtremeSkinLoader.Instance.Load<CustomHat>());
#endif
#if WITHNAMEPLATE
        if (!ExtremeNamePlateManager.IsLoaded)
        {
            ExtremeNamePlateManager.Load();
        }
#endif
#if WITHVISOR
		new SkinContainer<CustomVisor>(
			ExtremeSkinLoader.Instance.Load<CustomVisor>());
#endif
		ExtremeSkinsPlugin.Logger.LogInfo("------------------------------ All Skin Load Complete!! ------------------------------");
        SplashManagerUpdatePatch.SetSkinLoadMode(false);
    }

    private static bool isDlEnd(List<Task> dlTask)
    {
        foreach (Task task in dlTask)
        {
            if (!task.IsCompleted) { return false; }
        }
        return true;
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

