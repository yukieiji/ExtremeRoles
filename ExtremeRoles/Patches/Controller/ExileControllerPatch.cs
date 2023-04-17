using System;
using System.Linq;
using HarmonyLib;

using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.Module;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

using Il2CppObject = Il2CppSystem.Object;

namespace ExtremeRoles.Patches.Controller;

[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
public static class ExileControllerBeginePatch
{
    public static bool Prefix(
        ExileController __instance,
        [HarmonyArgument(0)] GameData.PlayerInfo exiled,
        [HarmonyArgument(1)] bool tie)
    {

        var state = ExtremeRolesPlugin.ShipState;
        bool confirmImp = GameManager.Instance.LogicOptions.GetConfirmImpostor();
        var shipOption = ExtremeGameModeManager.Instance.ShipOption;

        if (state.AssassinMeetingTrigger)
        {
            assassinMeetingEndBegin(__instance, state);
            return false; 
        }
        else if (GameManager.Instance.LogicOptions.GetConfirmImpostor())
        {

            __instance.StartCoroutine(__instance.Animate());
            return false;
        }
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

    private static void assassinMeetingEndBegin(
        ExileController instance, ExtremeShipStatus state)
    {
        setExiledTarget(instance, null);
        GameData.PlayerInfo player = GameData.Instance.GetPlayerById(
            state.IsMarinPlayerId);

        string transKey = state.IsAssassinAssign ?
            "assassinateMarinSucsess" : "assassinateMarinFail";
        string printStr = $"{player?.PlayerName}{Helper.Translation.GetString(transKey)}";

        if (instance.Player)
        {
            instance.Player.gameObject.SetActive(false);
        }
        instance.completeString = printStr;
        instance.ImpostorText.text = string.Empty;

        instance.StartCoroutine(instance.Animate());
    }

    private static void confirmExil(
        ExileController instance,
        GameData.PlayerInfo exiled,
        ConfirmExilMode mode, bool isShowRole, bool tie)
    {
        setExiledTarget(instance, exiled);
        var transController = FastDestroyableSingleton<TranslationController>.Instance;

        var allPlayer = GameData.Instance.AllPlayers.ToArray();
        var alivePlayers = allPlayer.Where(
            x => x.PlayerId != exiled.PlayerId && x.IsDead && x.Disconnected);
        var allRoles = ExtremeRoleManager.GameRole;

        int aliveImpNum = Enumerable.Count(
            alivePlayers,
            (GameData.PlayerInfo p) =>
            {
                return allRoles[p.PlayerId].IsImpostor();
            });
        int aliveCrewNum = Enumerable.Count(
            alivePlayers,
            (GameData.PlayerInfo p) =>
            {
                return allRoles[p.PlayerId].IsCrewmate();
            });
        int aliveNeutNum = Enumerable.Count(
            alivePlayers,
            (GameData.PlayerInfo p) =>
            {
                return allRoles[p.PlayerId].IsNeutral();
            });

        string completeString = string.Empty;

        if (exiled != null)
        {
            string playerName = exiled.PlayerName;
            var exiledPlayerRole = allRoles[exiled.PlayerId];
            switch (mode)
            {
                case ConfirmExilMode.Impostor:
                    int allImpNum = Enumerable.Count(
                        allPlayer, (GameData.PlayerInfo p) => allRoles[p.PlayerId].IsImpostor());
                    bool isExiledIsImp = exiledPlayerRole.IsImpostor();
                    StringNames stringKey;
                    if (isExiledIsImp && allImpNum > 1)
                    {
                        stringKey = StringNames.ExileTextPP;
                    }
                    else if (isExiledIsImp)
                    {
                        stringKey = StringNames.ExileTextSP;
                    }
                    else if (allImpNum > 1)
                    {
                        stringKey = StringNames.ExileTextPN;
                    }
                    else
                    {
                        stringKey = StringNames.ExileTextSN;
                    }
                    completeString = transController.GetString(
                        stringKey, new Il2CppObject[] { playerName });
                    break;
                case ConfirmExilMode.Crewmate:
                    int allCrewNum = Enumerable.Count(
                        allPlayer, (GameData.PlayerInfo p) => allRoles[p.PlayerId].IsCrewmate());
                    bool isExiledIsCrew = exiledPlayerRole.IsCrewmate();
                    string transCrewStringKey;
                    if (isExiledIsCrew && allCrewNum > 1)
                    {
                        transCrewStringKey = "ExileTextPPCrew";
                    }
                    else if (isExiledIsCrew)
                    {
                        transCrewStringKey = "ExileTextSPCrew";
                    }
                    else if (allCrewNum > 1)
                    {
                        transCrewStringKey = "ExileTextPNCrew";
                    }
                    else
                    {
                        transCrewStringKey = "ExileTextSNCrew";
                    }
                    completeString = string.Format(
                        Helper.Translation.GetString(transCrewStringKey), playerName);
                    break;
                case ConfirmExilMode.Neutral:
                    int allNeutNum = Enumerable.Count(
                        allPlayer, (GameData.PlayerInfo p) => allRoles[p.PlayerId].IsCrewmate());
                    bool isExiledIsNeut = exiledPlayerRole.IsCrewmate();
                    string transNeutStringKey;
                    if (isExiledIsNeut && allNeutNum > 1)
                    {
                        transNeutStringKey = "ExileTextPPNeut";
                    }
                    else if (isExiledIsNeut)
                    {
                        transNeutStringKey = "ExileTextSPNeut";
                    }
                    else if (allNeutNum > 1)
                    {
                        transNeutStringKey = "ExileTextPNNeut";
                    }
                    else
                    {
                        transNeutStringKey = "ExileTextSNNeut";
                    }
                    completeString = string.Format(
                        Helper.Translation.GetString(transNeutStringKey), playerName);
                    break;
                case ConfirmExilMode.AllTeam:
                    completeString = string.Format(
                        Helper.Translation.GetString("ExileTextAllTeam"),
                        exiledPlayerRole.Team,
                        playerName);
                    break;
                default:
                    break;
            }

            instance.Player.UpdateFromEitherPlayerDataOrCache(
                exiled, PlayerOutfitType.Default, PlayerMaterial.MaskType.Exile, false);
            instance.Player.ToggleName(false);
            instance.Player.SetCustomHatPosition(instance.exileHatPosition);
            instance.Player.SetCustomVisorPosition(instance.exileVisorPosition);

            SkinViewData skin = ShipStatus.Instance.CosmeticsCache.GetSkin(
                exiled.Outfits[PlayerOutfitType.Default].SkinId);
            instance.Player.FixSkinSprite(skin.EjectFrame);
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
            ConfirmExilMode.Impostor => transController.GetString(
                aliveImpNum == 1 ? StringNames.ImpostorsRemainS : StringNames.ImpostorsRemainP,
                new Il2CppObject[] { aliveImpNum }),

            ConfirmExilMode.Crewmate => string.Format(
                Helper.Translation.GetString(
                    aliveCrewNum == 1 ?
                        "CrewmateRemainS" :
                        "CrewmateRemainP"), aliveCrewNum),

            ConfirmExilMode.Neutral => string.Format(
                Helper.Translation.GetString(
                    aliveNeutNum == 1 ?
                        "NeutralRemainS" :
                        "NeutralRemainP"), aliveNeutNum),

            ConfirmExilMode.AllTeam => string.Format(
                Helper.Translation.GetString("AllTeamAlive"),
                aliveCrewNum, aliveImpNum, aliveNeutNum),

            _ => string.Empty
        };
    }

    private static void setExiledTarget(
        ExileController instance, GameData.PlayerInfo player)
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

        if (MeetingReporter.IsExist)
        {
            MeetingReporter.Instance.Destroy();
        }

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
