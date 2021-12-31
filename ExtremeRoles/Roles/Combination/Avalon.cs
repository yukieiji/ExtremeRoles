using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Combination
{
    public class Avalon : ConstCombinationRoleManagerBase
    {
        public const string Name = "AvalonsRoles";
        public Color SettingColor = Palette.White;
        public Avalon() : base(
            Name, new Color(255f, 255f, 255f), 2)
        {
            this.Roles.Add(new Assassin());
            this.Roles.Add(new Marlin());
        }

    }

    public class Assassin : MultiAssignRoleBase
    {
        public enum AssassinOption
        {
            IsDeadForceMeeting,
            CanSeeRoleBeforeFirstMeeting,
        }

        public bool IsFirstMeeting = false;
        public bool CanSeeRoleBeforeFirstMeeting = false;
        public bool IsDeadForceMeeting = true;

        public Assassin(
            ) : base(
                ExtremeRoleId.Assassin,
                ExtremeRoleType.Impostor,
                ExtremeRoleId.Assassin.ToString(),
                Palette.ImpostorRed,
                true, false, true, true)
        {}
        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            var meetingOpt = CustomOption.Create(
                this.OptionIdOffset + (int)AssassinOption.IsDeadForceMeeting,
                Design.ConcatString(
                    this.RoleName,
                    AssassinOption.IsDeadForceMeeting.ToString()),
                true, parentOps);
            CustomOption.Create(
                this.OptionIdOffset + (int)AssassinOption.CanSeeRoleBeforeFirstMeeting,
                Design.ConcatString(
                    this.RoleName,
                    AssassinOption.CanSeeRoleBeforeFirstMeeting.ToString()),
                false, meetingOpt);
        }

        public override void ExiledAction(
            GameData.PlayerInfo rolePlayer)
        {
            TryAssassinateMarin(rolePlayer);
            this.IsFirstMeeting = false;
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
            if (!this.IsDeadForceMeeting) { return; }

            AssassinMeetingTriggerOn(rolePlayer.PlayerId);
            rolePlayer.CmdReportDeadBody(rolePlayer.Data);
        }

        protected override void RoleSpecificInit()
        {
            this.IsDeadForceMeeting = OptionsHolder.AllOption[
                GetRoleOptionId((int)AssassinOption.IsDeadForceMeeting)].GetValue();
            this.CanSeeRoleBeforeFirstMeeting = OptionsHolder.AllOption[
                GetRoleOptionId((int)AssassinOption.CanSeeRoleBeforeFirstMeeting)].GetValue();
            this.IsFirstMeeting = true;
        }

        private void TryAssassinateMarin(
            GameData.PlayerInfo exileAssasin)
        {
            AssassinMeetingTriggerOn(exileAssasin.PlayerId);

            MeetingRoomManager.Instance.AssignSelf(exileAssasin.Object, null);
            DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(exileAssasin.Object);
            exileAssasin.Object.RpcStartMeeting(null);
        }

        private void AssassinMeetingTriggerOn(
            byte playerId)
        {
            ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger = true;
            ExtremeRolesPlugin.GameDataStore.ExiledAssassinId = playerId;
        }

    }


    public class Marlin : MultiAssignRoleBase, IRoleUpdate
    {
        public enum MarlinOption
        {
            HasTask,
            CanSeeVote,
            CanSeeNeutral,
        }

        public bool IsAssassinate = false;
        public bool CanSeeVote = false;
        public bool CanSeeNeutral = false;

        private Dictionary<byte, PoolablePlayer> PlayerIcon = new Dictionary<byte, PoolablePlayer>();
        public Marlin(
            ) : base(
                ExtremeRoleId.Marlin,
                ExtremeRoleType.Crewmate,
                ExtremeRoleId.Marlin.ToString(),
                ColorPalette.MarineBlue,
                false, false, false, false)
        {}

        public void SetPlayerIcon(
            Dictionary<byte, PoolablePlayer> playerIcons)
        {
            this.PlayerIcon = playerIcons;
        }

        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {
            if (targetRole.IsImposter())
            {
                return Palette.ImpostorRed;
            }
            else if (targetRole.IsNeutral() && this.CanSeeNeutral)
            {
                return Palette.DisabledGrey;
            }
            return Palette.White;
        }

        public void Update(PlayerControl rolePlayer)
        {
            int visibleCounter = 0;
            Vector3 bottomLeft = HudManager.Instance.UseButton.transform.localPosition;
            bottomLeft.x *= -1;
            bottomLeft += new Vector3(-0.25f, -0.25f, 0);

            foreach (KeyValuePair<byte, PoolablePlayer> item in this.PlayerIcon)
            {
                byte playerId = item.Key;
                var poolPlayer = item.Value;
                if (playerId == PlayerControl.LocalPlayer.PlayerId) { continue; }

                PlayerControl player = Player.GetPlayerControlById(playerId);
                SingleRoleBase role = ExtremeRoleManager.GameRole[playerId];
                if (player.Data.IsDead ||
                    player.Data.Disconnected ||
                    role.IsCrewmate() ||
                    (role.IsNeutral() && !this.CanSeeNeutral))
                {
                    poolPlayer.gameObject.SetActive(false);
                }
                else
                {
                    poolPlayer.gameObject.SetActive(true);
                    poolPlayer.transform.localScale = Vector3.one * 0.25f;
                    poolPlayer.transform.localPosition = bottomLeft + Vector3.right * visibleCounter * 0.45f;
                    ++visibleCounter;
                }
            }
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
                this.OptionIdOffset + (int)MarlinOption.HasTask,
                Design.ConcatString(
                    this.RoleName,
                    MarlinOption.HasTask.ToString()),
                false, parentOps);
            CustomOption.Create(
                this.OptionIdOffset + (int)MarlinOption.CanSeeVote,
                Design.ConcatString(
                    this.RoleName,
                    MarlinOption.CanSeeVote.ToString()),
                false, parentOps);
            CustomOption.Create(
                this.OptionIdOffset + (int)MarlinOption.CanSeeNeutral,
                Design.ConcatString(
                    this.RoleName,
                    MarlinOption.CanSeeNeutral.ToString()),
                false, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.IsAssassinate = false;

            this.HasTask = OptionsHolder.AllOption[
                GetRoleOptionId((int)MarlinOption.HasTask)].GetValue();
            this.CanSeeVote = OptionsHolder.AllOption[
                GetRoleOptionId((int)MarlinOption.CanSeeVote)].GetValue();
            this.CanSeeNeutral = OptionsHolder.AllOption[
                GetRoleOptionId((int)MarlinOption.CanSeeNeutral)].GetValue();
        }
    }
}
