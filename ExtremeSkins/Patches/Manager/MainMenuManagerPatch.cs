using HarmonyLib;

namespace ExtremeSkins.Patches.Manager
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MainMenuManagerStartPatch
    {
        public static void Postfix(MainMenuManager __instance)
        {
            // ExtremeSkins.ExtremeHatManager.CheckUpdate();
            ExtremeHatManager.Load();
        }
    }
}
