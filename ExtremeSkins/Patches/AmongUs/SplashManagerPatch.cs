using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using HarmonyLib;
using UnityEngine;

using ExtremeSkins.SkinManager;
using ExtremeRoles.Module;

using BepInEx.IL2CPP.Utils.Collections;

namespace ExtremeSkins.Patches.AmongUs
{
    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
    public static class SplashManagerStartPatch
    {
        public static void Postfix(SplashManager __instance)
        {

            if (ExtremeRoles.Compat.BepInExUpdater.UpdateRequired) { return; }

            bool creatorMode = ExtremeSkinsPlugin.CreatorMode.Value;

            List<Task> dlTask = new List<Task>();

#if WITHHAT
            if (!ExtremeHatManager.IsLoaded)
            {
                if (!creatorMode && ExtremeHatManager.IsUpdate())
                {
                    dlTask.Add(ExtremeHatManager.PullAllData());
                }
            }
#endif
#if WITHNAMEPLATE
            if (!ExtremeNamePlateManager.IsLoaded)
            {
                if (!creatorMode && ExtremeNamePlateManager.IsUpdate())
                {
                    dlTask.Add(ExtremeNamePlateManager.PullAllData());
                }
            }
#endif
#if WITHVISOR
            if (!ExtremeVisorManager.IsLoaded)
            {

                if (!creatorMode && ExtremeVisorManager.IsUpdate())
                {
                    dlTask.Add(ExtremeVisorManager.PullAllData());
                }
            }
#endif
            __instance.StartCoroutine(
                loadSkin(dlTask).WrapToIl2Cpp());
        }

        private static IEnumerator loadSkin(List<Task> dlTask)
        {
            SplashManagerUpdatePatch.SetSkinLoadMode(true);

            if (dlTask.Count > 0)
            {
                string showStr = Helper.Translation.GetString("waitSkinDl");

                Task.Run(() => DllApi.MessageBox(
                    System.IntPtr.Zero,
                    showStr, "Extreme Skins", 0));

                while (!isDlEnd(dlTask))
                {
                    yield return new WaitForSeconds(1.0f);
                }
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

