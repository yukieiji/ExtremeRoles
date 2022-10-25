using HarmonyLib;
using ExtremeSkins.SkinManager;

using AmongUs.Data.Legacy;

namespace ExtremeSkins.Patches.AmongUs
{

    [HarmonyPatch(typeof(LegacySaveManager), nameof(LegacySaveManager.LoadPlayerPrefs))]
    public static class LegacySaveManagerLoadPlayerPrefsPatch
    {
        // Fix Potential issues with broken colors
        private static bool needsPatch = false;
        
        public static void Prefix([HarmonyArgument(0)] bool overrideLoad)
        {
            if (!LegacySaveManager.loaded || overrideLoad)
            {
                needsPatch = true;
            }
        }
        public static void Postfix()
        {
            if (!needsPatch) { return; }
            LegacySaveManager.colorConfig %= ExtremeColorManager.ColorNum;
            needsPatch = false;
        }
    }
}