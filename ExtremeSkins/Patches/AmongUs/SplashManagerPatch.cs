using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using HarmonyLib;

using ExtremeSkins.SkinManager;

using BepInEx.IL2CPP.Utils.Collections;

namespace ExtremeSkins.Patches.AmongUs
{
    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
    public static class SplashManagerStartPatch
    {
        public static void Postfix(SplashManager __instance)
        {

            if (ExtremeRoles.Compat.BepInExUpdater.UpdateRequired) { return; }

            bool creatorMode = CreatorModeManager.Instance.IsEnable;

            List<IEnumerator> dlTask = new List<IEnumerator>();

#if WITHHAT
            if (!ExtremeHatManager.IsLoaded)
            {
                if (!creatorMode && ExtremeHatManager.IsUpdate())
                {
                    dlTask.Add(ExtremeHatManager.InstallData());
                }
            }
#endif
#if WITHNAMEPLATE
            if (!ExtremeNamePlateManager.IsLoaded)
            {
                if (!creatorMode && ExtremeNamePlateManager.IsUpdate())
                {
                    dlTask.Add(ExtremeNamePlateManager.InstallData());
                }
            }
#endif
#if WITHVISOR
            if (!ExtremeVisorManager.IsLoaded)
            {

                if (!creatorMode && ExtremeVisorManager.IsUpdate())
                {
                    dlTask.Add(ExtremeVisorManager.InstallData());
                }
            }
#endif
            __instance.StartCoroutine(
                loadSkin(dlTask).WrapToIl2Cpp());
        }

        private static IEnumerator loadSkin(List<IEnumerator> dlTask)
        {
            SplashManagerUpdatePatch.SetSkinLoadMode(true);

            foreach (IEnumerator task in dlTask)
            {
                yield return task;
            }

            ExtremeSkinsPlugin.Logger.LogInfo("------------------------------ Skin Load Start!! ------------------------------");
#if WITHHAT
            if (!ExtremeHatManager.IsLoaded)
            {
                ExtremeHatManager.Load();
            }
#endif
#if WITHNAMEPLATE
            if (!ExtremeNamePlateManager.IsLoaded)
            {
                ExtremeNamePlateManager.Load();
            }
#endif
#if WITHVISOR
            if (!ExtremeVisorManager.IsLoaded)
            {
                ExtremeVisorManager.Load();
            }
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
        private static bool isSkinLoad = true;

        public static void SetSkinLoadMode(bool modeOn)
        {
            isSkinLoad = modeOn;
        }

        public static bool Prefix()
        {
            return !isSkinLoad;
        }
    }
}

