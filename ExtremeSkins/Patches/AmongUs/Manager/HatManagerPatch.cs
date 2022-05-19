using System;
using HarmonyLib;

using ExtremeSkins.SkinManager;

namespace ExtremeSkins.Patches.AmongUs.Manager
{
#if WITHHAT
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
    public static class HatManagerGetHatByIdPatch
    {
        private static bool isRunning = false;
        private static bool isLoaded = false;
        public static void Prefix(HatManager __instance)
        {
            if (isRunning || isLoaded) { return; }
            isRunning = true; // prevent simultanious execution

            try
            {
                foreach (var hat in ExtremeHatManager.HatData.Values)
                {
                    __instance.allHats.Add(hat.GetData());
                }
                isRunning = false;
            }
            catch (Exception e)
            {
                ExtremeSkinsPlugin.Logger.LogInfo(
                    $"Unable to add Custom Hats\n{e}");
            }
            isLoaded = true;
        }
    }
#endif

#if WITHNAMEPLATE
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetNamePlateById))]
    public static class HatManagerGetNamePlateByIdPatch
    {
        private static bool isRunning = false;
        private static bool isLoaded = false;
        public static void Prefix(HatManager __instance)
        {
            if (isRunning || isLoaded) { return; }
            isRunning = true; // prevent simultanious execution
            
            try
            {
                foreach (var np in ExtremeNamePlateManager.NamePlateData.Values)
                {
                    __instance.allNamePlates.Add(np.GetData());
                }
                isRunning = false;
            }
            catch (Exception e)
            {
                ExtremeSkinsPlugin.Logger.LogInfo(
                    $"Unable to add Custom NamePlate\n{e}");
            }
            isLoaded = true;
        }
    }
#endif
#if WITHVISOR
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetVisorById))]
    public static class HatManagerGetVisorByIdPatch
    {
        private static bool isRunning = false;
        private static bool isLoaded = false;
        public static void Prefix(HatManager __instance)
        {
            if (isRunning || isLoaded) { return; }
            isRunning = true; // prevent simultanious execution

            try
            {
                foreach (var vi in ExtremeVisorManager.VisorData.Values)
                {
                    __instance.allVisors.Add(vi.GetData());
                }
                isRunning = false;
            }
            catch (Exception e)
            {
                ExtremeSkinsPlugin.Logger.LogInfo(
                    $"Unable to add Custom Visor\n{e}");
            }
            isLoaded = true;
        }
    }
#endif
}
