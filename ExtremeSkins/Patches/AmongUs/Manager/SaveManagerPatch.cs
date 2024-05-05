using HarmonyLib;

using AmongUs.Data.Legacy;
using ExtremeSkins.Loader;

namespace ExtremeSkins.Patches.AmongUs.Manager
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
            LegacySaveManager.colorConfig %= (uint)CustomColorLoader.AllColorNum;
            needsPatch = false;
        }
    }
}