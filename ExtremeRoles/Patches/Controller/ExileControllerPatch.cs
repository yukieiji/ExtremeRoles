using System.Linq;

using HarmonyLib;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Patches.Controller
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    class ExileControllerBeginePatch
    {

        private static TMPro.TextMeshPro breadText;

        public static bool Prefix(
            ExileController __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo exiled,
            [HarmonyArgument(1)] bool tie)
        {

            var gameData = ExtremeRolesPlugin.GameDataStore;

            if (!gameData.AssassinMeetingTrigger) { return true; }

			if (__instance.specialInputHandler != null)
			{
				__instance.specialInputHandler.disableVirtualCursor = true;
			}
			ExileController.Instance = __instance;
			ControllerManager.Instance.CloseAndResetAll();

			__instance.exiled = null;
			__instance.Text.gameObject.SetActive(false);
            __instance.Text.text = string.Empty;

            PlayerControl player = Helper.Player.GetPlayerControlById(
                gameData.IsMarinPlayerId);

            string printStr;

            if (gameData.AssassinateMarin)
            {
                printStr = player.Data.PlayerName + Helper.Translation.GetString(
                    "assassinateMarinSucsess");
            }
            else
            {
                printStr = player.Data.PlayerName + Helper.Translation.GetString(
                    "assassinateMarinFail");
            }
            __instance.Player.gameObject.SetActive(false);
            __instance.completeString = printStr;
			__instance.ImpostorText.text = string.Empty;
			__instance.StartCoroutine(__instance.Animate());
            return false;
        }

        public static void Postfix(
            ExileController __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo exiled,
            [HarmonyArgument(1)] bool tie)
        {
            if (!ExtremeRolesPlugin.GameDataStore.Union.IsEstablish()) { return; }
            if (breadText == null)
            {
                breadText = UnityEngine.Object.Instantiate(
                    __instance.ImpostorText,
                    __instance.Text.transform);
                if (PlayerControl.GameOptions.ConfirmImpostor)
                {
                    breadText.transform.localPosition += new UnityEngine.Vector3(0f, -0.4f, 0f);
                }
                else
                {
                    breadText.transform.localPosition += new UnityEngine.Vector3(0f, -0.2f, 0f);
                }
                breadText.gameObject.SetActive(true);
            }

            breadText.text = ExtremeRolesPlugin.GameDataStore.Union.GetBreadBakingCondition();

            __instance.StartCoroutine(
                Effects.Bloop(0.25f, breadText.transform, 1f, 0.5f));
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
    class ExileControllerReEnableGameplayPatch
    {
        public static void Postfix(
            ExileController __instance)
        {

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            var role = ExtremeRoleManager.GetLocalPlayerRole();

            if (!role.HasOtherKillCool) { return; }
            
            PlayerControl.LocalPlayer.SetKillTimer(
                role.KillCoolTime);
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
                resetAssassinMeeting();
                if (__instance.exiled != null && ExtremeRoleManager.GameRole.Count != 0)
                {
                    tempWinCheckDisable(__instance.exiled);
                }
                return true;
            }
            public static void Postfix(ExileController __instance)
            {
                wrapUpPostfix(
                    __instance,
                    __instance.exiled);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static bool Prefix(AirshipExileController __instance)
            {
                resetAssassinMeeting();
                if (__instance.exiled != null)
                {
                    tempWinCheckDisable(__instance.exiled);
                }
                return true;
            }
            public static void Postfix(AirshipExileController __instance)
            {
                wrapUpPostfix(
                    __instance,
                    __instance.exiled);
            }
        }

        private static void resetAssassinMeeting()
        {
            if (ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger)
            {
                ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger = false;
            }
        }
        private static void tempWinCheckDisable(GameData.PlayerInfo exiled)
        {

            var role = ExtremeRoleManager.GameRole[exiled.PlayerId];

            if (ExtremeRoleManager.IsDisableWinCheckRole(role))
            {
                ExtremeRolesPlugin.GameDataStore.WinCheckDisable = true;
            }
        }

        private static void wrapUpPostfix(
            ExileController instance,
            GameData.PlayerInfo exiled)
        {

            ExtremeRolesPlugin.Info.HideBlackBG();
            ExtremeRolesPlugin.GameDataStore.Union.ResetTimer();

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            var gameData = ExtremeRolesPlugin.GameDataStore;

            var deadedAssassin = gameData.DeadedAssassin;

            if (deadedAssassin.Count != 0)
            {

                int callAssassin = UnityEngine.Random.RandomRange(0, deadedAssassin.Count);

                byte playerId = deadedAssassin.ElementAt(callAssassin);
                deadedAssassin.Remove(playerId);

                var assasin = (Roles.Combination.Assassin)ExtremeRoleManager.GameRole[playerId];
                assasin.ExiledAction(
                    Helper.Player.GetPlayerControlById(playerId).Data);
            }

            var role = ExtremeRoleManager.GetLocalPlayerRole();
            var abilityRole = role as IRoleAbility;

            if (abilityRole != null)
            {
                abilityRole.ResetOnMeetingEnd();
            }
            var resetRole = role as IRoleResetMeeting;
            if (resetRole != null)
            {
                resetRole.ResetOnMeetingEnd();
            }

            var multiAssignRole = role as Roles.API.MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    abilityRole = multiAssignRole.AnotherRole as IRoleAbility;
                    if (abilityRole != null)
                    {
                        abilityRole.ResetOnMeetingStart();
                    }

                    resetRole = multiAssignRole.AnotherRole as IRoleResetMeeting;
                    if (resetRole != null)
                    {
                        resetRole.ResetOnMeetingStart();
                    }
                }
            }

            if (exiled == null) { return; };

            var exiledPlayerRole = ExtremeRoleManager.GameRole[exiled.PlayerId];
            var multiAssignExiledPlayerRole = exiledPlayerRole as Roles.API.MultiAssignRoleBase;

            exiledPlayerRole.ExiledAction(exiled);
            if (multiAssignExiledPlayerRole != null)
            {
                if (multiAssignExiledPlayerRole.AnotherRole != null)
                {
                    multiAssignExiledPlayerRole.AnotherRole.ExiledAction(exiled);
                }
            }

            ExtremeRolesPlugin.GameDataStore.WinCheckDisable = false;
        }
    }
}
