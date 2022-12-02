using System;

using UnityEngine;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;

using UnhollowerBaseLib.Attributes;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public class VoteAreaInfo : MonoBehaviour
    {
        protected CachedPlayerControl LocalPlayer;
        protected TextMeshPro NameText;
        protected TextMeshPro MeetingInfo;

        protected bool CommActive;

        public VoteAreaInfo(IntPtr ptr) : base(ptr) { }

        public virtual void Init(PlayerVoteArea pva, bool commActive)
        {
            this.LocalPlayer = CachedPlayerControl.LocalPlayer;
            this.NameText = pva.NameText;

            this.MeetingInfo = Instantiate(
                this.NameText, this.NameText.transform);
            this.MeetingInfo.transform.localPosition += Vector3.down * 0.20f + Vector3.left * 0.30f;
            this.MeetingInfo.fontSize *= 0.63f;
            this.MeetingInfo.autoSizeTextContainer = false;
            this.MeetingInfo.gameObject.name = "VoteAreaInfo";
            this.CommActive = commActive;

            base.gameObject.SetActive(true);
        }
    }



    [Il2CppRegister]
    public sealed class LocalPlayerVoteAreaInfo : VoteAreaInfo
    {
        public LocalPlayerVoteAreaInfo(IntPtr ptr) : base(ptr) { }

        public void FixedUpdate()
        {
            SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
            GhostRoleBase ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();

            resetInfo();
            setTag(role);
            setColor(role, ghostRole);
            setInfo(role, ghostRole);
        }

        private void resetInfo()
        {
            this.NameText.text = this.LocalPlayer.Data.PlayerName;

            if (this.LocalPlayer.Data.Role.IsImpostor)
            {
                this.NameText.color = Palette.ImpostorRed;
            }
            else
            {
                this.NameText.color = Palette.White;
            }
        }

        [HideFromIl2Cpp]
        private void setColor(
            SingleRoleBase role,
            GhostRoleBase ghostRole)
        {
            Color paintColor = role.GetNameColor(
                this.LocalPlayer.Data.IsDead);
            if (ghostRole != null)
            {
                Color ghostRoleColor = ghostRole.RoleColor;
                paintColor = (paintColor / 2.0f) + (ghostRoleColor / 2.0f);
            }
            if (paintColor == Palette.ClearWhite) { return; }

            this.NameText.color = paintColor;
        }

        [HideFromIl2Cpp]
        private void setInfo(
            SingleRoleBase role,
            GhostRoleBase ghostRole)
        {
            this.MeetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ?
                "" : getMeetingInfo(role, ghostRole);
            this.MeetingInfo.gameObject.SetActive(true);
        }

        [HideFromIl2Cpp]
        private string getMeetingInfo(
            SingleRoleBase role, GhostRoleBase ghostRole)
        {
            var (tasksCompleted, tasksTotal) = GameSystem.GetTaskInfo(
                this.LocalPlayer.Data);
            string roleNames = role.GetColoredRoleName(this.LocalPlayer.Data.IsDead);

            if (ghostRole != null)
            {
                string ghostRoleName = ghostRole.GetColoredRoleName();
                roleNames = $"{ghostRoleName}({roleNames})";
            }

            var completedStr = this.CommActive ? "?" : tasksCompleted.ToString();
            string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({completedStr}/{tasksTotal})</color>" : "";

            return $"{roleNames} {taskInfo}".Trim(); ;
        }

        [HideFromIl2Cpp]
        private void setTag(SingleRoleBase role)
        {
            string tag = role.GetRolePlayerNameTag(
                role, this.LocalPlayer.PlayerId);
            if (tag == string.Empty) { return; }
            this.NameText.text += tag;
        }
    }

    [Il2CppRegister]
    public sealed class OtherPlayerVoteAreaInfo : VoteAreaInfo
    {
        private GameData.PlayerInfo votePlayerInfo;

        public OtherPlayerVoteAreaInfo(IntPtr ptr) : base(ptr) { }

        public override void Init(PlayerVoteArea pva, bool commActive)
        {
            base.Init(pva, commActive);
            this.votePlayerInfo = GameData.Instance.GetPlayerById(pva.TargetPlayerId);
        }

        public void FixedUpdate()
        {
            resetInfo();

            byte playerId = this.votePlayerInfo.PlayerId;

            SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
            SingleRoleBase targetRole = ExtremeRoleManager.GameRole[playerId];

            GhostRoleBase ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
            ExtremeGhostRoleManager.GameRole.TryGetValue(
                playerId, out GhostRoleBase targetGhostRole);
            bool isLocalPlayerGhostRole = ghostRole != null;

            bool blockCondition = isBlockCondition(role) || isLocalPlayerGhostRole;
            bool meetingInfoBlock = role.IsBlockShowMeetingRoleInfo() || isLocalPlayerGhostRole;

            if (role is MultiAssignRoleBase multiRole &&
                multiRole.AnotherRole != null)
            {
                blockCondition = blockCondition || isBlockCondition(multiRole.AnotherRole);
                meetingInfoBlock = 
                    meetingInfoBlock || multiRole.AnotherRole.IsBlockShowMeetingRoleInfo();
            }

            setPlayerNameTag(role, targetRole);

            setMeetingInfo(
                targetRole,
                targetGhostRole,
                meetingInfoBlock,
                blockCondition);

            setNameColor(
                role,
                targetRole,
                ghostRole,
                targetGhostRole,
                meetingInfoBlock,
                blockCondition);

        }

        private void resetInfo()
        {
            this.NameText.text = this.votePlayerInfo.PlayerName;
            
            if (this.LocalPlayer.Data.Role.IsImpostor &&
                this.votePlayerInfo.Role.IsImpostor)
            {
                this.NameText.color = Palette.ImpostorRed;
            }
            else
            {
                this.NameText.color = Palette.White;
            }
        }

        [HideFromIl2Cpp]
        private void setNameColor(
            SingleRoleBase localRole,
            SingleRoleBase targetRole,
            GhostRoleBase localGhostRole,
            GhostRoleBase targetGhostRole,
            bool isMeetingInfoBlock,
            bool blockCondition)
        {

            byte targetPlayerId = this.votePlayerInfo.PlayerId;

            if (!OptionHolder.Client.GhostsSeeRole ||
                !this.LocalPlayer.Data.IsDead ||
                blockCondition)
            {
                Color paintColor = localRole.GetTargetRoleSeeColor(
                    targetRole, targetPlayerId);
                if (localGhostRole != null)
                {
                    Color paintGhostColor = localGhostRole.GetTargetRoleSeeColor(
                        targetPlayerId, targetRole, targetGhostRole);

                    if (paintGhostColor != Color.clear)
                    {
                        paintColor = (paintGhostColor / 2.0f) + (paintColor / 2.0f);
                    }
                }

                if (paintColor == Palette.ClearWhite) { return; }

                this.NameText.color = paintColor;

            }
            else
            {
                Color roleColor = targetRole.GetNameColor(true);

                // インポスター同士は見える
                if (!isMeetingInfoBlock || 
                    (localRole.IsImpostor() && targetRole.IsImpostor()))
                {
                    this.NameText.color = roleColor;
                }
            }
        }

        [HideFromIl2Cpp]
        private void setMeetingInfo(
            SingleRoleBase targetRole,
            GhostRoleBase targetGhostRole,
            bool isMeetingInfoBlock,
            bool blockCondition)
        {
            if (!this.LocalPlayer.Data.IsDead || blockCondition)
            {
                this.MeetingInfo.gameObject.SetActive(false);
            }
            else
            {
                this.MeetingInfo.text = 
                    MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? 
                    "" : getMeetingInfo(targetRole, targetGhostRole);
                this.MeetingInfo.gameObject.SetActive(!isMeetingInfoBlock);
            }
        }

        [HideFromIl2Cpp]
        private string getMeetingInfo(
            SingleRoleBase targetRole,
            GhostRoleBase targetGhostRole)
        {
            var (tasksCompleted, tasksTotal) = GameSystem.GetTaskInfo(this.votePlayerInfo);
            string roleNames = targetRole.GetColoredRoleName(this.LocalPlayer.Data.IsDead);

            if (targetGhostRole != null)
            {
                string ghostRoleName = targetGhostRole.GetColoredRoleName();
                roleNames = $"{ghostRoleName}({roleNames})";
            }

            var completedStr = this.CommActive ? "?" : tasksCompleted.ToString();
            string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({completedStr}/{tasksTotal})</color>" : "";

            string meetingInfoText = "";

            if (OptionHolder.Client.GhostsSeeRole && OptionHolder.Client.GhostsSeeTask)
            {
                meetingInfoText = $"{roleNames} {taskInfo}".Trim();
            }
            else if (OptionHolder.Client.GhostsSeeTask)
            {
                meetingInfoText = $"{taskInfo}".Trim();
            }
            else if (OptionHolder.Client.GhostsSeeRole)
            {
                meetingInfoText = $"{roleNames}";
            }
            return meetingInfoText;
        }

        [HideFromIl2Cpp]
        private void setPlayerNameTag(
            SingleRoleBase localRole,
            SingleRoleBase targetRole)
        {
            string tag = localRole.GetRolePlayerNameTag(
                targetRole, this.votePlayerInfo.PlayerId);
            if (tag == string.Empty) { return; }

            this.NameText.text += tag;
        }

        [HideFromIl2Cpp]
        private bool isBlockCondition(SingleRoleBase role)
        {
            if (this.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel)
            {
                return true;
            }
            else if (role.IsImpostor())
            {
                return ExtremeRolesPlugin.ShipState.IsAssassinAssign;
            }
            return false;
        }

    }
}
