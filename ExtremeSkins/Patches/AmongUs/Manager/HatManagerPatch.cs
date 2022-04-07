using System;
using HarmonyLib;

namespace ExtremeSkins.Patches.AmongUs.Manager
{
#if WITHHAT
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
    public static class HatManagerGetHatByIdPatch
    {
        private static bool isRunning;
        private static bool isLoaded;
        public static void Prefix(HatManager __instance)
        {
            if (isRunning) { return; }
            isRunning = true; // prevent simultanious execution

            try
            {
                if (!isLoaded)
                {
                    foreach (var hat in ExtremeHatManager.HatData.Values)
                    {
                        __instance.allHats.Add(
                            hat.GetData());
                    }
                }
            }
            catch (Exception e)
            {
                ExtremeSkinsPlugin.Logger.LogInfo(
                    $"Unable to add Custom Hats\n{e}");
            }
            isLoaded = true;
        }
        public static void Postfix(HatManager __instance)
        {
            isRunning = false;
        }
    }
#endif

#if WITHNAMEPLATE
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetNamePlateById))]
    public static class HatManagerGetNamePlateByIdPatch
    {
        private static bool isRunning;
        private static bool isLoaded;
        public static void Prefix(HatManager __instance)
        {
            if (isRunning) { return; }
            isRunning = true; // prevent simultanious execution

            try
            {
                if (!isLoaded)
                {
                    foreach (var np in ExtremeNamePlateManager.NamePlateData.Values)
                    {
                        __instance.allNamePlates.Add(
                            np.GetData());
                    }
                }
            }
            catch (Exception e)
            {
                ExtremeSkinsPlugin.Logger.LogInfo(
                    $"Unable to add Custom NamePlate\n{e}");
            }
            isLoaded = true;
        }
        public static void Postfix(HatManager __instance)
        {
            isRunning = false;
        }
    }
#endif
}
