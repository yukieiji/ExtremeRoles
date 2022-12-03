using ExtremeSkins.Helper;
using HarmonyLib;

namespace ExtremeSkins.Patches.AmongUs.Tab
{
    [HarmonyPatch(typeof(SkinsTab), nameof(SkinsTab.OnEnable))]
    public static class SkinsTabPatch
    {
        public static void Prefix()
        {
            CustomCosmicTab.RemoveAllTabs();
        }
    }
}
