using HarmonyLib;

namespace ExtremeRoles.Patches.MiniGame
{

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
    public static class SpawnInMinigameBeginPatch
    {
        public static void Postfix(SpawnInMinigame __instance)
        {
            if (ExtremeGameManager.Instance.ShipOption.IsAutoSelectRandomSpawn)
            {
                __instance.Close();
            }
        }
    }
}
