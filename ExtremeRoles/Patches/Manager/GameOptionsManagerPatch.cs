using HarmonyLib;

using AmongUs.GameOptions;

namespace ExtremeRoles.Patches.Manager
{
    [HarmonyPatch(typeof(GameOptionsManager), nameof(GameOptionsManager.SwitchGameMode))]
    public static class GameOptionsManagerSwitchGameModePatch
    {
        public static void Postfix([HarmonyArgument(0)] GameModes gameMode)
        {
            ExtremeGameManager.Create(gameMode);
        }
    }
}
