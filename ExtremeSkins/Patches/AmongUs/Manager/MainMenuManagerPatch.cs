using HarmonyLib;
using UnityEngine;

using ExtremeSkins.SkinManager;

namespace ExtremeSkins.Patches.AmongUs.Manager
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MainMenuManagerStartPatch
    {
        public static void Postfix(MainMenuManager __instance)
        {
            var exrLogo = new GameObject("bannerLogoExtremeSkins");
            exrLogo.transform.position = Vector3.up;
            exrLogo.transform.position += new Vector3(3.0f, -0.75f, 0.0f);
            var renderer = exrLogo.AddComponent<SpriteRenderer>();
            renderer.sprite = Module.Loader.CreateSpriteFromResources(
                "ExtremeSkins.Resources.TitleBurner.png", 425f);

            bool creatorMode = ExtremeSkinsPlugin.CreatorMode.Value;

#if WITHHAT
            if (!ExtremeHatManager.IsLoaded)
            {
                if (ExtremeHatManager.IsUpdate() && !creatorMode)
                {
                    ExtremeHatManager.PullAllData().GetAwaiter().GetResult();
                }
                ExtremeHatManager.Load();
            }
#endif

#if WITHNAMEPLATE
            if (!ExtremeNamePlateManager.IsLoaded)
            {
                if (ExtremeNamePlateManager.IsUpdate() && !creatorMode)
                {
                    ExtremeNamePlateManager.PullAllData().GetAwaiter().GetResult();
                }

                ExtremeNamePlateManager.Load();
            }
#endif
#if WITHVISOR
            if (!ExtremeVisorManager.IsLoaded)
            {
                
                if (ExtremeVisorManager.IsUpdate() && !creatorMode)
                {
                    ExtremeVisorManager.PullAllData().GetAwaiter().GetResult();
                }

                ExtremeVisorManager.Load();
            }
#endif
        }
    }
}
