
using System.Collections.Generic;

using HarmonyLib;

using ExtremeRoles.Roles;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch]
    class ExileControllerWrapUpPatch
    {

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch
        {
            public static void Postfix(ExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        private static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
            var deadedAssassin = Modules.PlayerDataContainer.DeadedAssassin;

            if (deadedAssassin.Count != 0)
            {
                foreach (var playerId in deadedAssassin)
                {
                    var assasin = (Roles.Combination.Assassin)ExtremeRoleManager.GameRole[playerId];

                    assasin.ExiledAction(
                        Modules.Helpers.GetPlayerControlById(playerId).Data);
                    if (assasin.IsForceWin) { break; }
     
                }
                Modules.PlayerDataContainer.DeadedAssassin.Clear();
            }

            if (exiled == null) { return; };

            ExtremeRoleManager.GameRole[exiled.PlayerId].ExiledAction(exiled);
        }
    }
}
