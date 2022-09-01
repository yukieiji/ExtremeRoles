using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

using UnityEngine;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;


namespace ExtremeRoles.Patches.Manager
{

    // コレを消すと動かなくなる
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class HudManagerStartPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
        public static void PostFix(HudManager __instance)
        {
            if (GameSystem.IsFreePlay) { return; }

            if (Module.InfoOverlay.Button.Body == null)
            {
                Module.InfoOverlay.Button.CreateInfoButton();
            }
            else
            {
                Module.InfoOverlay.Button.SetInfoButtonToGameStartShipPositon();
            }
        }
    }


    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdatePatch
    {
        private static bool buttonCreated = false;

        private static Dictionary<byte, TextMeshPro> allPlayerInfo = new Dictionary<byte, TextMeshPro>();
        private static TextMeshPro tabText;

        public static void Reset()
        {
            allPlayerInfo.Clear();
            tabText = null;
        }

        public static void Prefix(HudManager __instance)
        {
            if (__instance.GameSettings != null)
            {
                __instance.GameSettings.fontSize = 1.2f;
            }
            if (ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger)
            {

                __instance.UseButton.ToggleVisible(false);
                __instance.AbilityButton.ToggleVisible(false);
                __instance.ReportButton.ToggleVisible(false);
                __instance.KillButton.ToggleVisible(false);
                __instance.SabotageButton.ToggleVisible(false);
                __instance.ImpostorVentButton.ToggleVisible(false);
                __instance.TaskText.transform.parent.gameObject.SetActive(false);
                __instance.roomTracker.gameObject.SetActive(false);
                
                IVirtualJoystick virtualJoystick = __instance.joystick;

                if (virtualJoystick != null)
                {
                    virtualJoystick.ToggleVisuals(false);
                }
            }

        }
        public static void Postfix()
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) { return; }
            if (!ExtremeRolesPlugin.ShipState.IsRoleSetUpEnd) { return; }
            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
            GhostRoleBase ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
            CachedPlayerControl player = CachedPlayerControl.LocalPlayer;

            resetNameTagsAndColors(player);

            bool blockCondition = isBlockCondition(player, role) || ghostRole != null;
            bool playeringInfoBlock = role.IsBlockShowPlayingRoleInfo() || ghostRole != null;

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

        private static void resetNameTagsAndColors(CachedPlayerControl localPlayer)
        {

            foreach (PlayerControl player in CachedPlayerControl.AllPlayerControls)
            {
                if (player.CurrentOutfitType != PlayerOutfitType.Shapeshifted)
                {
                    player.cosmetics.SetName(player.Data.PlayerName);
                }
                if (localPlayer.Data.Role.IsImpostor && player.Data.Role.IsImpostor)
                {
                    player.cosmetics.SetNameColor(Palette.ImpostorRed);
                }
                else
                {
                    player.cosmetics.SetNameColor(Color.white);
                }
            }

            if (localPlayer.Data.Role.IsImpostor)
            {
                List<CachedPlayerControl> impostors = CachedPlayerControl.AllPlayerControls.ToArray().ToList();
                impostors.RemoveAll(x => !(x.Data.Role.IsImpostor));
                foreach (PlayerControl player in impostors)
                {
                    player.cosmetics.SetNameColor(Palette.ImpostorRed);
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
            var localPlayerId = localPlayer.PlayerId;

            // Modules.Helpers.DebugLog($"Player Name:{role.NameColor}");

            // まずは自分のプレイヤー名の色を変える
            Color localRoleColor = playerRole.GetNameColor(localPlayer.Data.IsDead);

            if (playerGhostRole != null)
            {
                Color ghostRoleColor = playerGhostRole.RoleColor;
                localRoleColor = (localRoleColor / 2.0f) + (ghostRoleColor / 2.0f);
            }
            localPlayer.cosmetics.SetNameColor(localRoleColor);

            GhostRoleBase targetGhostRole;

            foreach (PlayerControl targetPlayer in CachedPlayerControl.AllPlayerControls)
            {
                if (targetPlayer.PlayerId == localPlayerId) { continue; }

                byte targetPlayerId = targetPlayer.PlayerId;
                var targetRole = ExtremeRoleManager.GameRole[targetPlayerId];

                ExtremeGhostRoleManager.GameRole.TryGetValue(targetPlayerId, out targetGhostRole);


                if (!OptionHolder.Client.GhostsSeeRole ||
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

            foreach (PlayerControl targetPlayer in CachedPlayerControl.AllPlayerControls)
            {
                byte playerId = targetPlayer.PlayerId;
                string tag = playerRole.GetRolePlayerNameTag(
                    ExtremeRoleManager.GameRole[playerId], playerId);
                if (tag == string.Empty) { continue; }

                targetPlayer.cosmetics.nameText.text += tag;
            }
        }

        private static void playerInfoUpdate(
            CachedPlayerControl localPlayer,
            bool blockCondition,
            bool playeringInfoBlock)
        {

            bool commsActive = false;
            foreach (PlayerTask t in localPlayer.PlayerControl.myTasks.GetFastEnumerator())
            {
                if (t.TaskType == TaskTypes.FixComms)
                {
                    commsActive = true;
                    break;
                }
            }

            foreach (PlayerControl player in CachedPlayerControl.AllPlayerControls)
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
                    playerInfo.gameObject.name = "Info";
                    allPlayerInfo[player.PlayerId] = playerInfo;
                }

                playerInfo.transform.localPosition = 
                    player.cosmetics.nameText.transform.localPosition + Vector3.up * 0.5f;
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
            CachedPlayerControl localPlayer, SingleRoleBase role)
        {
            if (localPlayer.Data.Role.Role == RoleTypes.GuardianAngel)
            {
                return true;
            }
            else if (role.IsImpostor())
            {
                return ExtremeRolesPlugin.ShipState.IsAssassinAssign;
            }

            return false;

        }

        private static string getRoleInfo(
            CachedPlayerControl localPlayer,
            PlayerControl targetPlayer,
            bool commonActive)
        {

            var (tasksCompleted, tasksTotal) = GameSystem.GetTaskInfo(targetPlayer.Data);
            byte targetPlayerId = targetPlayer.PlayerId;
            string roleNames = ExtremeRoleManager.GameRole[targetPlayerId].GetColoredRoleName(
                localPlayer.Data.IsDead);

            if (ExtremeGhostRoleManager.GameRole.ContainsKey(targetPlayerId))
            {
                string ghostRoleName = ExtremeGhostRoleManager.GameRole[targetPlayerId].GetColoredRoleName();
                roleNames = $"{ghostRoleName}({roleNames})";
            }

            var completedStr = commonActive ? "?" : tasksCompleted.ToString();
            string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({completedStr}/{tasksTotal})</color>" : "";

            string playerInfoText = "";

            if (targetPlayer.PlayerId == localPlayer.PlayerId)
            {
                playerInfoText = $"{roleNames}";
                if (DestroyableSingleton<TaskPanelBehaviour>.InstanceExists)
                {
                    if (tabText == null)
                    {
                        tabText = FastDestroyableSingleton<TaskPanelBehaviour>.Instance.tab.transform.FindChild(
                            "TabText_TMP").GetComponent<TextMeshPro>();
                    }
                    tabText.SetText(
                        $"{FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Tasks)} {taskInfo}");
                }
            }
            else if (OptionHolder.Client.GhostsSeeRole && OptionHolder.Client.GhostsSeeTask)
            {
                playerInfoText = $"{roleNames} {taskInfo}".Trim();
            }
            else if (OptionHolder.Client.GhostsSeeTask)
            {
                playerInfoText = $"{taskInfo}".Trim();
            }
            else if (OptionHolder.Client.GhostsSeeRole)
            {
                playerInfoText = $"{roleNames}";
            }
            return playerInfoText;
        }


        private static void roleUpdate(
            CachedPlayerControl player, SingleRoleBase checkRole)
        {
            var updatableRole = checkRole as IRoleUpdate;
            if (updatableRole != null)
            {
                updatableRole.Update(player);
            }
        }

    }
}
