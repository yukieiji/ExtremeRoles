using System.Collections.Generic;
using HarmonyLib;

using UnityEngine;
using TMPro;

using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.CheckPoint;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Solo.Host;
using ExtremeRoles.Performance;
using ExtremeRoles.Patches.MapOverlay;

using PlayerHelper = ExtremeRoles.Helper.Player;
using CommomSystem = ExtremeRoles.Roles.API.Systems.Common;


namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.OnGameStart))]
public static class HudManagerOnGameStartPatch
{
	public static void Postfix()
	{
		if (PlayerControl.LocalPlayer == null)
		{
			return;
		}

		var gameStart = ExtremeGameModeManager.Instance.ShipOption.GameStart;

		if (!gameStart.IsKillCoolDownIsTen)
		{
			if (GameProgressSystem.IsTaskPhase)
			{
				var role = ExtremeRoleManager.GetLocalPlayerRole();
				PlayerControl.LocalPlayer.SetKillTimer(
					role.TryGetKillCool(out float killCoolTime) ?
					killCoolTime : PlayerHelper.DefaultKillCoolTime);
			}

			if (gameStart.RemoveSomeoneButton)
			{
				RemoveMeetingNumCheckpoint.RpcCheckpoint(gameStart.ReduceNum);
			}
		}
		ShipStatus.Instance.Timer = 15 - gameStart.FirstButtonCoolDown;
	}
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.SetAlertOverlay))]
public static class HudManagerSetAlertOverlayPatch
{
	public static bool Prefix()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null ||
			!GameProgressSystem.IsGameNow)
		{
			return true;
		}
		return Xion.PlayerId != localPlayer.PlayerId;
	}
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.SetTouchType))]
public static class HudManagerSetTouchTypePatch
{
	public static void Postfix()
	{
		if (GameSystem.IsFreePlay)
		{
			InfoOverlay.Instance.InitializeToGame();
		}
	}
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class HudManagerStartPatch
{
    public static void Postfix()
    {
		InfoOverlay.Instance.InitializeToLobby();
	}
}


[HarmonyPatch(typeof(HudManager), nameof(HudManager.ToggleMapVisible))]
public static class HudManagerToggleMapVisibletPatch
{
	public static void Prefix([HarmonyArgument(0)] ref MapOptions options)
	{
		if (options.Mode != MapOptions.Modes.CountOverlay ||
		   ExtremeRoleManager.GameRole.Count == 0 ||
		   ExtremeRoleManager.GetLocalPlayerRole().CanUseAdmin() ||
		   MapCountOverlayUpdatePatch.IsAbilityUse()) { return; }

		options.Mode = MapOptions.Modes.Normal;
	}
}


[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManagerUpdatePatch
{
    public const string RoleInfoObjectName = "Info";
    private const float infoScale = 0.25f;

    private static bool buttonCreated = false;
    private static bool isActiveUpdate = true;

    public static Dictionary<byte, TextMeshPro> PlayerInfoText => allPlayerInfo;

    private static Dictionary<byte, TextMeshPro> allPlayerInfo = new Dictionary<byte, TextMeshPro>();
    private static TextMeshPro tabText;

    public static void Reset()
    {
        allPlayerInfo.Clear();
        tabText = null;
        isActiveUpdate = true;
    }

    public static void SetBlockUpdate(bool isBlock)
    {
        isActiveUpdate = !isBlock;
    }

    public static bool Prefix(HudManager __instance)
    {
        if (OnemanMeetingSystemManager.IsActive)
        {
            __instance.UseButton.ToggleVisible(false);
            __instance.AbilityButton.ToggleVisible(false);
            __instance.ReportButton.ToggleVisible(false);
            __instance.KillButton.ToggleVisible(false);
            __instance.SabotageButton.ToggleVisible(false);
            __instance.ImpostorVentButton.ToggleVisible(false);
            __instance.TaskPanel.gameObject.SetActive(false);
            __instance.roomTracker.gameObject.SetActive(false);

            IVirtualJoystick virtualJoystick = __instance.joystick;

            if (virtualJoystick != null)
            {
                virtualJoystick.ToggleVisuals(false);
            }
        }
        return isActiveUpdate;
    }

    public static void Postfix()
    {
		var player = PlayerControl.LocalPlayer;

		if (!GameProgressSystem.IsGameNow || player == null)
		{
			return;
		}

        SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
        GhostRoleBase ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();

        resetNameTagsAndColors(player);

        bool blockCondition = isBlockCondition(player, role) || ghostRole != null;
        bool playeringInfoBlock = role.IsBlockShowPlayingRoleInfo() || ghostRole != null;

        if (role is MultiAssignRoleBase multiRole &&
            multiRole.AnotherRole != null)
        {
            blockCondition = blockCondition || isBlockCondition(
                player, multiRole.AnotherRole);
            playeringInfoBlock =
               playeringInfoBlock || multiRole.AnotherRole.IsBlockShowPlayingRoleInfo();
        }

        playerInfoUpdate(
            player,
            blockCondition,
            playeringInfoBlock);

        setPlayerNameColor(
            player,
            role, ghostRole,
            blockCondition,
            playeringInfoBlock);
        setPlayerNameTag(role);

        buttonCreate(role);
        roleUpdate(player, role);

        MultiAssignRoleBase multiAssignRole = role as MultiAssignRoleBase;
        if (multiAssignRole != null)
        {
            if (multiAssignRole.AnotherRole != null)
            {
                buttonCreate(multiAssignRole.AnotherRole);
                roleUpdate(player, multiAssignRole.AnotherRole);
                multiAssignRole.OverrideAnotherRoleSetting();
            }
        }

        /* TODO:幽霊役職タスク
        if (ghostRole != null)
        {
            role.HasTask = role.HasTask && ghostRole.HasTask;
        }
        */

    }
    private static void buttonCreate(SingleRoleBase checkRole)
    {
        if (buttonCreated) { return; }

        var abilityRole = checkRole as IRoleAbility;

        if (abilityRole != null)
        {
            if (abilityRole.Button == null)
            {
                buttonCreated = true; //一時的にブロック

                abilityRole.CreateAbility();
                abilityRole.RoleAbilityInit();

                buttonCreated = abilityRole.Button != null; // 作れたかどうか
            }
            else
            {
                buttonCreated = true;
            }
        }
    }

    private static void resetNameTagsAndColors(PlayerControl localPlayer)
    {
        foreach (PlayerControl player in PlayerCache.AllPlayerControl)
        {
			if (player == null)
			{
				continue;
			}

			player.cosmetics.SetName(player.CurrentOutfit.PlayerName);

            if (localPlayer.Data != null &&
				localPlayer.Data.Role != null &&
				localPlayer.Data.Role.IsImpostor &&
				player.Data != null &&
				player.Data.Role != null &&
				player.Data.Role.IsImpostor)
            {
                player.cosmetics.SetNameColor(Palette.ImpostorRed);
            }
            else
            {
                player.cosmetics.SetNameColor(Color.white);
            }
        }
    }

    private static void setPlayerNameColor(
        PlayerControl localPlayer,
        SingleRoleBase playerRole,
        GhostRoleBase playerGhostRole,
        bool blockCondition,
        bool playeringInfoBlock)
    {
        byte localPlayerId = localPlayer.PlayerId;

        // Modules.Helpers.DebugLog($"Player Name:{role.NameColor}");

        // まずは自分のプレイヤー名の色を変える
        Color localRoleColor = playerRole.GetNameColor(localPlayer.Data.IsDead);

        if (playerGhostRole != null)
        {
            Color ghostRoleColor = playerGhostRole.Color;
            localRoleColor = (localRoleColor / 2.0f) + (ghostRoleColor / 2.0f);
        }
        localPlayer.cosmetics.SetNameColor(localRoleColor);

        GhostRoleBase targetGhostRole;

        foreach (PlayerControl targetPlayer in PlayerCache.AllPlayerControl)
        {
            if (targetPlayer.PlayerId == localPlayerId)
			{
				continue;
			}

            byte targetPlayerId = targetPlayer.PlayerId;
            if (!ExtremeRoleManager.TryGetRole(targetPlayerId, out var targetRole))
            {
                ExtremeRolesPlugin.Logger.LogError($"Role not found for PlayerId: {targetPlayerId} in HudManagerPatch.setPlayerNameColor");
                continue;
            }

            ExtremeGhostRoleManager.GameRole.TryGetValue(targetPlayerId, out targetGhostRole);

            if (!ClientOption.Instance.GhostsSeeRole.Value ||
                !localPlayer.Data.IsDead ||
                blockCondition)
            {
                Color paintColor = playerRole.GetTargetRoleSeeColor(
                    targetRole, targetPlayerId);

                if (playerGhostRole != null)
                {
                    Color paintGhostColor = playerGhostRole.GetTargetRoleSeeColor(
                        targetPlayerId, targetRole, targetGhostRole);

                    if (paintGhostColor != Color.clear)
                    {
                        paintColor = (paintGhostColor / 2.0f) + (paintColor / 2.0f);
                    }
                }

                if (paintColor == Palette.ClearWhite) { continue; }

                targetPlayer.cosmetics.SetNameColor(paintColor);
            }
            else
            {
                Color roleColor = targetRole.GetNameColor(true);

                if (!playeringInfoBlock)
                {
                    targetPlayer.cosmetics.SetNameColor(roleColor);
                }
            }
        }
    }

    private static void setPlayerNameTag(
        SingleRoleBase playerRole)
    {

        foreach (PlayerControl targetPlayer in PlayerCache.AllPlayerControl)
        {
            byte playerId = targetPlayer.PlayerId;
            if (!ExtremeRoleManager.TryGetRole(playerId, out var targetRoleForTag))
            {
                ExtremeRolesPlugin.Logger.LogError($"Role not found for PlayerId: {playerId} in HudManagerPatch.setPlayerNameTag");
                continue;
            }
            string tag = playerRole.GetRolePlayerNameTag(targetRoleForTag, playerId);
            if (tag == string.Empty)
			{
				continue;
			}

            targetPlayer.cosmetics.nameText.text += tag;
        }
    }

    private static void playerInfoUpdate(
        PlayerControl localPlayer,
        bool blockCondition,
        bool playeringInfoBlock)
    {

        bool commsActive = PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(localPlayer);

        foreach (PlayerControl player in PlayerCache.AllPlayerControl)
        {

            if (player.PlayerId != localPlayer.PlayerId && !localPlayer.Data.IsDead)
            {
                continue;
            }


            if (!allPlayerInfo.TryGetValue(player.PlayerId, out TextMeshPro playerInfo) ||
                playerInfo == null)
            {
                playerInfo = UnityEngine.Object.Instantiate(
                    player.cosmetics.nameText,
                    player.cosmetics.nameText.transform.parent);
                playerInfo.fontSize *= 0.75f;
                playerInfo.gameObject.name = RoleInfoObjectName;
                allPlayerInfo[player.PlayerId] = playerInfo;
            }

            playerInfo.transform.localPosition =
                player.cosmetics.nameText.transform.localPosition + Vector3.up * infoScale;
            string playerInfoText = getRoleInfo(localPlayer, player, commsActive);
            playerInfo.text = playerInfoText;

            if (player.PlayerId == localPlayer.PlayerId)
            {
                playerInfo.gameObject.SetActive(player.Visible);
            }
            else if (blockCondition)
            {
                playerInfo.gameObject.SetActive(false);
            }
            else
            {
                playerInfo.gameObject.SetActive((player.Visible && !playeringInfoBlock));
            }
        }
    }

    private static bool isBlockCondition(
        PlayerControl localPlayer, SingleRoleBase role)
    {
        if (localPlayer.Data.Role.Role == RoleTypes.GuardianAngel)
        {
            return true;
        }
        else if (CommomSystem.IsForceInfoBlockRole(role))
        {
            return ExtremeRolesPlugin.ShipState.IsAssassinAssign;
        }

        return false;

    }

    private static string getRoleInfo(
        PlayerControl localPlayer,
        PlayerControl targetPlayer,
        bool commonActive)
    {

        var (tasksCompleted, tasksTotal) = GameSystem.GetTaskInfo(targetPlayer.Data);
        byte targetPlayerId = targetPlayer.PlayerId;
        string roleNames = ExtremeRoleManager.TryGetRole(targetPlayerId, out var targetRole)
			? targetRole.GetColoredRoleName(localPlayer.Data.IsDead) : "";

        if (ExtremeGhostRoleManager.GameRole.TryGetValue(targetPlayerId, out var ghostRole))
        {
            string ghostRoleName = ghostRole.GetColoredRoleName();
            roleNames = $"{ghostRoleName}({roleNames})";
        }

        string completedStr = commonActive ? "?" : tasksCompleted.ToString();
        string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({completedStr}/{tasksTotal})</color>" : "";

        string playerInfoText = "";

        var clientOption = ClientOption.Instance;
        bool isGhostSeeRole = clientOption.GhostsSeeRole.Value;
        bool isGhostSeeTask = clientOption.GhostsSeeTask.Value;

        if (targetPlayer.PlayerId == localPlayer.PlayerId)
        {
            playerInfoText = $"{roleNames}";

            if (HudManager.InstanceExists)
            {
                if (tabText == null)
                {
                    tabText = HudManager.Instance.TaskPanel.tab.transform.FindChild(
                        "TabText_TMP").GetComponent<TextMeshPro>();
                }
                tabText.SetText(
                    $"{TranslationController.Instance.GetString(StringNames.Tasks)} {taskInfo}");
            }
        }
        else if (isGhostSeeRole && isGhostSeeTask)
        {
            playerInfoText = $"{roleNames} {taskInfo}".Trim();
        }
        else if (isGhostSeeTask)
        {
            playerInfoText = $"{taskInfo}".Trim();
        }
        else if (isGhostSeeRole)
        {
            playerInfoText = $"{roleNames}";
        }
        return playerInfoText;
    }


    private static void roleUpdate(
        PlayerControl player, SingleRoleBase checkRole)
    {
        var updatableRole = checkRole as IRoleUpdate;
        if (updatableRole != null)
        {
            updatableRole.Update(player);
        }
    }

}
