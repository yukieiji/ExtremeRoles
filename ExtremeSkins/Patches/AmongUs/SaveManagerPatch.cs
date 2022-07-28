using HarmonyLib;
using ExtremeSkins.SkinManager;

namespace ExtremeSkins.Patches.AmongUs
{

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadPlayerPrefs))]
    public static class SaveManagerLoadPlayerPrefsPatch
    {
        // Fix Potential issues with broken colors
        private static bool needsPatch = false;
        
        public static void Prefix([HarmonyArgument(0)] bool overrideLoad)
        {
            if (!SaveManager.loaded || overrideLoad)
            {
                needsPatch = true;
            }
        }
        public static void Postfix()
        {
            if (!needsPatch) { return; }
            SaveManager.colorConfig %= ExtremeColorManager.ColorNum;
            needsPatch = false;
        }
    }
}
