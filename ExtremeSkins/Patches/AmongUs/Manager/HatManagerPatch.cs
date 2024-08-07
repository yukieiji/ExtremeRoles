﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

using ExtremeSkins.Module;

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
				foreach (CustomHat hat in CosmicStorage<CustomHat>.GetAll())
				{
					hatData.Add(hat.Data);
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
                foreach (CustomNamePlate np in CosmicStorage<CustomNamePlate>.GetAll())
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
                foreach (CustomVisor vi in CosmicStorage<CustomVisor>.GetAll())
                {
                    visorData.Add(vi.Data);
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
