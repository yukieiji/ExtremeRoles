using HarmonyLib;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldFlipSkeld))]
    class ConstantsShouldFlipSkeldPatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (PlayerControl.GameOptions == null) { return true; }
            __result = PlayerControl.GameOptions.MapId == 3;
            return false;
        }
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
    class ConstantsShouldHorseAroundPatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (AmongUsClient.Instance != null && 
                AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started && 
                OptionHolder.AllOption[(int)OptionHolder.CommonOptionKey.EnableHorseMode].GetValue())
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

}
