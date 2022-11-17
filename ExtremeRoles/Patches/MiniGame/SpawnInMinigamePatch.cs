using HarmonyLib;

namespace ExtremeRoles.Patches.MiniGame
{

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
    public static class SpawnInMinigameBeginPatch
    {
        public static void Postfix(SpawnInMinigame __instance)
        {
            if (OptionHolder.Ship.IsAutoSelectRandomSpawn)
            {
                __instance.LocationButtons[
                    RandomGenerator.Instance.Next(
                        __instance.LocationButtons.Length)].ReceiveClickUp();
            }
        }
    }
}
