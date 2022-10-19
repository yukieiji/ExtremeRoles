using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

using ExtremeSkins.Module;
using ExtremeSkins.SkinManager;

namespace ExtremeSkins.Patches.AmongUs.Manager
{
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.Initialize))]
    public static class HatManagerInitializePatch
    {
        public static void Postfix(HatManager __instance)
        {
#if WITHHAT
            try
            {
                List<HatData> hatData = __instance.allHats.ToList();
                foreach (CustomHat hat in ExtremeHatManager.HatData.Values)
                {
                    hatData.Add(hat.GetData());
                }
                __instance.allHats = hatData.ToArray();
            }
            catch (Exception e)
            {
                ExtremeSkinsPlugin.Logger.LogInfo(
                    $"Unable to add Custom Hats\n{e}");
            }
#endif
#if WITHNAMEPLATE
            try
            {
                List<NamePlateData> npData = __instance.allNamePlates.ToList();
                foreach (CustomNamePlate np in ExtremeNamePlateManager.NamePlateData.Values)
                {
                    npData.Add(np.GetData());
                }
                __instance.allNamePlates = npData.ToArray();
            }
            catch (Exception e)
            {
                ExtremeSkinsPlugin.Logger.LogInfo(
                    $"Unable to add Custom NamePlate\n{e}");
            }
#endif
#if WITHVISOR
            try
            {
                List<VisorData> visorData = __instance.allVisors.ToList();
                foreach (CustomVisor vi in ExtremeVisorManager.VisorData.Values)
                {
                    visorData.Add(vi.GetData());
                }
                __instance.allVisors = visorData.ToArray();
            }
            catch (Exception e)
            {
                ExtremeSkinsPlugin.Logger.LogInfo(
                    $"Unable to add Custom Visor\n{e}");
            }
#endif
        }
    }
}
