using HarmonyLib;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using AmongUs.GameOptions;

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
            if (GameOptionsManager.Instance.CurrentGameOptions.GetBool(
                    BoolOptionNames.ConfirmImpostor))
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
            public static void Prefix(ExileController __instance)
            {
                WrapUpPrefix();
            }
            public static void Postfix(ExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static void Prefix(AirshipExileController __instance)
            {
                WrapUpPrefix();
            }
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        public static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
            ExtremeRolesPlugin.Info.BlockShow(false);
            ExtremeRolesPlugin.Info.HideBlackBG();
            ExtremeRolesPlugin.ShipState.ResetOnMeeting();
            Meeting.MeetingHudSelectPatch.SetSelectBlock(false);

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            var state = ExtremeRolesPlugin.ShipState;

            if (state.TryGetDeadAssasin(out byte playerId))
            {
                var assasin = (Roles.Combination.Assassin)ExtremeRoleManager.GameRole[playerId];
                assasin.ExiledAction(
                    Helper.Player.GetPlayerControlById(playerId));
            }


            var role = ExtremeRoleManager.GetLocalPlayerRole();
            if (role is IRoleAbility abilityRole)
            {
                abilityRole.ResetOnMeetingEnd();
            }
            if (role is IRoleResetMeeting resetRole)
            {
                resetRole.ResetOnMeetingEnd();
            }
            if (role is MultiAssignRoleBase multiAssignRole)
            {
                if (multiAssignRole.AnotherRole is IRoleAbility abilityMultiAssignRole)
                {
                    abilityMultiAssignRole.ResetOnMeetingEnd();
                }
                if (multiAssignRole.AnotherRole is IRoleResetMeeting resetMultiAssignRole)
                {
                    resetMultiAssignRole.ResetOnMeetingEnd();
                }
            }

            var ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
            if (ghostRole != null)
            {
                ghostRole.ResetOnMeetingEnd();
            }
        }

        public static void WrapUpPrefix()
        {
            if (ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger)
            {
                ExtremeRolesPlugin.ShipState.AssassinMeetingTriggerOff();
            }
        }
    }
}
