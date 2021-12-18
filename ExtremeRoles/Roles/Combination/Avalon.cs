using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.Combination
{
    public class Avalon : CombinationRoleManagerBase
    {
        public static string Name = "AvalonsRoles";
        public static Color SettingColor = Palette.White;
        public Avalon() : base(
            Name, new Color(255f, 255f, 255f), 2)
        {
            this.Roles.Add(new Assassin());
            this.Roles.Add(new Marlin());
        }
        protected override void RoleSpecificInit()
        {
            return;
        }

    }

    public class Assassin : MultiAssignRoleAbs
    {
        public enum AssassinOption
        {
            IsDeadForceMeeting,
            CanSeeRoleBeforeFirstMeeting,
        }

        public bool IsFirstMeeting = false;
        public bool CanSeeRoleBeforeFirstMeeting = false;
        public bool IsDeadForceMeeting = true;

        //private GameObject AssassinateUI;

        public Assassin(
            ) : base(
                ExtremeRoleId.Assassin,
                ExtremeRoleType.Impostor,
                ExtremeRoleId.Assassin.ToString(),
                Palette.ImpostorRed,
                true, false, true, true)
        {}
        protected override void CreateSpecificOption(
            CustomOption parentOps)
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

            killerPlayer.CmdReportDeadBody(rolePlayer.Data);
        }

        protected override void RoleSpecificInit()
        {
            this.IsDeadForceMeeting = OptionsHolder.AllOptions[
                GetRoleSettingId((int)AssassinOption.IsDeadForceMeeting)].GetBool();
            this.CanSeeRoleBeforeFirstMeeting = OptionsHolder.AllOptions[
                GetRoleSettingId((int)AssassinOption.CanSeeRoleBeforeFirstMeeting)].GetBool();
            this.IsFirstMeeting = true;
        }

        public void TryAssassinateMarin(
            GameData.PlayerInfo exileAssasin)
        {
            Patches.AssassinMeeting.AssassinMeetingTrigger = true;
            Patches.AssassinMeeting.ExiledAssassinId = exileAssasin.PlayerId;

            MeetingRoomManager.Instance.AssignSelf(exileAssasin.Object, null);
            DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(exileAssasin.Object);
            exileAssasin.Object.RpcStartMeeting(null);
        }
    }


    public class Marlin : MultiAssignRoleAbs
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
                new Color32(0, 40, 245, byte.MaxValue),
                false, false, false, false)
        {}
        protected override void CreateSpecificOption(
            CustomOption parentOps)
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

            this.HasTask = OptionsHolder.AllOptions[
                GetRoleSettingId((int)MarlinOption.HasTask)].GetBool();
            this.CanSeeVote = OptionsHolder.AllOptions[
                GetRoleSettingId((int)MarlinOption.CanSeeVote)].GetBool();
            this.CanSeeNeutral = OptionsHolder.AllOptions[
                GetRoleSettingId((int)MarlinOption.CanSeeNeutral)].GetBool();
        }

        public void SetPlayerIcon(
            Dictionary<byte, PoolablePlayer> playerIcons)
        {
            this.PlayerIcon = playerIcons;
            UpdateIcon();
        }
        public void UpdateIcon()
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
                SingleRoleAbs role = ExtremeRoleManager.GameRole[playerId];
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

    }
}
