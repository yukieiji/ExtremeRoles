using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.CoreScripts;

using HarmonyLib;
using Hazel;

using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch]
    public class CacheLocalPlayerPatch
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            var type = typeof(PlayerControl).GetNestedTypes(AccessTools.all).FirstOrDefault(t => t.Name.Contains("Start"));
            return AccessTools.Method(type, nameof(Il2CppSystem.Collections.IEnumerator.MoveNext));
        }

        [HarmonyPostfix]
        public static void SetLocalPlayer()
        {
            PlayerControl localPlayer = PlayerControl.LocalPlayer;
            if (!localPlayer)
            {
                CachedPlayerControl.LocalPlayer = null;
                return;
            }

            CachedPlayerControl cached = CachedPlayerControl.AllPlayerControls.FirstOrDefault(
                p => p.PlayerControl.Pointer == localPlayer.Pointer);
            if (cached != null)
            {
                CachedPlayerControl.LocalPlayer = cached;
                return;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Awake))]
    public class PlayerControlAwakePatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (__instance.notRealPlayer) { return; }

            new CachedPlayerControl(__instance);

#if DEBUG
            foreach (var cachedPlayer in CachedPlayerControl.AllPlayerControls)
            {
                if (!cachedPlayer.PlayerControl || 
                    !cachedPlayer.PlayerPhysics || 
                    !cachedPlayer.NetTransform || 
                    !cachedPlayer.transform)
                {
                    Logging.Debug($"CachedPlayer {cachedPlayer.PlayerControl.name} has null fields");
                }
            }
#endif
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Deserialize))]
    public class PlayerControlDeserializePatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            CachedPlayerControl.PlayerPtrs[__instance.Pointer].PlayerId = __instance.PlayerId;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnDestroy))]
    public class PlayerControlOnDestroyPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (__instance.notRealPlayer) { return; }
            CachedPlayerControl.Remove(__instance);
        }
    }


    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
    public class PlayerControlCoStartMeetingPatch
    {
        public static void Prefix([HarmonyArgument(0)] GameData.PlayerInfo target)
        {
            var gameData = ExtremeRolesPlugin.GameDataStore;

            if (gameData.AssassinMeetingTrigger) { return; }

            // Count meetings
            if (target == null)
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
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) { return; }
            if (!ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd) { return; }
            if (ExtremeRoleManager.GameRole.Count == 0) { return; }
            if (CachedPlayerControl.LocalPlayer.PlayerId != __instance.PlayerId) { return; }

            resetNameTagsAndColors();

            SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
            GhostRoleBase ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();

            bool blockCondition = isBlockCondition(
                CachedPlayerControl.LocalPlayer, role) || ghostRole != null;
            bool meetingInfoBlock = role.IsBlockShowMeetingRoleInfo() || ghostRole != null;
            bool playeringInfoBlock = role.IsBlockShowPlayingRoleInfo() || ghostRole != null;

            playerInfoUpdate(
                blockCondition,
                meetingInfoBlock,
                playeringInfoBlock);

            setPlayerNameColor(
                __instance,
                role, ghostRole,
                blockCondition,
                meetingInfoBlock,
                playeringInfoBlock);
            setPlayerNameTag(role);
            buttonUpdate(__instance, role, ghostRole);
            refreshRoleDescription(__instance, role, ghostRole);

            ExtremeRolesPlugin.GameDataStore.History.Enqueue(__instance);
            ExtremeRolesPlugin.GameDataStore.Union.Update();
        }

        private static void resetNameTagsAndColors()
        {

            foreach (PlayerControl player in CachedPlayerControl.AllPlayerControls)
            {
                if (player.CurrentOutfitType != PlayerOutfitType.Shapeshifted)
                {
                    player.cosmetics.SetName(player.Data.PlayerName);
                }
                if (CachedPlayerControl.LocalPlayer.Data.Role.IsImpostor && player.Data.Role.IsImpostor)
                {
                    player.cosmetics.SetNameColor(Palette.ImpostorRed);
                }
                else
                {
                    player.cosmetics.SetNameColor(Color.white);
                }
                if (MeetingHud.Instance != null)
                {
                    foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
                    {
                        if (pva.TargetPlayerId != player.PlayerId) { continue; }
                        
                        pva.NameText.text = player.Data.PlayerName;
                       
                        if (CachedPlayerControl.LocalPlayer.Data.Role.IsImpostor &&
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

            if (CachedPlayerControl.LocalPlayer.Data.Role.IsImpostor)
            {
                List<CachedPlayerControl> impostors = CachedPlayerControl.AllPlayerControls.ToArray().ToList();
                impostors.RemoveAll(x => !(x.Data.Role.IsImpostor));
                foreach (PlayerControl player in impostors)
                {
                    player.cosmetics.SetNameColor(Palette.ImpostorRed);
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
            SingleRoleBase playerRole,
            GhostRoleBase playerGhostRole,
            bool blockCondition,
            bool meetingInfoBlock,
            bool playeringInfoBlock)
        {
            var localPlayerId = player.PlayerId;

            // Modules.Helpers.DebugLog($"Player Name:{role.NameColor}");

            // まずは自分のプレイヤー名の色を変える
            Color localRoleColor = playerRole.GetNameColor(
                CachedPlayerControl.LocalPlayer.Data.IsDead);

            if (playerGhostRole != null)
            {
                Color ghostRoleColor = playerGhostRole.RoleColor;
                localRoleColor = (localRoleColor / 2.0f) + (ghostRoleColor / 2.0f);
            }
            player.cosmetics.SetNameColor(localRoleColor);
            setVoteAreaColor(localPlayerId, localRoleColor);

            GhostRoleBase targetGhostRole;

            foreach (PlayerControl targetPlayer in CachedPlayerControl.AllPlayerControls)
            {
                if (targetPlayer.PlayerId == player.PlayerId) { continue; }

                byte targetPlayerId = targetPlayer.PlayerId;
                var targetRole = ExtremeRoleManager.GameRole[targetPlayerId];

                ExtremeGhostRoleManager.GameRole.TryGetValue(targetPlayerId, out targetGhostRole);
                

                if (!OptionHolder.Client.GhostsSeeRole || 
                    !CachedPlayerControl.LocalPlayer.Data.IsDead ||
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
                    setVoteAreaColor(targetPlayerId, paintColor);
                }
                else
                {
                    Color roleColor = targetRole.GetNameColor(true);

                    if (!playeringInfoBlock)
                    {
                        targetPlayer.cosmetics.SetNameColor(roleColor);
                    }
                    setGhostVoteAreaColor(
                        targetPlayerId,
                        roleColor,
                        meetingInfoBlock,
                        targetRole.Team == playerRole.Team);
                }
            }

        }

        private static void setPlayerNameTag(
            SingleRoleBase playerRole)
        {

            foreach (PlayerControl targetPlayer in CachedPlayerControl.AllPlayerControls)
            {
                byte playerId = targetPlayer.PlayerId;
                string tag = playerRole.GetRolePlayerNameTag(
                    ExtremeRoleManager.GameRole[playerId], playerId);
                if (tag == string.Empty) { continue; }

                targetPlayer.cosmetics.nameText.text += tag;

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
            bool blockCondition,
            bool meetingInfoBlock,
            bool playeringInfoBlock)
        {

            bool commsActive = false;
            foreach (PlayerTask t in 
                CachedPlayerControl.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
            {
                if (t.TaskType == TaskTypes.FixComms)
                {
                    commsActive = true;
                    break;
                }
            }

            foreach (PlayerControl player in CachedPlayerControl.AllPlayerControls)
            {

                if (player.PlayerId != CachedPlayerControl.LocalPlayer.PlayerId && 
                    !CachedPlayerControl.LocalPlayer.Data.IsDead)
                {
                    continue;
                }

                Transform playerInfoTransform = player.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;

                if (playerInfo == null)
                {
                    playerInfo = UnityEngine.Object.Instantiate(
                        player.cosmetics.nameText,
                        player.cosmetics.nameText.transform.parent);
                    playerInfo.fontSize *= 0.75f;
                    playerInfo.gameObject.name = "Info";
                }

                // Set the position every time bc it sometimes ends up in the wrong place due to camoflauge
                playerInfo.transform.localPosition = player.cosmetics.nameText.transform.localPosition + Vector3.up * 0.5f;

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

                var (playerInfoText, meetingInfoText) = getRoleAndMeetingInfo(player, commsActive);
                playerInfo.text = playerInfoText;
                
                if (player.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
                {
                    playerInfo.gameObject.SetActive(player.Visible);
                    setMeetingInfo(meetingInfo, meetingInfoText, true);
                }
                else if (blockCondition)
                {
                    playerInfo.gameObject.SetActive(false);
                    setMeetingInfo(meetingInfo, "", false);
                }
                else
                {
                    playerInfo.gameObject.SetActive((player.Visible && !playeringInfoBlock));
                    setMeetingInfo(meetingInfo, meetingInfoText, !meetingInfoBlock);
                }
            }
        }
        private static void setMeetingInfo(
            TMPro.TextMeshPro meetingInfo,
            string text, bool active)
        {
            if (meetingInfo != null)
            {
                meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : text;
                meetingInfo.gameObject.SetActive(active);
            }
        }

        private static bool isBlockCondition(
            CachedPlayerControl localPlayer, SingleRoleBase role)
        {
            if (localPlayer.Data.Role.Role == RoleTypes.GuardianAngel)
            {
                return true;
            }
            else if (role.IsImpostor())
            {
                return ExtremeRolesPlugin.GameDataStore.IsAssassinAssign;
            }

            return false;

        }

        private static Tuple<string, string> getRoleAndMeetingInfo(
            PlayerControl targetPlayer, bool commonActive)
        {

            var (tasksCompleted, tasksTotal) = GameSystem.GetTaskInfo(targetPlayer.Data);
            byte targetPlayerId = targetPlayer.PlayerId;
            string roleNames = ExtremeRoleManager.GameRole[targetPlayerId].GetColoredRoleName(
                CachedPlayerControl.LocalPlayer.Data.IsDead);

            if (ExtremeGhostRoleManager.GameRole.ContainsKey(targetPlayerId))
            {
                string ghostRoleName = ExtremeGhostRoleManager.GameRole[targetPlayerId].GetColoredRoleName();
                roleNames = $"{ghostRoleName}({roleNames})";
            }

            var completedStr = commonActive ? "?" : tasksCompleted.ToString();
            string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({completedStr}/{tasksTotal})</color>" : "";

            string playerInfoText = "";
            string meetingInfoText = "";

            if (targetPlayer.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
            {
                playerInfoText = $"{roleNames}";
                if (DestroyableSingleton<TaskPanelBehaviour>.InstanceExists)
                {
                    TMPro.TextMeshPro tabText = FastDestroyableSingleton<TaskPanelBehaviour>.Instance.tab.transform.FindChild(
                        "TabText_TMP").GetComponent<TMPro.TextMeshPro>();
                    tabText.SetText(
                        $"{FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Tasks)} {taskInfo}");
                }
                meetingInfoText = $"{roleNames} {taskInfo}".Trim();
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
            SingleRoleBase playerRole,
            GhostRoleBase playerGhostRole)
        {

            var removedTask = new List<PlayerTask>();
            foreach (PlayerTask task in player.myTasks.GetFastEnumerator())
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

            string addText = playerRole.GetImportantText();
            if (playerGhostRole != null)
            {
                addText =$"{addText}\n{playerGhostRole.GetImportantText()}";
            }
            importantTextTask.Text = addText;
            player.myTasks.Insert(0, importantTextTask);

        }
        private static void buttonUpdate(
            PlayerControl player,
            SingleRoleBase playerRole,
            GhostRoleBase playerGhostRole)
        {
            if (!player.AmOwner) { return; }

            bool enable = Player.ShowButtons && !CachedPlayerControl.LocalPlayer.Data.IsDead;

            killButtonUpdate(player, playerRole, enable);
            ventButtonUpdate(playerRole, enable);

            sabotageButtonUpdate(playerRole);
            roleAbilityButtonUpdate(playerRole);

            ghostRoleButtonUpdate(playerGhostRole);
        }

        private static void killButtonUpdate(
            PlayerControl player,
            SingleRoleBase role, bool enable)
        {

            bool isImposter = role.IsImpostor();

            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;

            if (role.CanKill)
            {
                if (enable)
                {

                    if (!isImposter && player.CanMove)
                    {
                        player.SetKillTimer(player.killTimer - Time.fixedDeltaTime);
                    }

                    PlayerControl target = Player.GetClosestKillRangePlayer();

                    // Logging.Debug($"TargetAlive?:{target}");

                    hudManager.KillButton.SetTarget(target);
                    Player.SetPlayerOutLine(target, role.GetNameColor());
                    hudManager.KillButton.Show();
                    hudManager.KillButton.gameObject.SetActive(true);
                }
                else
                {
                    hudManager.KillButton.SetDisabled();
                }
            }
            else if (isImposter)
            {
                hudManager.KillButton.SetDisabled();
            }
        }

        private static void roleAbilityButtonUpdate(
            SingleRoleBase role)
        {
            void buttonUpdate(SingleRoleBase role)
            {
                var abilityRole = role as IRoleAbility;

                if (abilityRole != null && abilityRole.Button != null)
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

            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;

            if (role.UseSabotage)
            {
                // インポスターとヴィジランテは死んでもサボタージ使える
                if (enable && (role.IsImpostor() || role.Id == ExtremeRoleId.Vigilante))
                {
                    hudManager.SabotageButton.Show();
                    hudManager.SabotageButton.gameObject.SetActive(true);
                }
                // それ以外は死んでないときだけサボタージ使える
                else if(enable && !CachedPlayerControl.LocalPlayer.Data.IsDead)
                {
                    hudManager.SabotageButton.Show();
                    hudManager.SabotageButton.gameObject.SetActive(true);
                }
                else
                {
                    hudManager.SabotageButton.SetDisabled();
                }
            }
            else
            {
                hudManager.SabotageButton.SetDisabled();
            }
        }

        private static void ventButtonUpdate(
            SingleRoleBase role, bool enable)
        {

            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;

            if (role.UseVent)
            {
                if (!role.IsVanillaRole())
                {
                    if (enable) { hudManager.ImpostorVentButton.Show(); }
                    else { hudManager.ImpostorVentButton.SetDisabled(); }
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
                                hudManager.AbilityButton.Show();
                            }
                            else
                            {
                                hudManager.ImpostorVentButton.Show();
                                hudManager.AbilityButton.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            hudManager.ImpostorVentButton.SetDisabled();
                            hudManager.AbilityButton.SetDisabled(); 
                        }
                    }
                }
            }
            else
            {
                hudManager.ImpostorVentButton.SetDisabled();
            }
        }

        private static void ghostRoleButtonUpdate(GhostRoleBase playerGhostRole)
        {
            if (playerGhostRole != null && playerGhostRole.Button != null)
            {
                switch(CachedPlayerControl.LocalPlayer.Data.Role.Role)
                {
                    case RoleTypes.Engineer:
                    case RoleTypes.Scientist:
                    case RoleTypes.Shapeshifter:
                        FastDestroyableSingleton<HudManager>.Instance.AbilityButton.Hide();
                        break;
                    default:
                        break;
                }
                playerGhostRole.Button.Update();
            }
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
                case RPCOperator.Command.Initialize:
                    RPCOperator.Initialize();
                    break;
                case RPCOperator.Command.ForceEnd:
                    RPCOperator.ForceEnd();
                    break;
                case RPCOperator.Command.SetRoleToAllPlayer:
                    List<Module.IAssignedPlayer> assignData = new List<Module.IAssignedPlayer>();
                    int assignDataNum = reader.ReadPackedInt32();
                    for (int i = 0; i < assignDataNum; ++i)
                    {
                        byte assignedPlayerId = reader.ReadByte();
                        byte assignRoleType = reader.ReadByte();
                        int exRoleId = reader.ReadPackedInt32();
                        switch (assignRoleType)
                        {
                            case (byte)Module.IAssignedPlayer.ExRoleType.Single:
                                assignData.Add(new
                                    Module.AssignedPlayerToSingleRoleData(
                                        assignedPlayerId, exRoleId));
                                break;
                            case (byte)Module.IAssignedPlayer.ExRoleType.Comb:
                                byte assignCombType = reader.ReadByte(); // combTypeId
                                byte bytedGameContId = reader.ReadByte(); // byted GameContId
                                byte bytedAmongUsVanillaRoleId = reader.ReadByte(); // byted AmongUsVanillaRoleId
                                assignData.Add(new
                                    Module.AssignedPlayerToCombRoleData(
                                        assignedPlayerId, exRoleId, assignCombType,
                                        bytedGameContId, bytedAmongUsVanillaRoleId));
                                break;
                        }
                    }
                    RPCOperator.SetRoleToAllPlayer(assignData);
                    ExtremeRolesPlugin.GameDataStore.RoleSetUpEnd();
                    break;
                case RPCOperator.Command.FixLightOff:
                    RPCOperator.FixLightOff();
                    break;
                case RPCOperator.Command.ShareOption:
                    int numOptions = (int)reader.ReadByte();
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
                case RPCOperator.Command.PlaySound:
                    byte soundType = reader.ReadByte();
                    RPCOperator.PlaySound(soundType);
                    break;
                case RPCOperator.Command.IntegrateModCall:
                    RPCOperator.IntegrateModCall(ref reader);
                    break;
                case RPCOperator.Command.HeroHeroAcademia:
                    RPCOperator.HeroHeroAcademiaCommand(ref reader);
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
                case RPCOperator.Command.CarpenterUseAbility:
                    RPCOperator.CarpenterUseAbility(ref reader);
                    break;
                case RPCOperator.Command.SurvivorDeadWin:
                    byte survivorPlayerId = reader.ReadByte();
                    RPCOperator.SurvivorDeadWin(survivorPlayerId);
                    break;
                case RPCOperator.Command.AssasinVoteFor:
                    byte voteTargetId = reader.ReadByte();
                    RPCOperator.AssasinVoteFor(voteTargetId);
                    break;
                case RPCOperator.Command.CarrierAbility:
                    byte carrierCarryOpCallPlayerId = reader.ReadByte();
                    float carrierPlayerPosX = reader.ReadSingle();
                    float carrierPlayerPosY = reader.ReadSingle();
                    byte carryDeadBodyPlayerId = reader.ReadByte();
                    bool isCarryDeadBody = reader.ReadBoolean();
                    RPCOperator.CarrierAbility(
                        carrierCarryOpCallPlayerId,
                        carrierPlayerPosX,
                        carrierPlayerPosY,
                        carryDeadBodyPlayerId,
                        isCarryDeadBody);
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
                case RPCOperator.Command.MerySetCamp:
                    byte maryPlayerId = reader.ReadByte();
                    RPCOperator.MarySetCamp(maryPlayerId);
                    break;
                case RPCOperator.Command.MeryAcivateVent:
                    int maryCampIndex = reader.ReadInt32();
                    RPCOperator.MaryActiveVent(maryCampIndex);
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
                case RPCOperator.Command.SetGhostRole:
                    RPCOperator.SetGhostRole(
                        ref reader);
                    break;
                case RPCOperator.Command.UseGhostRoleAbility:
                    byte useGhostRoleType = reader.ReadByte();
                    bool isReport = reader.ReadBoolean();
                    RPCOperator.UseGhostRoleAbility(
                        useGhostRoleType, isReport, ref reader);
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
            if (role.Id == ExtremeRoleId.Villain)
            {
                guardBreakKill(__instance, target, role.KillCoolTime);
                return false; 
            }
            if (!role.HasOtherKillCool) { return true; }

            float killCool = role.KillCoolTime;

            GameData.PlayerInfo data = target.Data;
            if (target.protectedByGuardian)
            {

                target.protectedByGuardianThisRound = true;
                bool flag = CachedPlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel;
                if (__instance.AmOwner || flag)
                {
                    target.ShowFailedMurder();
                    __instance.SetKillTimer(killCool / 2f);
                }
                else
                {
                    target.RemoveProtection();
                }
                if (flag)
                {
                    StatsManager.Instance.IncrementStat(
                        StringNames.StatsGuardianAngelCrewmatesProtected);
                    return false;
                }
            }
            else
            {
                if (__instance.AmOwner)
                {
                    StatsManager.Instance.IncrementStat(StringNames.StatsImpostorKills);
                    if (__instance.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
                    {
                        StatsManager.Instance.IncrementStat(StringNames.StatsShapeshifterShiftedKills);
                    }
                    if (Constants.ShouldPlaySfx())
                    {
                        SoundManager.Instance.PlaySound(
                            __instance.KillSfx, false, 0.8f);
                    }
                    __instance.SetKillTimer(killCool);
                }
                FastDestroyableSingleton<Telemetry>.Instance.WriteMurder();
	            target.gameObject.layer = LayerMask.NameToLayer("Ghost");
	            if (target.AmOwner)
	            {
		            StatsManager.Instance.IncrementStat(StringNames.StatsTimesMurdered);
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
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(
                        __instance.Data, data);
                    FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
		            target.cosmetics.SetNameMask(false);
		            target.RpcSetScanner(false);
		            ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
		            importantTextTask.transform.SetParent(
                        __instance.transform, false);
		            if (!PlayerControl.GameOptions.GhostsDoTasks)
		            {
			            target.ClearTasks();
			            importantTextTask.Text = FastDestroyableSingleton<TranslationController>.Instance.GetString(
                            StringNames.GhostIgnoreTasks, Array.Empty<Il2CppSystem.Object>());
		            }
		            else
		            {
			            importantTextTask.Text = FastDestroyableSingleton<TranslationController>.Instance.GetString(
                            StringNames.GhostDoTasks, Array.Empty<Il2CppSystem.Object>());
		            }
		            target.myTasks.Insert(0, importantTextTask);
	            }
                FastDestroyableSingleton<AchievementManager>.Instance.OnMurder(
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
            }
           
            return false;
        }

        public static void Postfix(
            PlayerControl __instance,
            [HarmonyArgument(0)] PlayerControl target)
        {

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            if (!target.Data.IsDead) { return; }

            ExtremeRolesPlugin.GameDataStore.AddDeadInfo(
                target, DeathReason.Kill, __instance);

            byte targetPlayerId = target.PlayerId;

            var role = ExtremeRoleManager.GameRole[targetPlayerId];

            if (!role.HasTask || role.IsNeutral())
            {
                target.ClearTasks();
            }

            if (ExtremeRoleManager.IsDisableWinCheckRole(role))
            {
                ExtremeRolesPlugin.GameDataStore.WinCheckDisable = true;
            }

            var multiAssignRole = role as MultiAssignRoleBase;

            role.RolePlayerKilledAction(
                target, __instance);
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    multiAssignRole.AnotherRole.RolePlayerKilledAction(
                        target, __instance);
                }
            }

            ExtremeRolesPlugin.GameDataStore.WinCheckDisable = false;

            var player = CachedPlayerControl.LocalPlayer;

            if (player.PlayerId != targetPlayerId)
            {
                var hockRole = ExtremeRoleManager.GameRole[
                    player.PlayerId] as IRoleMurderPlayerHock;
                multiAssignRole = ExtremeRoleManager.GameRole[
                    player.PlayerId] as MultiAssignRoleBase;

                if (hockRole != null)
                {
                    hockRole.HockMuderPlayer(
                        __instance, target);
                }
                if (multiAssignRole != null)
                {
                    hockRole = multiAssignRole.AnotherRole as IRoleMurderPlayerHock;
                    if (hockRole != null)
                    {
                        hockRole.HockMuderPlayer(
                            __instance, target);
                    }
                }
            }
        }
        private static void guardBreakKill(
            PlayerControl instance,
            PlayerControl target,
            float killCool)
        {
            if (target.protectedByGuardian)
            {
                target.RemoveProtection();
            }

            if (instance.AmOwner)
            {
                StatsManager.Instance.IncrementStat(StringNames.StatsImpostorKills);
                if (instance.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
                {
                    StatsManager.Instance.IncrementStat(StringNames.StatsShapeshifterShiftedKills);
                }
                if (Constants.ShouldPlaySfx())
                {
                    SoundManager.Instance.PlaySound(
                        instance.KillSfx, false, 0.8f);
                }
                instance.SetKillTimer(killCool);
            }
            FastDestroyableSingleton<Telemetry>.Instance.WriteMurder();
            target.gameObject.layer = LayerMask.NameToLayer("Ghost");
            if (target.AmOwner)
            {
                StatsManager.Instance.IncrementStat(StringNames.StatsTimesMurdered);
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
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(
                    instance.Data, target.Data);
                FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
                target.cosmetics.SetNameMask(false);
                target.RpcSetScanner(false);
                ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
                importantTextTask.transform.SetParent(
                    instance.transform, false);
                if (!PlayerControl.GameOptions.GhostsDoTasks)
                {
                    target.ClearTasks();
                    importantTextTask.Text = FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.GhostIgnoreTasks, Array.Empty<Il2CppSystem.Object>());
                }
                else
                {
                    importantTextTask.Text = FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.GhostDoTasks, Array.Empty<Il2CppSystem.Object>());
                }
                target.myTasks.Insert(0, importantTextTask);
            }
            FastDestroyableSingleton<AchievementManager>.Instance.OnMurder(
                instance.AmOwner, target.AmOwner);

            var killAnimation = instance.KillAnimations.ToList();

            var useKillAnimation = default(KillAnimation);

            if (killAnimation.Count > 0)
            {
                useKillAnimation = killAnimation[UnityEngine.Random.Range(
                    0, killAnimation.Count)];
            }

            instance.MyPhysics.StartCoroutine(
                useKillAnimation.CoPerformKill(instance, target));
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
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(
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
                __instance.RawSetSkin(newOutfit.SkinId, newOutfit.ColorId);
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
                    FastDestroyableSingleton<RoleManager>.Instance.shapeshiftAnim, __instance.gameObject.transform);
                roleEffectAnimation.SetMaterialColor(
                    __instance.Data.Outfits[PlayerOutfitType.Default].ColorId);
                if (__instance.cosmetics.FlipX)
                {
                    roleEffectAnimation.transform.position -= new Vector3(0.14f, 0f, 0f);
                }

                Action changeAction = () =>
                {
                    changeOutfit();
                    __instance.cosmetics.SetScale(__instance.defaultPlayerScale);
                };

                roleEffectAnimation.MidAnimCB = changeAction;
                
                if (Constants.ShouldHorseAround())
                {
                    __instance.StartCoroutine(__instance.ScalePlayer(0.3f, 0.25f));
                }
                else
                {
                    __instance.StartCoroutine(__instance.ScalePlayer(0.7f, 0.25f));
                }

                Action roleAnimation = () =>
                {
                    __instance.shapeshifting = false;
                };

                roleEffectAnimation.Play(
                    __instance, roleAnimation,
                    CachedPlayerControl.LocalPlayer.PlayerControl.cosmetics.FlipX,
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
