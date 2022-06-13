using System.Linq;

using HarmonyLib;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

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

            GameData.PlayerInfo player = GameData.Instance.GetPlayerById(
                gameData.IsMarinPlayerId);

            string printStr;

            if (gameData.AssassinateMarin)
            {
                printStr = player?.PlayerName + Helper.Translation.GetString(
                    "assassinateMarinSucsess");
            }
            else
            {
                printStr = player?.PlayerName + Helper.Translation.GetString(
                    "assassinateMarinFail");
            }            
            __instance.Player?.gameObject.SetActive(false);
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

            ReEnablePostfix();
        }

        public static void ReEnablePostfix()
        {
            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            var role = ExtremeRoleManager.GetLocalPlayerRole();

            if (!role.HasOtherKillCool) { return; }

            CachedPlayerControl.LocalPlayer.PlayerControl.SetKillTimer(
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
                WrapUpPrefix(__instance);
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
                WrapUpPrefix(__instance);
                return true;
            }
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        public static void WrapUpPrefix(ExileController __instance)
        {
            resetAssassinMeeting();
            if (__instance.exiled != null)
            {
                tempWinCheckDisable(__instance.exiled);
            }
        }

        public static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {

            ExtremeRolesPlugin.Info.HideBlackBG();
            ExtremeRolesPlugin.GameDataStore.Union.ResetTimer();
            ExtremeRolesPlugin.GameDataStore.AbilityManager.Clear();

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

            var ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
            if (ghostRole != null)
            {
                if (ghostRole.Button != null)
                {
                    ghostRole.Button.ResetCoolTimer();
                }
                ghostRole.ReseOnMeetingEnd();
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

        private static void resetAssassinMeeting()
        {
            if (ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger)
            {
                ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger = false;
            }
        }
        private static void tempWinCheckDisable(GameData.PlayerInfo exiled)
        {

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            var role = ExtremeRoleManager.GameRole[exiled.PlayerId];

            if (ExtremeRoleManager.IsDisableWinCheckRole(role))
            {
                ExtremeRolesPlugin.GameDataStore.WinCheckDisable = true;
            }
        }

    }
}
