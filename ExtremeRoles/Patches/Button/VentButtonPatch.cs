using HarmonyLib;

namespace ExtremeRoles.Patches.Button
{
    // VentButtonクラスに関するパッチ
    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
    public static class VentButtonDoClickPatch
    {
        public static bool Prefix(VentButton __instance)
        {
            // Manually modifying the VentButton to use Vent.Use again in order to trigger the Vent.Use prefix patch
            if (__instance.currentTarget != null)
            {
                Helper.Logging.Debug($"VentButtonClicked");
                __instance.currentTarget.Use();
            }
            return false;
        }
    }
}
