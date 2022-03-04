using System;
using HarmonyLib;

namespace ExtremeSkins.Patches.Manager
{
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
    public static class HatManagerGetHatByIdPatch
    {
        private static bool isRunning;
        public static void Prefix(HatManager __instance)
        {
            if (isRunning) { return; }
            isRunning = true; // prevent simultanious execution

            try
            {
                foreach (var hat in ExtremeHatManager.HatData.Values)
                {
                    __instance.AllHats.Add(hat.GetHatBehaviour());
                }
            }
            catch (Exception e)
            {
                ExtremeSkinsPlugin.Logger.LogInfo(
                    $"Unable to add Custom Hats\n{e}");
            }
        }
        public static void Postfix(HatManager __instance)
        {
            isRunning = false;
        }
    }
}
