﻿using System;
using System.Linq;
using HarmonyLib;

using AmongUs.GameOptions;

using ExtremeRoles.Compat;
using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

using Il2CppObject = Il2CppSystem.Object;
using Assassin = ExtremeRoles.Roles.Combination.Assassin;
using Translation = ExtremeRoles.Helper.Translation;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

#nullable enable

namespace ExtremeRoles.Patches.Controller;

[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
public static class ExileControllerBeginePatch
{
    private const string TransKeyBase = "ExileText";

	/* JAJPs
	[Info   :Extreme Roles] TransKey:ExileTextSP    Value:{0}がインポスターだった。
	[Info   :Extreme Roles] TransKey:ExileTextSN    Value:{0}はインポスターではなかった。
	[Info   :Extreme Roles] TransKey:ExileTextPP    Value:{0}はインポスターだった。
	[Info   :Extreme Roles] TransKey:ExileTextPN    Value:{0}はインポスターではなかった。
	[Info   :Extreme Roles] TransKey:NoExileSkip    Value:誰も追放されなかった。（投票スキップ）
	[Info   :Extreme Roles] TransKey:NoExileTie    Value:誰も追放されなかった。（同数投票）
	[Info   :Extreme Roles] TransKey:ExileTextNonConfirm    Value:{0}が追放された。
	[Info   :Extreme Roles] TransKey:ImpostorsRemainS    Value:インポスターが{0}人残っている。
	[Info   :Extreme Roles] TransKey:ImpostorsRemainP    Value:インポスターが{0}人残っている。
	*/

	[HarmonyPrefix, HarmonyPriority(Priority.Last)]
    public static bool Prefix(
        ExileController __instance,
        [HarmonyArgument(0)] NetworkedPlayerInfo exiled,
        [HarmonyArgument(1)] bool tie)
    {
		if (CompatModManager.Instance.IsModMap<SubmergedIntegrator>())
		{
			return true;
		}
		else
		{
			return PrefixRun(__instance, exiled, tie);
		}
    }

    public static void Postfix(ExileController __instance)
    {
        if (!MeetingReporter.IsExist ||
            ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }

		string reports = MeetingReporter.Instance.GetMeetingEndReport();

		if (string.IsNullOrEmpty(reports)) { return; }

        TMPro.TextMeshPro infoText = UnityEngine.Object.Instantiate(
            __instance.ImpostorText,
            __instance.Text.transform);

        float textOffset = GameOptionsManager.Instance.CurrentGameOptions.GetBool(
            BoolOptionNames.ConfirmImpostor) ? -0.4f : -0.2f;

        infoText.transform.localPosition += new UnityEngine.Vector3(0f, textOffset, 0f);
        infoText.gameObject.SetActive(true);
        infoText.text = reports;

        __instance.StartCoroutine(
            Effects.Bloop(0.25f, infoText.transform, 1f, 0.5f));
    }

	public static bool PrefixRun(
		ExileController __instance,
		NetworkedPlayerInfo exiled,
		bool tie)
	{
		if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

		var state = ExtremeRolesPlugin.ShipState;
		if (state.AssassinMeetingTrigger)
		{
			assassinMeetingEndBegin(__instance, state);
			return false;
		}
		else if (GameManager.Instance.LogicOptions.GetConfirmImpostor())
		{
			var shipOption = ExtremeGameModeManager.Instance.ShipOption;
			confirmExile(
				__instance, exiled, shipOption.Exile, tie);
			return false;
		}
		return true;
	}

    private static void assassinMeetingEndBegin(
        ExileController instance, ExtremeShipStatus state)
    {
        setExiledTarget(instance, null);
        NetworkedPlayerInfo? player = GameData.Instance.GetPlayerById(
            state.IsMarinPlayerId);
		if (player == null)
		{
			return;
		}

        string transKey = state.IsAssassinateMarin ?
            "assassinateMarinSucsess" : "assassinateMarinFail";
        string printStr = $"{player.PlayerName}{Translation.GetString(transKey)}";

        if (instance.Player)
        {
            instance.Player.gameObject.SetActive(false);
        }
        instance.completeString = printStr;
        instance.ImpostorText.text = string.Empty;

        instance.StartCoroutine(instance.Animate());
    }

    private static void confirmExile(
        ExileController instance,
        NetworkedPlayerInfo exiled,
        in ExileOption option,
		bool tie)
    {
        setExiledTarget(instance, exiled);
        var transController = FastDestroyableSingleton<TranslationController>.Instance;

        var allPlayer = GameData.Instance.AllPlayers.ToArray();
        var alivePlayers = allPlayer.Where(
            x =>
            {
                return
                    (
                        (exiled != null && x.PlayerId != exiled.PlayerId) || (exiled == null)
                    ) && !x.IsDead && !x.Disconnected;
            });
        var allRoles = ExtremeRoleManager.GameRole;

        int aliveImpNum = Enumerable.Count(
            alivePlayers,
            (NetworkedPlayerInfo p) =>
            {
                return allRoles[p.PlayerId].IsImpostor();
            });
        int aliveCrewNum = Enumerable.Count(
            alivePlayers,
            (NetworkedPlayerInfo p) =>
            {
                return allRoles[p.PlayerId].IsCrewmate();
            });
        int aliveNeutNum = Enumerable.Count(
            alivePlayers,
            (NetworkedPlayerInfo p) =>
            {
                return allRoles[p.PlayerId].IsNeutral();
            });

        string completeString = string.Empty;

		var mode = option.Mode;
        if (exiled != null)
        {
            string playerName = exiled.PlayerName;
            var exiledPlayerRole = allRoles[exiled.PlayerId];
            switch (mode)
            {
                case ConfirmExileMode.AllTeam:
                    string team = Translation.GetString(exiledPlayerRole.Team.ToString());
                    completeString = option.IsConfirmRole ?
                        string.Format(
							Translation.GetString("ExileTextAllTeamWithRole"),
                            playerName, team, exiledPlayerRole.GetColoredRoleName()) :
                        string.Format(
							Translation.GetString("ExileTextAllTeam"),
                            playerName, team);
                    break;
                default:
                    completeString = getCompleteString(
                        playerName, exiledPlayerRole, in option);
                    break;
            }

			instance.Player.UpdateFromEitherPlayerDataOrCache(
				exiled, PlayerOutfitType.Default,
				PlayerMaterial.MaskType.Exile, false, (Il2CppSystem.Action)(() =>
			{
				string exiledPlayerSkinId = exiled.Outfits[PlayerOutfitType.Default].SkinId;

				SkinViewData skin = CachedShipStatus.Instance.CosmeticsCache.GetSkin(exiledPlayerSkinId);
				if (!FastDestroyableSingleton<HatManager>.Instance.CheckLongModeValidCosmetic(
						exiledPlayerSkinId, instance.Player.GetIgnoreLongMode()))
				{
					skin = ShipStatus.Instance.CosmeticsCache.GetSkin("skin_None");
				}

				var showFrame = instance.useIdleAnim ? skin.IdleFrame : skin.EjectFrame;

				instance.Player.FixSkinSprite(showFrame);
			}));
			instance.Player.ToggleName(false);
            instance.Player.SetCustomHatPosition(instance.exileHatPosition);
            instance.Player.SetCustomVisorPosition(instance.exileVisorPosition);
        }
        else
        {
            completeString = transController.GetString(
                tie ? StringNames.NoExileTie : StringNames.NoExileSkip,
                Array.Empty<Il2CppObject>());
            instance.Player.gameObject.SetActive(false);
        }

        instance.completeString = completeString;
        instance.ImpostorText.text = mode switch
        {
            ConfirmExileMode.Impostor => transController.GetString(
                aliveImpNum == 1 ? StringNames.ImpostorsRemainS : StringNames.ImpostorsRemainP,
                [ aliveImpNum ]),

            ConfirmExileMode.Crewmate => string.Format(
				Translation.GetString(
                    aliveCrewNum == 1 ? "CrewmateRemainS" : "CrewmateRemainP"),
                aliveCrewNum),

            ConfirmExileMode.Neutral => string.Format(
				Translation.GetString(
                    aliveNeutNum == 1 ?  "NeutralRemainS" : "NeutralRemainP"),
                aliveNeutNum),

            ConfirmExileMode.AllTeam => string.Format(
				Translation.GetString("AllTeamAlive"),
                aliveCrewNum, aliveImpNum, aliveNeutNum),

            _ => string.Empty
        };
        instance.StartCoroutine(instance.Animate());
    }

    private static string getSuffix(
        bool isExiledSameMode,
        bool isModeTeamContain)
    {
        string modeTeamSuffix;

        if (isExiledSameMode && isModeTeamContain)
        {
            modeTeamSuffix = "PP";
        }
        else if (isExiledSameMode)
        {
            modeTeamSuffix = "SP";
        }
        else if (isModeTeamContain)
        {
            modeTeamSuffix = "PN";
        }
        else
        {
            modeTeamSuffix = "SN";
        }

        return modeTeamSuffix;
    }

    private static string getCompleteString(
        string playerName,
        SingleRoleBase exiledPlayerRole,
		in ExileOption option)
    {
		var mode = option.Mode;

        string teamSuffix = mode switch
        {
            ConfirmExileMode.Crewmate => "Crew",
            ConfirmExileMode.Neutral => "Neut",
            _ => string.Empty,
        };

        var allPlayer = GameData.Instance.AllPlayers.ToArray();
        var allRoles = ExtremeRoleManager.GameRole;

        bool isExiledSameMode = false;
        int modeTeamAlive = 0;
        switch (mode)
        {
            case ConfirmExileMode.Impostor:
                modeTeamAlive = allPlayer.Count(
					(NetworkedPlayerInfo p) =>
						p != null &&
						ExtremeRoleManager.TryGetRole(p.PlayerId, out var role) &&
						role!.IsImpostor());
                isExiledSameMode = exiledPlayerRole.IsImpostor();
                break;
            case ConfirmExileMode.Crewmate:
                modeTeamAlive = allPlayer.Count(
					(NetworkedPlayerInfo p) =>
						p != null &&
						ExtremeRoleManager.TryGetRole(p.PlayerId, out var role) &&
						role!.IsCrewmate());
                isExiledSameMode = exiledPlayerRole.IsCrewmate();
                break;
            case ConfirmExileMode.Neutral:
                modeTeamAlive = allPlayer.Count(
					(NetworkedPlayerInfo p) =>
						p != null &&
						ExtremeRoleManager.TryGetRole(p.PlayerId, out var role) &&
						role!.IsNeutral());
                isExiledSameMode = exiledPlayerRole.IsNeutral();
                break;
            default:
                break;
        };
        string suffix = getSuffix(isExiledSameMode, modeTeamAlive > 1);
        string transKey = $"{TransKeyBase}{suffix}{teamSuffix}";

        if (Enum.TryParse(transKey, out StringNames sn))
        {
            return FastDestroyableSingleton<TranslationController>.Instance.GetString(
                sn, [ playerName ]);
        }
        else
        {
            return
                option.IsConfirmRole ?
                string.Format(
                    Translation.GetString($"{transKey}WithRole"),
                    playerName,
                    exiledPlayerRole.GetColoredRoleName()
                ) :
                string.Format(
                    Translation.GetString(transKey),
                    playerName
                );
        }
    }

    private static void setExiledTarget(
        ExileController instance, NetworkedPlayerInfo? player)
    {
        if (instance.specialInputHandler != null)
        {
            instance.specialInputHandler.disableVirtualCursor = true;
        }
        ExileController.Instance = instance;
        ControllerManager.Instance.CloseAndResetAll();

        instance.exiled = player;
        instance.Text.gameObject.SetActive(false);
        instance.Text.text = string.Empty;
    }
}

[HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
public static class ExileControllerReEnableGameplayPatch
{
    public static void Postfix()
    {
        ReEnablePostfix();
    }

    public static void ReEnablePostfix()
    {
        if (ExtremeRoleManager.GameRole.Count == 0) { return; }

        MeetingReporter.Reset();

        var role = ExtremeRoleManager.GetLocalPlayerRole();

        if (!role.TryGetKillCool(out float killCool)) { return; }

        PlayerControl.LocalPlayer.SetKillTimer(killCool);
    }

}

[HarmonyPatch]
public static class ExileControllerWrapUpPatch
{

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public static class BaseExileControllerPatch
    {
        public static void Prefix()
        {
            WrapUpPrefix();
        }
        public static void Postfix(ExileController __instance)
        {
            WrapUpPostfix(__instance.exiled);
        }
    }

    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    public static class AirshipExileControllerPatch
    {
        public static void Prefix()
        {
            WrapUpPrefix();
        }
        public static void Postfix(AirshipExileController __instance)
        {
            WrapUpPostfix(__instance.exiled);
        }
    }

    public static void WrapUpPostfix(NetworkedPlayerInfo? exiled)
    {
        InfoOverlay.Instance.IsBlock = false;
        Meeting.Hud.MeetingHudSelectPatch.SetSelectBlock(false);

        if (ExtremeRoleManager.GameRole.Count == 0) { return; }

        var state = ExtremeRolesPlugin.ShipState;

        if (state.TryGetDeadAssasin(out byte playerId) &&
			ExtremeRoleManager.TryGetSafeCastedRole(playerId, out Assassin? assasin))
        {
            assasin!.ExiledAction(
				Helper.Player.GetPlayerControlById(playerId));
        }


        var role = ExtremeRoleManager.GetLocalPlayerRole();
        if (role is IRoleAbility abilityRole)
        {
            abilityRole.Button.OnMeetingEnd();
        }
        if (role is IRoleResetMeeting resetRole)
        {
            resetRole.ResetOnMeetingEnd(exiled);
        }
        if (role is MultiAssignRoleBase multiAssignRole)
        {
            if (multiAssignRole.AnotherRole is IRoleAbility abilityMultiAssignRole)
            {
                abilityMultiAssignRole.Button.OnMeetingEnd();
            }
            if (multiAssignRole.AnotherRole is IRoleResetMeeting resetMultiAssignRole)
            {
                resetMultiAssignRole.ResetOnMeetingEnd(exiled);
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
