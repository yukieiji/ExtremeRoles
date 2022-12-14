using HarmonyLib;
using ExtremeSkins.Helper;

namespace ExtremeSkins.Patches.AmongUs
{
    [HarmonyPatch(
        typeof(PlayerCustomizationMenu), 
        nameof(PlayerCustomizationMenu.OnDisable))]
    public static class PlayerCustomizationMenuOnDisablePatch
    {
        public static void Postfix()
        {
            CustomCosmicTab.RemoveAllTabs();
        }
    }
}
