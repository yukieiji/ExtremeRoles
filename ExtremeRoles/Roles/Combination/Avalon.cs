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
        public Avalon() : base(
            Name, new Color(255f, 255f, 255f), 2,
            OptionHolder.MaxImposterNum)
        {
            this.Roles.Add(new Assassin());
            this.Roles.Add(new Marlin());
        }

    }

    public class Assassin : MultiAssignRoleBase
    {
        public enum AssassinOption
        {
            CanKilled,
            CanKilledFromCrew,
            CanKilledFromNeutral,
            IsDeadForceMeeting,
            CanSeeRoleBeforeFirstMeeting,
        }

        public bool IsFirstMeeting = false;
        public bool CanSeeRoleBeforeFirstMeeting = false;

        private bool canKilled = false;
        private bool canKilledFromCrew = false;
        private bool canKilledFromNeutral = false;
        private bool isDeadForceMeeting = true;

        public Assassin(
            ) : base(
                ExtremeRoleId.Assassin,
                ExtremeRoleType.Impostor,
                ExtremeRoleId.Assassin.ToString(),
                Palette.ImpostorRed,
                true, false, true, true)
        {}

        public override bool TryRolePlayerKilledFrom(
            PlayerControl rolePlayer, PlayerControl fromPlayer)
        {
            if (!this.canKilled) { return false; }

            var fromPlayerRole = ExtremeRoleManager.GameRole[fromPlayer.PlayerId];

            if (fromPlayerRole.IsNeutral())
            {
                return this.canKilledFromNeutral;
            }
            else if (fromPlayerRole.IsCrewmate())
            {
                return this.canKilledFromCrew;
            }

            return false;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            var killedOps = CustomOption.Create(
                GetRoleOptionId((int)AssassinOption.CanKilled),
                string.Concat(
                    this.RoleName,
                    AssassinOption.CanKilled.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)AssassinOption.CanKilledFromCrew),
                string.Concat(
                    this.RoleName,
                    AssassinOption.CanKilledFromCrew.ToString()),
                false, killedOps);

            CustomOption.Create(
                GetRoleOptionId((int)AssassinOption.CanKilledFromNeutral),
                string.Concat(
                    this.RoleName,
                    AssassinOption.CanKilledFromNeutral.ToString()),
                false, killedOps);

            var meetingOpt = CustomOption.Create(
                GetRoleOptionId((int)AssassinOption.IsDeadForceMeeting),
                string.Concat(
                    this.RoleName,
                    AssassinOption.IsDeadForceMeeting.ToString()),
                true, killedOps);
            CustomOption.Create(
                GetRoleOptionId((int)AssassinOption.CanSeeRoleBeforeFirstMeeting),
                string.Concat(
                    this.RoleName,
                    AssassinOption.CanSeeRoleBeforeFirstMeeting.ToString()),
                false, meetingOpt);
        }

        public override void ExiledAction(
            GameData.PlayerInfo rolePlayer)
        {

            assassinMeetingTriggerOn(rolePlayer.PlayerId);
            if (AmongUsClient.Instance.AmHost)
            {
                MeetingRoomManager.Instance.AssignSelf(rolePlayer.Object, null);
                DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(rolePlayer.Object);
                rolePlayer.Object.RpcStartMeeting(null);
            }

            this.IsFirstMeeting = false;
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
            if (!this.isDeadForceMeeting)
            {
                RPCOperator.Call(
                    rolePlayer.NetId,
                    RPCOperator.Command.AssasinAddDead,
                    new List<byte> { rolePlayer.PlayerId });
                AddDead(rolePlayer.PlayerId);
                return; 
            }

            assassinMeetingTriggerOn(rolePlayer.PlayerId);
            killerPlayer.CmdReportDeadBody(rolePlayer.Data);
            this.IsFirstMeeting = false;
        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionHolder.AllOption;


            this.canKilled = allOption[
                GetRoleOptionId((int)AssassinOption.CanKilled)].GetValue();
            this.canKilledFromCrew = allOption[
                GetRoleOptionId((int)AssassinOption.CanKilledFromCrew)].GetValue();
            this.canKilledFromNeutral = allOption[
                GetRoleOptionId((int)AssassinOption.CanKilledFromNeutral)].GetValue();

            this.isDeadForceMeeting = allOption[
                GetRoleOptionId((int)AssassinOption.IsDeadForceMeeting)].GetValue();
            this.CanSeeRoleBeforeFirstMeeting = allOption[
                GetRoleOptionId((int)AssassinOption.CanSeeRoleBeforeFirstMeeting)].GetValue();
            this.IsFirstMeeting = true;
        }

        private void assassinMeetingTriggerOn(
            byte playerId)
        {
            ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger = true;
            ExtremeRolesPlugin.GameDataStore.ExiledAssassinId = playerId;
        }

        public static void AddDead(byte playerId)
        {
            ExtremeRolesPlugin.GameDataStore.DeadedAssassin.Add(
                playerId);
        }

        public static void VoteFor(byte targetId)
        {
            ExtremeRolesPlugin.GameDataStore.AssassinateMarin = 
                ExtremeRoleManager.GameRole[
                    targetId].Id == ExtremeRoleId.Marlin;
            ExtremeRolesPlugin.GameDataStore.IsMarinPlayerId = targetId;
        }

    }


    public class Marlin : MultiAssignRoleBase, IRoleSpecialSetUp, IRoleResetMeeting
    {
        public enum MarlinOption
        {
            HasTask,
            CanSeeVote,
            CanSeeNeutral,
            CanUseVent,
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

        public void IntroBeginSetUp()
        {
            return;
        }

        public void IntroEndSetUp()
        {
            this.PlayerIcon = Player.CreatePlayerIcon();
            this.showIcon();
        }

        public void ResetOnMeetingEnd()
        {
            this.showIcon();
        }

        public void ResetOnMeetingStart()
        {
            foreach (var (_, poolPlayer) in this.PlayerIcon)
            {
                poolPlayer.gameObject.SetActive(false);
            }
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

            var multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.GetTargetRoleSeeColor(
                        multiAssignRole.AnotherRole,
                        targetPlayerId);
                }
            }

            return Palette.White;
        }


        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId((int)MarlinOption.HasTask),
                string.Concat(
                    this.RoleName,
                    MarlinOption.HasTask.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleOptionId((int)MarlinOption.CanSeeVote),
                string.Concat(
                    this.RoleName,
                    MarlinOption.CanSeeVote.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleOptionId((int)MarlinOption.CanSeeNeutral),
                string.Concat(
                    this.RoleName,
                    MarlinOption.CanSeeNeutral.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleOptionId((int)MarlinOption.CanUseVent),
                string.Concat(
                    this.RoleName,
                    MarlinOption.CanUseVent.ToString()),
                false, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.IsAssassinate = false;

            this.HasTask = OptionHolder.AllOption[
                GetRoleOptionId((int)MarlinOption.HasTask)].GetValue();
            this.CanSeeVote = OptionHolder.AllOption[
                GetRoleOptionId((int)MarlinOption.CanSeeVote)].GetValue();
            this.CanSeeNeutral = OptionHolder.AllOption[
                GetRoleOptionId((int)MarlinOption.CanSeeNeutral)].GetValue();
            this.UseVent = OptionHolder.AllOption[
                GetRoleOptionId((int)MarlinOption.CanUseVent)].GetValue();
        }

        private void showIcon()
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
                if (role.IsCrewmate() ||
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
