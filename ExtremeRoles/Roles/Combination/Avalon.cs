using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

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
            CanSeeVote,
        }

        public bool IsFirstMeeting = false;
        public bool CanSeeRoleBeforeFirstMeeting = false;
        public bool CanSeeVote = false;

        public bool CanKilled = false;
        public bool CanKilledFromCrew = false;
        public bool CanKilledFromNeutral = false;
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
            if (!this.CanKilled) { return false; }

            var fromPlayerRole = ExtremeRoleManager.GameRole[fromPlayer.PlayerId];

            if (fromPlayerRole.IsNeutral())
            {
                return this.CanKilledFromNeutral;
            }
            else if (fromPlayerRole.IsCrewmate())
            {
                return this.CanKilledFromCrew;
            }

            return false;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            var killedOps = CreateBoolOption(
                AssassinOption.CanKilled,
                false, parentOps);
            CreateBoolOption(
                AssassinOption.CanKilledFromCrew,
                false, killedOps);
            CreateBoolOption(
                AssassinOption.CanKilledFromNeutral,
                false, killedOps);
            var meetingOpt = CreateBoolOption(
                AssassinOption.IsDeadForceMeeting,
                true, killedOps);
            CreateBoolOption(
                AssassinOption.CanSeeRoleBeforeFirstMeeting,
                false, meetingOpt);

            CreateBoolOption(
                 AssassinOption.CanSeeVote,
                true, parentOps);
        }

        public override void ExiledAction(
            GameData.PlayerInfo rolePlayer)
        {

            assassinMeetingTriggerOn(rolePlayer.PlayerId);
            if (AmongUsClient.Instance.AmHost)
            {
                MeetingRoomManager.Instance.AssignSelf(rolePlayer.Object, null);
                FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(rolePlayer.Object);
                rolePlayer.Object.RpcStartMeeting(null);
            }

            this.IsFirstMeeting = false;
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
            if (!this.isDeadForceMeeting || MeetingHud.Instance != null)
            {
                AddDead(rolePlayer.PlayerId);
                return; 
            }

            assassinMeetingTriggerOn(rolePlayer.PlayerId);
            if (AmongUsClient.Instance.AmHost &&
                rolePlayer.PlayerId == killerPlayer.PlayerId)
            {
                MeetingRoomManager.Instance.AssignSelf(rolePlayer, null);
                FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(rolePlayer);
                rolePlayer.RpcStartMeeting(null);
            }
            else
            {
                killerPlayer.ReportDeadBody(rolePlayer.Data);
            }
            this.IsFirstMeeting = false;
        }

        public override bool IsBlockShowPlayingRoleInfo()
        {
            return !this.IsFirstMeeting && !this.CanSeeRoleBeforeFirstMeeting;
        }

        public override bool IsBlockShowMeetingRoleInfo()
        {
            if (ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger)
            { 
                return true; 
            }
            else if (this.CanSeeRoleBeforeFirstMeeting)
            {
                return this.IsFirstMeeting;
            }

            return false;

        }
        protected override void RoleSpecificInit()
        {
            var allOption = OptionHolder.AllOption;


            this.CanKilled = allOption[
                GetRoleOptionId(AssassinOption.CanKilled)].GetValue();
            this.CanKilledFromCrew = allOption[
                GetRoleOptionId(AssassinOption.CanKilledFromCrew)].GetValue();
            this.CanKilledFromNeutral = allOption[
                GetRoleOptionId(AssassinOption.CanKilledFromNeutral)].GetValue();
            this.CanSeeVote = allOption[
                GetRoleOptionId(AssassinOption.CanSeeVote)].GetValue();

            this.isDeadForceMeeting = allOption[
                GetRoleOptionId(AssassinOption.IsDeadForceMeeting)].GetValue();
            this.CanSeeRoleBeforeFirstMeeting = allOption[
                GetRoleOptionId(AssassinOption.CanSeeRoleBeforeFirstMeeting)].GetValue();
            this.IsFirstMeeting = true;

            ExtremeRolesPlugin.GameDataStore.IsAssassinAssign = true;

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
            CanSeeAssassin,
            CanSeeVote,
            CanSeeNeutral,
            CanUseVent,
        }

        public bool IsAssassinate = false;
        public bool CanSeeVote = false;
        public bool CanSeeNeutral = false;
        private bool canSeeAssassin = false;

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
            if (targetRole.Id == ExtremeRoleId.Assassin && !this.canSeeAssassin)
            {
                return Palette.White;
            }
            else if (targetRole.IsImpostor())
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
            CreateBoolOption(
                MarlinOption.HasTask,
                false, parentOps);

            CreateBoolOption(
                MarlinOption.CanSeeAssassin,
                true, parentOps);

            CreateBoolOption(
                MarlinOption.CanSeeVote,
                true, parentOps);
            CreateBoolOption(
                MarlinOption.CanSeeNeutral,
                false, parentOps);
            CreateBoolOption(
                MarlinOption.CanUseVent,
                false, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.IsAssassinate = false;

            var allOption = OptionHolder.AllOption;

            this.HasTask = allOption[
                GetRoleOptionId(MarlinOption.HasTask)].GetValue();
            this.canSeeAssassin = allOption[
                GetRoleOptionId(MarlinOption.CanSeeAssassin)].GetValue();
            this.CanSeeVote = allOption[
                GetRoleOptionId(MarlinOption.CanSeeVote)].GetValue();
            this.CanSeeNeutral = allOption[
                GetRoleOptionId(MarlinOption.CanSeeNeutral)].GetValue();
            this.UseVent = allOption[
                GetRoleOptionId(MarlinOption.CanUseVent)].GetValue();
        }

        private void showIcon()
        {
            int visibleCounter = 0;
            Vector3 bottomLeft = FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition;
            bottomLeft.x *= -1;
            bottomLeft += new Vector3(-0.25f, -0.25f, 0);

            foreach (KeyValuePair<byte, PoolablePlayer> item in this.PlayerIcon)
            {
                byte playerId = item.Key;
                var poolPlayer = item.Value;
                if (playerId == CachedPlayerControl.LocalPlayer.PlayerId) { continue; }

                PlayerControl player = Player.GetPlayerControlById(playerId);
                SingleRoleBase role = ExtremeRoleManager.GameRole[playerId];
                if (role.IsCrewmate() ||
                    (role.IsNeutral() && !this.CanSeeNeutral) ||
                    (role.Id == ExtremeRoleId.Assassin && !this.canSeeAssassin))
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
