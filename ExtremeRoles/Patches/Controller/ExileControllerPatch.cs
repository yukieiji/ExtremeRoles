using HarmonyLib;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Controller
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    public static class ExileControllerBeginePatch
    {
        public static bool Prefix(
            ExileController __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo exiled,
            [HarmonyArgument(1)] bool tie)
        {

            var state = ExtremeRolesPlugin.ShipState;

            if (!state.AssassinMeetingTrigger) { return true; }

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
                state.IsMarinPlayerId);

            string printStr;

            if (state.IsAssassinateMarin)
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
            if (!ExtremeRolesPlugin.ShipState.IsShowAditionalInfo()) { return; }
            TMPro.TextMeshPro infoText = UnityEngine.Object.Instantiate(
                __instance.ImpostorText,
                __instance.Text.transform);
            if (PlayerControl.GameOptions.ConfirmImpostor)
            {
                infoText.transform.localPosition += new UnityEngine.Vector3(0f, -0.4f, 0f);
            }
            else
            {
                infoText.transform.localPosition += new UnityEngine.Vector3(0f, -0.2f, 0f);
            }
            infoText.gameObject.SetActive(true);

            infoText.text = ExtremeRolesPlugin.ShipState.GetAditionalInfo();

            __instance.StartCoroutine(
                Effects.Bloop(0.25f, infoText.transform, 1f, 0.5f));
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
    public static class ExileControllerWrapUpPatch
    {

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        public static class BaseExileControllerPatch
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
            ExtremeRolesPlugin.Info.BlockShow(false);
            ExtremeRolesPlugin.ShipState.ResetOnMeeting();

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            var state = ExtremeRolesPlugin.ShipState;


            if (state.TryGetDeadAssasin(out byte playerId))
            {
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

            ExtremeRolesPlugin.ShipState.SetDisableWinCheck(false);
        }

        private static void resetAssassinMeeting()
        {
            if (ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger)
            {
                ExtremeRolesPlugin.ShipState.AssassinMeetingTriggerOff();
            }
        }
        private static void tempWinCheckDisable(GameData.PlayerInfo exiled)
        {

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            var role = ExtremeRoleManager.GameRole[exiled.PlayerId];

            if (ExtremeRoleManager.IsDisableWinCheckRole(role))
            {
                ExtremeRolesPlugin.ShipState.SetDisableWinCheck(true);
            }
        }

    }
}
