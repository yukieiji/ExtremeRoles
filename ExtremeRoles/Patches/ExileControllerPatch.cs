
using HarmonyLib;

using ExtremeRoles.Roles;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    class ExileControllerBeginePatch
    {
        public static bool Prefix(
            ExileController __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo exiled,
            [HarmonyArgument(1)] bool tie)
        {
			if (!AssassinMeeting.AssassinMeetingTrigger) { return true; }

			if (__instance.specialInputHandler != null)
			{
				__instance.specialInputHandler.disableVirtualCursor = true;
			}
			ExileController.Instance = __instance;
			ControllerManager.Instance.CloseAndResetAll();

			__instance.exiled = null;
			__instance.Text.gameObject.SetActive(false);
            __instance.Text.text = string.Empty;

            string printStr;

            if (AssassinMeeting.AssassinateMarin)
            {
                printStr = "Sucsess!!";
            }
            else
            {
                printStr = "Fail!!";
            }
            __instance.Player.gameObject.SetActive(false);
            __instance.completeString = printStr;
			__instance.ImpostorText.text = string.Empty;
			__instance.StartCoroutine(__instance.Animate());
            return false;
        }
    }


    [HarmonyPatch]
    class ExileControllerWrapUpPatch
    {

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch
        {
            public static bool Prefix(ExileController __instance)
            {
                ResetAssassinMeeting();
                return true;
            }
            public static void Postfix(ExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static bool Prefix(AirshipExileController __instance)
            {
                ResetAssassinMeeting();
                return true;
            }
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        private static void ResetAssassinMeeting()
        {
            if (AssassinMeeting.AssassinMeetingTrigger)
            {
                AssassinMeeting.AssassinMeetingTrigger = false;
            }
        }
        private static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
            var deadedAssassin = Module.PlayerDataContainer.DeadedAssassin;

            if (deadedAssassin.Count != 0)
            {
                foreach (var playerId in deadedAssassin)
                {
                    var assasin = (Roles.Combination.Assassin)ExtremeRoleManager.GameRole[playerId];

                    assasin.ExiledAction(
                        Helper.Player.GetPlayerControlById(playerId).Data);
                    if (AssassinMeeting.AssassinateMarin) { break; }
     
                }
                Module.PlayerDataContainer.DeadedAssassin.Clear();
            }

            AssassinMeeting.AssassinMeetingTrigger = false;

            if (exiled == null) { return; };

            ExtremeRoleManager.GameRole[exiled.PlayerId].ExiledAction(exiled);
        }
    }
}
