using System;
using System.Collections.Generic;
using System.Linq;
using Assets.CoreScripts;

using HarmonyLib;
using Hazel;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Patches
{

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoStartMeeting))]
    public class PlayerControlCoStartMeetingPatch
    {
        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo meetingTarget)
        {
            var gameData = ExtremeRolesPlugin.GameDataStore;

            if (gameData.AssassinMeetingTrigger) { return; }

            // Count meetings
            if (meetingTarget == null)
            {
                ++gameData.MeetingsCount;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class PlayerControlExiledPatch
    {
        public static void Postfix(PlayerControl __instance)
        {

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            ExtremeRolesPlugin.GameDataStore.AddDeadInfo(
                __instance, DeathReason.Exile, null);

            var role = ExtremeRoleManager.GameRole[__instance.PlayerId];

            if (!role.HasTask || role.IsNeutral())
            {
                __instance.ClearTasks();
            }
        }

    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public class PlayerControlFixedUpdatePatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (!ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd()) { return; }
            if (ExtremeRoleManager.GameRole.Count == 0) { return; }
            if (PlayerControl.LocalPlayer != __instance) { return; }

            resetNameTagsAndColors();

            var role = ExtremeRoleManager.GetLocalPlayerRole();

            playerInfoUpdate(role);
            setPlayerNameColor(__instance, role);
            setPlayerNameTag(role);
            buttonUpdate(__instance, role);
            refreshRoleDescription(__instance, role);

            ExtremeRolesPlugin.GameDataStore.History.Enqueue(__instance);
            ExtremeRolesPlugin.GameDataStore.Union.Update();
        }

        private static void resetNameTagsAndColors()
        {

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.CurrentOutfitType != PlayerOutfitType.Shapeshifted)
                {
                    player.nameText.text = player.Data.PlayerName;
                }
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && player.Data.Role.IsImpostor)
                {
                    player.nameText.color = Palette.ImpostorRed;
                }
                else
                {
                    player.nameText.color = Color.white;
                }
                if (MeetingHud.Instance != null)
                {
                    foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
                    {
                        if (pva.TargetPlayerId != player.PlayerId) { continue; }
                        
                        pva.NameText.text = player.Data.PlayerName;
                       
                        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor &&
                            player.Data.Role.IsImpostor)
                        {
                            pva.NameText.color = Palette.ImpostorRed;
                        }
                        else
                        {
                            pva.NameText.color = Palette.White;
                        }
                        break;
                    }
                }
            }

            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
            {
                List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList();
                impostors.RemoveAll(x => !(x.Data.Role.IsImpostor));
                foreach (PlayerControl player in impostors)
                {
                    player.nameText.color = Palette.ImpostorRed;
                    if (MeetingHud.Instance != null)
                    {
                        foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
                        {
                            if (player.PlayerId != pva.TargetPlayerId) { continue; }
                            pva.NameText.color = Palette.ImpostorRed;
                        }
                    }
                }
            }

        }

        private static void setPlayerNameColor(
            PlayerControl player,
            SingleRoleBase playerRole)
        {
            var localPlayerId = player.PlayerId;

            bool voteNamePaintBlock = false;
            bool playerNamePaintBlock = false;
            bool isBlocked = ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger;
            if (playerRole.Id == ExtremeRoleId.Assassin)
            {
                voteNamePaintBlock = true;
                playerNamePaintBlock = ((Roles.Combination.Assassin)playerRole).CanSeeRoleBeforeFirstMeeting || isBlocked;
            }

            // Modules.Helpers.DebugLog($"Player Name:{role.NameColor}");

            // まずは自分のプレイヤー名の色を変える
            player.nameText.color = playerRole.NameColor;
            setVoteAreaColor(localPlayerId, playerRole.NameColor);

            foreach (PlayerControl targetPlayer in PlayerControl.AllPlayerControls)
            {
                if (targetPlayer.PlayerId == player.PlayerId) { continue; }

                byte targetPlayerId = targetPlayer.PlayerId;

                if (!OptionHolder.Client.GhostsSeeRole || 
                    !PlayerControl.LocalPlayer.Data.IsDead || 
                    PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel)
                {
                    var targetRole = ExtremeRoleManager.GameRole[targetPlayerId];
                    Color paintColor = playerRole.GetTargetRoleSeeColor(
                        targetRole, targetPlayerId);
                    if (paintColor == Palette.ClearWhite) { continue; }

                    targetPlayer.nameText.color = paintColor;
                    setVoteAreaColor(targetPlayerId, paintColor);
                }
                else
                {
                    var targetPlayerRole = ExtremeRoleManager.GameRole[
                        targetPlayerId];
                    Color roleColor = targetPlayerRole.NameColor;
                    if (!playerNamePaintBlock)
                    {
                        targetPlayer.nameText.color = roleColor;
                    }
                    setGhostVoteAreaColor(
                        targetPlayerId,
                        roleColor,
                        voteNamePaintBlock,
                        targetPlayerRole.Team == playerRole.Team);
                }
            }

        }

        private static void setPlayerNameTag(
            SingleRoleBase playerRole)
        {

            foreach (PlayerControl targetPlayer in PlayerControl.AllPlayerControls)
            {
                byte playerId = targetPlayer.PlayerId;
                string tag = playerRole.GetRolePlayerNameTag(
                    ExtremeRoleManager.GameRole[playerId], playerId);
                if (tag == string.Empty) { continue; }

                targetPlayer.nameText.text += tag;

                if (MeetingHud.Instance != null)
                {
                    foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
                    {
                        if (targetPlayer.PlayerId != pva.TargetPlayerId) { continue; }
                        pva.NameText.text += tag;
                    }
                }
            }
        }

        private static void setVoteAreaColor(
            byte targetPlayerId,
            Color targetColor)
        {
            if (MeetingHud.Instance != null)
            {
                foreach (PlayerVoteArea voteArea in MeetingHud.Instance.playerStates)
                {
                    if (voteArea.NameText != null && targetPlayerId == voteArea.TargetPlayerId)
                    {
                        voteArea.NameText.color = targetColor;
                    }
                }
            }
        }

        private static void setGhostVoteAreaColor(
            byte targetPlayerId,
            Color targetColor,
            bool voteNamePaintBlock,
            bool isSameTeam)
        {
            if (MeetingHud.Instance != null)
            {
                foreach (PlayerVoteArea voteArea in MeetingHud.Instance.playerStates)
                {
                    if (voteArea.NameText != null &&
                        targetPlayerId == voteArea.TargetPlayerId &&
                        (!voteNamePaintBlock || isSameTeam))
                    {
                        voteArea.NameText.color = targetColor;
                    }
                }
            }
        }

        private static void playerInfoUpdate(
            SingleRoleBase playerRole)
        {

            bool commsActive = false;
            foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks)
            {
                if (t.TaskType == TaskTypes.FixComms)
                {
                    commsActive = true;
                    break;
                }
            }

            var isBlocked = ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger;

            if (playerRole.Id == ExtremeRoleId.Assassin)
            {
                isBlocked = ((Roles.Combination.Assassin)playerRole).IsFirstMeeting || isBlocked;
            }

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {

                if ((player != PlayerControl.LocalPlayer && 
                        !PlayerControl.LocalPlayer.Data.IsDead))
                {
                    continue;
                }

                Transform playerInfoTransform = player.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;

                if (playerInfo == null)
                {
                    playerInfo = UnityEngine.Object.Instantiate(
                        player.nameText, player.nameText.transform.parent);
                    playerInfo.fontSize *= 0.75f;
                    playerInfo.gameObject.name = "Info";
                }

                // Set the position every time bc it sometimes ends up in the wrong place due to camoflauge
                playerInfo.transform.localPosition = player.nameText.transform.localPosition + Vector3.up * 0.5f;

                PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == player.PlayerId);
                Transform meetingInfoTransform = playerVoteArea != null ? playerVoteArea.NameText.transform.parent.FindChild("Info") : null;
                TMPro.TextMeshPro meetingInfo = meetingInfoTransform != null ? meetingInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (meetingInfo == null && playerVoteArea != null)
                {
                    meetingInfo = UnityEngine.Object.Instantiate(
                        playerVoteArea.NameText,
                        playerVoteArea.NameText.transform.parent);
                    meetingInfo.transform.localPosition += Vector3.down * 0.20f;
                    meetingInfo.fontSize *= 0.63f;
                    meetingInfo.autoSizeTextContainer = true;
                    meetingInfo.gameObject.name = "Info";
                }

                var (playerInfoText, meetingInfoText) = getRoleAndMeetingInfo(player, commsActive, isBlocked);
                playerInfo.text = playerInfoText;
                playerInfo.gameObject.SetActive(player.Visible);

                if (meetingInfo != null)
                {
                    meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : meetingInfoText;
                }

                if ((player != PlayerControl.LocalPlayer &&
                    PlayerControl.LocalPlayer.Data.IsDead &&
                    PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel))
                {
                    playerInfo.gameObject.SetActive(false);
                    if (meetingInfo != null)
                    {
                        meetingInfo.gameObject.SetActive(false);
                    }
                }

            }
        }

        private static Tuple<string, string> getRoleAndMeetingInfo(
            PlayerControl targetPlayer, bool commonActive,
            bool IsLocalPlayerAssassinFirstMeeting = false)
        {

            var (tasksCompleted, tasksTotal) = GameSystem.GetTaskInfo(targetPlayer.Data);
            string roleNames = ExtremeRoleManager.GameRole[targetPlayer.PlayerId].GetColoredRoleName();

            var completedStr = commonActive ? "?" : tasksCompleted.ToString();
            string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({completedStr}/{tasksTotal})</color>" : "";

            string playerInfoText = "";
            string meetingInfoText = "";

            if (targetPlayer == PlayerControl.LocalPlayer)
            {
                playerInfoText = $"{roleNames}";
                if (DestroyableSingleton<TaskPanelBehaviour>.InstanceExists)
                {
                    TMPro.TextMeshPro tabText = DestroyableSingleton<
                        TaskPanelBehaviour>.Instance.tab.transform.FindChild("TabText_TMP").GetComponent<TMPro.TextMeshPro>();
                    tabText.SetText($"{TranslationController.Instance.GetString(StringNames.Tasks)} {taskInfo}");
                }
                meetingInfoText = $"{roleNames} {taskInfo}".Trim();
            }
            else if (IsLocalPlayerAssassinFirstMeeting)
            {

                Roles.Combination.Assassin role = ExtremeRoleManager.GetLocalPlayerRole() as Roles.Combination.Assassin;
                if (role != null)
                {
                    if(role.CanSeeRoleBeforeFirstMeeting && OptionHolder.Client.GhostsSeeRole)
                    {
                        playerInfoText = $"{roleNames}";
                    }
                }

            }
            else if (OptionHolder.Client.GhostsSeeRole && OptionHolder.Client.GhostsSeeTask)
            {
                playerInfoText = $"{roleNames} {taskInfo}".Trim();
                meetingInfoText = playerInfoText;
            }
            else if (OptionHolder.Client.GhostsSeeTask)
            {
                playerInfoText = $"{taskInfo}".Trim();
                meetingInfoText = playerInfoText;
            }
            else if (OptionHolder.Client.GhostsSeeRole)
            {
                playerInfoText = $"{roleNames}";
                meetingInfoText = playerInfoText;
            }

            return Tuple.Create(playerInfoText, meetingInfoText);

        }
        private static void refreshRoleDescription(
            PlayerControl player,
            SingleRoleBase playerRole)
        {

            var removedTask = new List<PlayerTask>();
            foreach (PlayerTask task in player.myTasks)
            {
                if (task == null) { return; }

                var textTask = task.gameObject.GetComponent<ImportantTextTask>();
                if (textTask != null)
                {
                    removedTask.Add(task); // TextTask does not have a corresponding RoleInfo and will hence be deleted
                }
            }

            foreach (PlayerTask task in removedTask)
            {
                task.OnRemove();
                player.myTasks.Remove(task);
                UnityEngine.Object.Destroy(task.gameObject);
            }

            var importantTextTask = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
            importantTextTask.transform.SetParent(player.transform, false);

            importantTextTask.Text = playerRole.GetImportantText();
            player.myTasks.Insert(0, importantTextTask);

        }
        private static void buttonUpdate(
            PlayerControl player,
            SingleRoleBase playerRole)
        {
            if (!player.AmOwner) { return; }

            bool enable = Player.ShowButtons && !PlayerControl.LocalPlayer.Data.IsDead;

            killButtonUpdate(player, playerRole, enable);
            ventButtonUpdate(playerRole, enable);

            sabotageButtonUpdate(playerRole);
            roleAbilityButtonUpdate(playerRole);
        }

        private static void killButtonUpdate(
            PlayerControl player,
            SingleRoleBase role, bool enable)
        {

            bool isImposter = role.IsImpostor();

            if (role.CanKill)
            {
                if (enable)
                {

                    if (!isImposter && player.CanMove)
                    {
                        player.SetKillTimer(player.killTimer - Time.fixedDeltaTime);
                    }

                    PlayerControl target = player.FindClosestTarget(!isImposter);

                    // Logging.Debug($"TargetAlive?:{target}");

                    DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(target);
                    Player.SetPlayerOutLine(target, role.NameColor);
                    HudManager.Instance.KillButton.Show();
                    HudManager.Instance.KillButton.gameObject.SetActive(true);
                }
                else
                {
                    HudManager.Instance.KillButton.SetDisabled();
                }
            }
            else if (isImposter)
            {
                HudManager.Instance.KillButton.SetDisabled();
            }
        }

        private static void roleAbilityButtonUpdate(
            SingleRoleBase role)
        {
            void buttonUpdate(SingleRoleBase role)
            {
                var abilityRole = role as IRoleAbility;

                if (abilityRole != null)
                {
                    abilityRole.Button.Update();
                }
            }

            buttonUpdate(role);

            var multiAssignRole = role as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    buttonUpdate(multiAssignRole.AnotherRole);
                }
            }

        }

        private static void sabotageButtonUpdate(
            SingleRoleBase role)
        {

            bool enable = Player.ShowButtons;

            if (role.UseSabotage)
            {
                // インポスターは死んでもサボタージ使える
                if (enable && role.IsImpostor())
                {
                    HudManager.Instance.SabotageButton.Show();
                    HudManager.Instance.SabotageButton.gameObject.SetActive(true);
                }
                // それ以外は死んでないときだけサボタージ使える
                else if(enable && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    HudManager.Instance.SabotageButton.Show();
                    HudManager.Instance.SabotageButton.gameObject.SetActive(true);
                }
                else
                {
                    HudManager.Instance.SabotageButton.SetDisabled();
                }
            }
            else
            {
                HudManager.Instance.SabotageButton.SetDisabled();
            }
        }

        private static void ventButtonUpdate(
            SingleRoleBase role, bool enable)
        {
            if (role.UseVent)
            {
                if (!role.IsVanillaRole())
                {
                    if (enable) { HudManager.Instance.ImpostorVentButton.Show(); }
                    else { HudManager.Instance.ImpostorVentButton.SetDisabled(); }
                }
                else
                {
                    if (((Roles.Solo.VanillaRoleWrapper)role).VanilaRoleId == RoleTypes.Engineer)
                    {
                        if (enable)
                        {
                            if (!OptionHolder.AllOption[
                                    (int)OptionHolder.CommonOptionKey.EngineerUseImpostorVent].GetValue())
                            {
                                HudManager.Instance.AbilityButton.Show();
                            }
                            else
                            {
                                HudManager.Instance.ImpostorVentButton.Show();
                                HudManager.Instance.AbilityButton.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            HudManager.Instance.ImpostorVentButton.SetDisabled();
                            HudManager.Instance.AbilityButton.SetDisabled(); 
                        }
                    }
                }
            }
            else
            {
                HudManager.Instance.ImpostorVentButton.SetDisabled();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FindClosestTarget))]
    public class PlayerControlFindClosestTargetPatch
    {
        static bool Prefix(
            PlayerControl __instance,
            ref PlayerControl __result,
            [HarmonyArgument(0)] bool protecting)
        {

            if (!ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd()) { return true; }

            var gameRoles = ExtremeRoleManager.GameRole;

            if (gameRoles.Count == 0) { return true; }

            var role = gameRoles[__instance.PlayerId];

            __result = null;

            int killRange = PlayerControl.GameOptions.KillDistance;
            if (role.HasOtherKillRange)
            {
                killRange = role.KillRange;
            }

            float num = GameOptionsData.KillDistances[Mathf.Clamp(killRange, 0, 2)];
            
            if (!ShipStatus.Instance)
            {
                return false;
            }
            Vector2 truePosition = __instance.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;

            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                
                if (!playerInfo.Disconnected && 
                    (playerInfo.PlayerId != __instance.PlayerId) && 
                    !playerInfo.IsDead && 
                    !role.IsSameTeam(gameRoles[playerInfo.PlayerId]) && 
                    (!playerInfo.Object.inVent || OptionHolder.Ship.CanKillVentInPlayer))
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object && @object.Collider.enabled)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && 
                            !PhysicsHelpers.AnyNonTriggersBetween(
                                truePosition, vector.normalized,
                                magnitude, Constants.ShipAndObjectsMask))
                        {
                            __result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return false;
        }
    }


    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    public class PlayerControlHandleRpcPatch
    {
        static void Postfix(
            PlayerControl __instance,
            [HarmonyArgument(0)] byte callId,
            [HarmonyArgument(1)] MessageReader reader)
        {

            if (__instance == null || reader == null) { return; }

            switch ((RPCOperator.Command)callId)
            {
                case RPCOperator.Command.CleanDeadBody:
                    byte deadBodyPlayerId = reader.ReadByte();
                    RPCOperator.CleanDeadBody(deadBodyPlayerId);
                    break;
                case RPCOperator.Command.ForceEnd:
                    RPCOperator.ForceEnd();
                    break;
                case RPCOperator.Command.Initialize:
                    RPCOperator.Initialize();
                    break;
                case RPCOperator.Command.FixLightOff:
                    RPCOperator.FixLightOff();
                    break;
                case RPCOperator.Command.SetNormalRole:
                    byte normalRoleId = reader.ReadByte();
                    byte normalRoleAssignPlayerId = reader.ReadByte();
                    RPCOperator.SetNormalRole(
                        normalRoleId, normalRoleAssignPlayerId);
                    break;
                case RPCOperator.Command.SetCombinationRole:
                    byte combinationRoleId = reader.ReadByte();
                    byte combinationRoleAssignPlayerId = reader.ReadByte();
                    byte gameControlId = reader.ReadByte();
                    byte bytedRoleType = reader.ReadByte();
                    RPCOperator.SetCombinationRole(
                        combinationRoleId,
                        combinationRoleAssignPlayerId,
                        gameControlId, bytedRoleType);
                    break;
                case RPCOperator.Command.ShareOption:
                    int numOptions = (int)reader.ReadPackedUInt32();
                    RPCOperator.ShareOption(numOptions, reader);
                    break;
                case RPCOperator.Command.ReplaceRole:
                    byte targetPlayerId = reader.ReadByte();
                    byte replaceTarget = reader.ReadByte();
                    byte ops = reader.ReadByte();
                    RPCOperator.ReplaceRole(
                        targetPlayerId, replaceTarget, ops);
                    break;
                case RPCOperator.Command.CustomVentUse:
                    int ventId = reader.ReadPackedInt32();
                    byte ventingPlayer = reader.ReadByte();
                    byte isEnter = reader.ReadByte();
                    RPCOperator.CustomVentUse(ventId, ventingPlayer, isEnter);
                    break;
                case RPCOperator.Command.StartVentAnimation:
                    int animationVentId = reader.ReadPackedInt32();
                    RPCOperator.StartVentAnimation(animationVentId);
                    break;
                case RPCOperator.Command.UncheckedShapeShift:
                    byte shapeShiftPlayerId = reader.ReadByte();
                    byte shapeShiftTargetPlayerId = reader.ReadByte();
                    byte shapeShiftAnimationTrigger = reader.ReadByte();
                    RPCOperator.UncheckedShapeShift(
                        shapeShiftPlayerId,
                        shapeShiftTargetPlayerId,
                        shapeShiftAnimationTrigger);
                    break;
                case RPCOperator.Command.UncheckedMurderPlayer:
                    byte sourceId = reader.ReadByte();
                    byte targetId = reader.ReadByte();
                    byte killAnimationTrigger = reader.ReadByte();
                    RPCOperator.UncheckedMurderPlayer(
                        sourceId, targetId, killAnimationTrigger);
                    break;
                case RPCOperator.Command.ReplaceDeadReason:
                    byte changePlayerId = reader.ReadByte();
                    byte reason = reader.ReadByte();
                    RPCOperator.ReplaceDeadReason(
                        changePlayerId, reason);
                    break;
                case RPCOperator.Command.SetWinGameControlId:
                    int id = reader.ReadInt32();
                    RPCOperator.SetWinGameControlId(id);
                    break;
                case RPCOperator.Command.SetWinPlayer:
                    int playerNum = reader.ReadInt32();
                    List<byte> winPlayerId = new List<byte>();
                    for (int i= 0; i < playerNum; ++i)
                    {
                        winPlayerId.Add(reader.ReadByte());
                    }
                    RPCOperator.SetWinPlayer(winPlayerId);
                    break;
                case RPCOperator.Command.SetRoleWin:
                    byte rolePlayerId = reader.ReadByte();
                    RPCOperator.SetRoleWin(rolePlayerId);
                    break;
                case RPCOperator.Command.ShareMapId:
                    byte mapId = reader.ReadByte();
                    RPCOperator.ShareMapId(mapId);
                    break;
                case RPCOperator.Command.ShareVersion:
                    int major = reader.ReadInt32();
                    int minor = reader.ReadInt32();
                    int build = reader.ReadInt32();
                    int revision = reader.ReadInt32();
                    int clientId = reader.ReadPackedInt32();
                    RPCOperator.AddVersionData(
                        major, minor, build,
                        revision, clientId);
                    break;
                case RPCOperator.Command.BodyGuardFeatShield:
                    byte bodyGuardFeatShieldOpCallPlayerId = reader.ReadByte();
                    byte featShieldTargePlayerId = reader.ReadByte();
                    RPCOperator.BodyGuardFeatShield(
                        bodyGuardFeatShieldOpCallPlayerId,
                        featShieldTargePlayerId);
                    break;
                case RPCOperator.Command.BodyGuardResetShield:
                    byte bodyGuardResetShieldOpCallPlayerId = reader.ReadByte();
                    RPCOperator.BodyGuardResetShield(
                        bodyGuardResetShieldOpCallPlayerId);
                    break;
                case RPCOperator.Command.TimeMasterShieldOn:
                    byte shieldOnTimeMaster = reader.ReadByte();
                    RPCOperator.TimeMasterShieldOn(
                        shieldOnTimeMaster);
                    break;
                case RPCOperator.Command.TimeMasterShieldOff:
                    byte shieldOffTimeMaster = reader.ReadByte();
                    RPCOperator.TimeMasterShieldOff(
                        shieldOffTimeMaster);
                    break;
                case RPCOperator.Command.TimeMasterRewindTime:
                    byte timeMasterPlayerId = reader.ReadByte();
                    RPCOperator.TimeMasterRewindTime(
                        timeMasterPlayerId);
                    break;
                case RPCOperator.Command.TimeMasterResetMeeting:
                    byte timeMasterResetPlayerId = reader.ReadByte();
                    RPCOperator.TimeMasterResetMeeting(
                        timeMasterResetPlayerId);
                    break;
                case RPCOperator.Command.AgencyTakeTask:
                    byte agencyTargetPlayerId = reader.ReadByte();
                    int getTaskNum = reader.ReadInt32();

                    List<int> getTaskId = new List<int> ();
                    
                    for (int i = 0; i < getTaskNum; ++i)
                    {
                        getTaskId.Add(reader.ReadInt32());
                    }

                    RPCOperator.AgencyTakeTask(
                        agencyTargetPlayerId, getTaskId);
                    break;
                case RPCOperator.Command.AgencySetNewTask:
                    byte agencyCallerId = reader.ReadByte();
                    int taskSetIndex = reader.ReadInt32();
                    int newTaskId = reader.ReadInt32();
                    RPCOperator.AgencySetNewTask(
                        agencyCallerId, taskSetIndex, newTaskId);
                    break;
                case RPCOperator.Command.FencerCounterOn:
                    byte counterOnTimeFencer = reader.ReadByte();
                    RPCOperator.FencerCounterOn(
                        counterOnTimeFencer);
                    break;
                case RPCOperator.Command.FencerCounterOff:
                    byte counterOffTimeFencer = reader.ReadByte();
                    RPCOperator.FencerCounterOff(
                        counterOffTimeFencer);
                    break;
                case RPCOperator.Command.FencerEnableKillButton:
                    byte fencerPlayerId = reader.ReadByte();
                    RPCOperator.FencerEnableKillButton(
                        fencerPlayerId);
                    break;
                case RPCOperator.Command.CuresMakerCurseKillCool:
                    byte curesMakerPlayerId = reader.ReadByte();
                    byte curesPlayerId = reader.ReadByte();
                    RPCOperator.CuresMakerCurseKillCool(
                        curesMakerPlayerId, curesPlayerId);
                    break;
                case RPCOperator.Command.AssasinVoteFor:
                    byte voteTargetId = reader.ReadByte();
                    RPCOperator.AssasinVoteFor(voteTargetId);
                    break;
                case RPCOperator.Command.CarrierCarryBody:
                    byte carrierCarryOpCallPlayerId = reader.ReadByte();
                    byte carryDeadBodyPlayerId = reader.ReadByte();
                    RPCOperator.CarrierCarryBody(
                        carrierCarryOpCallPlayerId,
                        carryDeadBodyPlayerId);
                    break;
                case RPCOperator.Command.CarrierSetBody:
                    byte carrierSetOpCallPlayerId = reader.ReadByte();
                    RPCOperator.CarrierSetBody(
                        carrierSetOpCallPlayerId);
                    break;
                case RPCOperator.Command.PainterPaintBody:
                    byte painterPlayerId = reader.ReadByte();
                    byte paintDeadBodyPlayerId = reader.ReadByte();
                    RPCOperator.PainterPaintBody(
                        painterPlayerId,
                        paintDeadBodyPlayerId);
                    break;
                case RPCOperator.Command.FakerCreateDummy:
                    byte fakerPlayerId = reader.ReadByte();
                    byte colorTargetId = reader.ReadByte();
                    RPCOperator.FakerCreateDummy(
                        fakerPlayerId, colorTargetId);
                    break;
                case RPCOperator.Command.OverLoaderSwitchAbility:
                    byte overLoaderPlayerId = reader.ReadByte();
                    byte activate  = reader.ReadByte();
                    RPCOperator.OverLoaderSwitchAbility(
                        overLoaderPlayerId, activate);
                    break;
                case RPCOperator.Command.CrackerCrackDeadBody:
                    byte crackerId = reader.ReadByte();
                    byte crackTarget = reader.ReadByte();
                    RPCOperator.CrackerCrackDeadBody(crackerId, crackTarget);
                    break;
                case RPCOperator.Command.SlaveDriverSetNewTask:
                    byte slaveDriverId = reader.ReadByte();
                    int replaceTaskIndex = reader.ReadInt32();
                    int setTaskId = reader.ReadInt32();
                    RPCOperator.SlaveDriverSetNewTask(
                        slaveDriverId, replaceTaskIndex, setTaskId);
                    break;
                case RPCOperator.Command.AliceShipBroken:
                    byte alicePlayerId = reader.ReadByte();
                    byte newTaskSetPlayerId = reader.ReadByte();
                    int newTaskNum = reader.ReadInt32();

                    List<int> task = new List<int>();

                    for (int i = 0; i < newTaskNum; ++i)
                    {
                        task.Add(reader.ReadInt32());
                    }
                    RPCOperator.AliceShipBroken(
                        alicePlayerId, newTaskSetPlayerId, task);
                    break;
                case RPCOperator.Command.TaskMasterSetNewTask:
                    byte taskMasterId = reader.ReadByte();
                    int index = reader.ReadInt32();
                    int taskId = reader.ReadInt32();
                    RPCOperator.TaskMasterSetNewTask(
                        taskMasterId, index, taskId);
                    break;
                case RPCOperator.Command.JesterOutburstKill:
                    byte outburstKillerId = reader.ReadByte();
                    byte killTargetId = reader.ReadByte();
                    RPCOperator.JesterOutburstKill(
                        outburstKillerId, killTargetId);
                    break;
                case RPCOperator.Command.YandereSetOneSidedLover:
                    byte yanderePlayerId = reader.ReadByte();
                    byte loverPlayerId = reader.ReadByte();
                    RPCOperator.YandereSetOneSidedLover(
                        yanderePlayerId, loverPlayerId);
                    break;
                default:
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public class PlayerControlMurderPlayerPatch
    {
        public static bool Prefix(
            PlayerControl __instance,
            [HarmonyArgument(0)] PlayerControl target)
        {
            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

            var role = ExtremeRoleManager.GameRole[__instance.PlayerId];
            if (!role.HasOtherKillCool) { return true; }

            float killCool = role.KillCoolTime;

            GameData.PlayerInfo data = target.Data;
            if (!target.protectedByGuardian)
            {
                if (__instance.AmOwner)
                {
                    StatsManager instance = StatsManager.Instance;
                    uint num = instance.ImpostorKills;
                    instance.ImpostorKills = num + 1U;
                    if (Constants.ShouldPlaySfx())
                    {
                        SoundManager.Instance.PlaySound(
                            __instance.KillSfx, false, 0.8f);
                    }
                    __instance.SetKillTimer(killCool);
                }
                DestroyableSingleton<Telemetry>.Instance.WriteMurder();
                target.gameObject.layer = LayerMask.NameToLayer("Ghost");
                if (target.AmOwner)
                {
                    StatsManager instance2 = StatsManager.Instance;
                    uint num = instance2.TimesMurdered;
                    instance2.TimesMurdered = num + 1U;
                    if (Minigame.Instance)
                    {
                        try
                        {
                            Minigame.Instance.Close();
                            Minigame.Instance.Close();
                        }
                        catch
                        { }
                    }
                    DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(
                        __instance.Data, data);
                    DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
                    target.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
                    target.RpcSetScanner(false);
                    ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
                    importantTextTask.transform.SetParent(
                        __instance.transform, false);
                    if (!PlayerControl.GameOptions.GhostsDoTasks)
                    {
                        target.ClearTasks();
                        importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(
                            StringNames.GhostIgnoreTasks, Array.Empty<Il2CppSystem.Object>());
                    }
                    else
                    {
                        importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(
                            StringNames.GhostDoTasks, Array.Empty<Il2CppSystem.Object>());
                    }
                    target.myTasks.Insert(0, importantTextTask);
                }
                DestroyableSingleton<AchievementManager>.Instance.OnMurder(
                    __instance.AmOwner, target.AmOwner);

                var killAnimation = __instance.KillAnimations.ToList();

                var useKillAnimation = default(KillAnimation);

                if (killAnimation.Count > 0)
                {
                    useKillAnimation = killAnimation[UnityEngine.Random.Range(
                        0, killAnimation.Count)];
                }

                __instance.MyPhysics.StartCoroutine(
                    useKillAnimation.CoPerformKill(__instance, target));
                
                return false;
            }
            target.protectedByGuardianThisRound = true;
            if (__instance.AmOwner || PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel)
            {
                target.ShowFailedMurder();
                __instance.SetKillTimer(killCool / 2f);
                return false;
            }
            if (__instance.AmOwner)
            {
                __instance.SetKillTimer(killCool);
                return false;
            }
            target.RemoveProtection();
            return false;
        }

        public static void Postfix(
            PlayerControl __instance,
            [HarmonyArgument(0)] PlayerControl target)
        {

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            ExtremeRolesPlugin.GameDataStore.AddDeadInfo(
                target, DeathReason.Kill, __instance);
            
            var role = ExtremeRoleManager.GameRole[target.PlayerId];

            if (!role.HasTask || role.IsNeutral())
            {
                target.ClearTasks();
            }
        }
    }


    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    public class PlayerControlSetCoolDownPatch
    {
        public static bool Prefix(
            PlayerControl __instance, [HarmonyArgument(0)] float time)
        {
            var roles = ExtremeRoleManager.GameRole;
            if (roles.Count == 0 || !roles.ContainsKey(__instance.PlayerId)) { return true; }

            var role = roles[__instance.PlayerId];

            var killCool = PlayerControl.GameOptions.KillCooldown;
            if (killCool <= 0f) { return false; }
            float maxTime = killCool;

            if (!role.CanKill) { return false; }

            if (role.HasOtherKillCool)
            {
                maxTime = role.KillCoolTime;
            }

            __instance.killTimer = Mathf.Clamp(
                time, 0f, maxTime);
            DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(
                __instance.killTimer, maxTime);

            return false;

        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
    public class PlayerControlShapeshiftPatch
    {
        public static bool Prefix(
            PlayerControl __instance,
            [HarmonyArgument(0)] PlayerControl targetPlayer,
            [HarmonyArgument(1)] bool animate)
        {
            var roles = ExtremeRoleManager.GameRole;
            if (roles.Count == 0 || !roles.ContainsKey(__instance.PlayerId)) { return true; }

            var role = roles[__instance.PlayerId];
            if (role.IsVanillaRole()) { return true; }


            GameData.PlayerInfo targetPlayerInfo = targetPlayer.Data;
            GameData.PlayerOutfit newOutfit;
            if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
            {
                newOutfit = __instance.Data.Outfits[PlayerOutfitType.Default];
            }
            else
            {
                newOutfit = targetPlayer.Data.Outfits[PlayerOutfitType.Default];
            }
            Action changeOutfit = delegate ()
            {
                __instance.RawSetName(newOutfit.PlayerName);
                __instance.RawSetColor(newOutfit.ColorId);
                __instance.RawSetHat(newOutfit.HatId, newOutfit.ColorId);
                __instance.RawSetSkin(newOutfit.SkinId);
                __instance.RawSetVisor(newOutfit.VisorId);
                __instance.RawSetPet(newOutfit.PetId, newOutfit.ColorId);
                __instance.Visible = __instance.Visible;
                if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
                {
                    __instance.CurrentOutfitType = PlayerOutfitType.Default;
                    __instance.Data.Outfits.Remove(PlayerOutfitType.Shapeshifted);
                }
                else
                {
                    __instance.CurrentOutfitType = PlayerOutfitType.Shapeshifted;
                    __instance.Data.SetOutfit(__instance.CurrentOutfitType, newOutfit);
                }
            };
            if (animate)
            {
                __instance.shapeshifting = true;
                if (__instance.AmOwner)
                {
                    PlayerControl.HideCursorTemporarily();
                }
                RoleEffectAnimation roleEffectAnimation = UnityEngine.Object.Instantiate<RoleEffectAnimation>(
                    DestroyableSingleton<RoleManager>.Instance.shapeshiftAnim, __instance.gameObject.transform);
                roleEffectAnimation.SetMaterialColor(
                    __instance.Data.Outfits[PlayerOutfitType.Default].ColorId);
                if (__instance.MyRend.flipX)
                {
                    roleEffectAnimation.transform.position -= new Vector3(0.14f, 0f, 0f);
                }

                Action changeAction = () =>
                {
                    changeOutfit();
                    __instance.myRend.transform.localScale = __instance.defaultPlayerScale;
                    __instance.MyPhysics.Skin.gameObject.transform.localScale = __instance.defaultPlayerScale;
                };

                roleEffectAnimation.MidAnimCB = changeAction;

                __instance.StartCoroutine(__instance.ScalePlayer(0.7f, 0.25f));

                Action roleAnimation = () =>
                {
                    __instance.shapeshifting = false;
                };

                roleEffectAnimation.Play(
                    __instance, roleAnimation,
                    PlayerControl.LocalPlayer.MyRend.flipX,
                    RoleEffectAnimation.SoundType.Local, 0f);
                return false;
            }
            changeOutfit();
            return false;

        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public class PlayerControlRpcSyncSettingsPatch
    {
        public static void Postfix()
        {
            OptionHolder.ShareOptionSelections();
        }
    }
}
