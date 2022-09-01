using UnityEngine;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Interface;

using UnhollowerBaseLib.Attributes;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    // 同じ処理だけど継承するとどうもおかしくなるのでしない

    [Il2CppRegister]
    public sealed class LocalPlayerVoteAreaInfo : MonoBehaviour, IVoteAreaInfo
    {
        private CachedPlayerControl localPlayer;
        private TextMeshPro nameText;
        private TextMeshPro meetingInfo;

        private bool commActive;
        
        public void Init(PlayerVoteArea pva, bool commActive)
        {
            this.localPlayer = CachedPlayerControl.LocalPlayer;
            this.nameText = pva.NameText;

            this.meetingInfo = Instantiate(
                this.nameText, this.nameText.transform);
            IVoteAreaInfo.InitializeText(this.meetingInfo);
            this.commActive = commActive;

            base.gameObject.SetActive(true);
        }

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
            this.nameText.text = this.localPlayer.Data.PlayerName;

            if (this.localPlayer.Data.Role.IsImpostor)
            {
                this.nameText.color = Palette.ImpostorRed;
            }
            else
            {
                this.nameText.color = Palette.White;
            }
        }

        [HideFromIl2Cpp]
        private void setColor(
            SingleRoleBase role,
            GhostRoleBase ghostRole)
        {
            Color paintColor = role.GetNameColor(
                this.localPlayer.Data.IsDead);
            if (ghostRole != null)
            {
                Color ghostRoleColor = ghostRole.RoleColor;
                paintColor = (paintColor / 2.0f) + (ghostRoleColor / 2.0f);
            }
            if (paintColor == Palette.ClearWhite) { return; }

            this.nameText.color = paintColor;
        }

        [HideFromIl2Cpp]
        private void setInfo(
            SingleRoleBase role,
            GhostRoleBase ghostRole)
        {
            this.meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ?
                "" : getMeetingInfo(role, ghostRole);
            this.meetingInfo.gameObject.SetActive(true);
        }

        [HideFromIl2Cpp]
        private string getMeetingInfo(
            SingleRoleBase role, GhostRoleBase ghostRole)
        {
            var (tasksCompleted, tasksTotal) = GameSystem.GetTaskInfo(
                this.localPlayer.Data);
            string roleNames = role.GetColoredRoleName(this.localPlayer.Data.IsDead);

            if (ghostRole != null)
            {
                string ghostRoleName = ghostRole.GetColoredRoleName();
                roleNames = $"{ghostRoleName}({roleNames})";
            }

            var completedStr = this.commActive ? "?" : tasksCompleted.ToString();
            string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({completedStr}/{tasksTotal})</color>" : "";

            return $"{roleNames} {taskInfo}".Trim(); ;
        }

        [HideFromIl2Cpp]
        private void setTag(SingleRoleBase role)
        {
            string tag = role.GetRolePlayerNameTag(
                role, this.localPlayer.PlayerId);
            if (tag == string.Empty) { return; }
            this.nameText.text += tag;
        }
    }

    [Il2CppRegister]
    public sealed class OtherPlayerVoteAreaInfo : MonoBehaviour, IVoteAreaInfo
    {
        private CachedPlayerControl localPlayer;
        private TextMeshPro nameText;
        private TextMeshPro meetingInfo;
        private bool commActive;

        private GameData.PlayerInfo votePlayerInfo;

        public void Init(PlayerVoteArea pva, bool commActive)
        {
            this.localPlayer = CachedPlayerControl.LocalPlayer;
            this.nameText = pva.NameText;

            this.meetingInfo = Instantiate(
                this.nameText, this.nameText.transform);
            IVoteAreaInfo.InitializeText(this.meetingInfo);
            this.commActive = commActive;
            this.votePlayerInfo = GameData.Instance.GetPlayerById(pva.TargetPlayerId);
            
            base.gameObject.SetActive(true);
        }

        public void FixedUpdate()
        {
            resetInfo();

            SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
            SingleRoleBase targetRole = ExtremeRoleManager.GameRole[this.votePlayerInfo.PlayerId];

            GhostRoleBase ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
            bool targetPlyaerIsGhostRole = ExtremeGhostRoleManager.GameRole.TryGetValue(
                this.votePlayerInfo.PlayerId,
                out GhostRoleBase targetGhostRole);

            bool blockCondition = isBlockCondition(role) || targetPlyaerIsGhostRole;
            bool meetingInfoBlock = role.IsBlockShowMeetingRoleInfo() || targetPlyaerIsGhostRole;
            
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
            this.nameText.text = this.votePlayerInfo.PlayerName;
            
            if (this.localPlayer.Data.Role.IsImpostor &&
                this.votePlayerInfo.Role.IsImpostor)
            {
                this.nameText.color = Palette.ImpostorRed;
            }
            else
            {
                this.nameText.color = Palette.White;
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
                !this.localPlayer.Data.IsDead ||
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

                this.nameText.color = paintColor;

            }
            else
            {
                Color roleColor = targetRole.GetNameColor(true);
                if (!isMeetingInfoBlock || 
                    (targetRole.Team == localRole.Team))
                {
                    this.nameText.color = roleColor;
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
            if (!this.localPlayer.Data.IsDead || blockCondition)
            {
                this.meetingInfo.gameObject.SetActive(false);
            }
            else
            {
                this.meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? 
                    "" : getMeetingInfo(targetRole, targetGhostRole);
                this.meetingInfo.gameObject.SetActive(!isMeetingInfoBlock);
            }
        }

        [HideFromIl2Cpp]
        private string getMeetingInfo(
            SingleRoleBase targetRole,
            GhostRoleBase targetGhostRole)
        {
            var (tasksCompleted, tasksTotal) = GameSystem.GetTaskInfo(this.votePlayerInfo);
            string roleNames = targetRole.GetColoredRoleName(this.localPlayer.Data.IsDead);

            if (targetGhostRole != null)
            {
                string ghostRoleName = targetGhostRole.GetColoredRoleName();
                roleNames = $"{ghostRoleName}({roleNames})";
            }

            var completedStr = this.commActive ? "?" : tasksCompleted.ToString();
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

            this.nameText.text += tag;
        }

        [HideFromIl2Cpp]
        private bool isBlockCondition(SingleRoleBase role)
        {
            if (this.localPlayer.Data.Role.Role == RoleTypes.GuardianAngel)
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
