using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Combination
{
    public sealed class Avalon : ConstCombinationRoleManagerBase
    {
        public const string Name = "AvalonsRoles";
        public Avalon() : base(
            Name, new Color(255f, 255f, 255f), 2,
            GameSystem.MaxImposterNum)
        {
            this.Roles.Add(new Assassin());
            this.Roles.Add(new Marlin());
        }
    }

    public sealed class Assassin : MultiAssignRoleBase
    {
        public enum AssassinOption
        {
            HasTask,
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
                true, false, true, true,
                tab: OptionTab.Combination)
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
            IOption parentOps)
        {
            CreateBoolOption(
                AssassinOption.HasTask,
                false, parentOps);
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
            
            if (isServant()) { return; }

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

            if (isServant()) { return; }

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
            if (ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger)
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

            this.HasTask = allOption[
                GetRoleOptionId(AssassinOption.HasTask)].GetValue();
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
        }

        private void assassinMeetingTriggerOn(
            byte playerId)
        {
            ExtremeRolesPlugin.ShipState.AssassinMeetingTriggerOn(
                playerId);
        }

        public static void AddDead(byte playerId)
        {
            ExtremeRolesPlugin.ShipState.AddDeadAssasin(playerId);
        }

        public static void VoteFor(byte targetId)
        {
            ExtremeRolesPlugin.ShipState.SetAssassnateTarget(targetId);
        }
        private bool isServant() => this.AnotherRole?.Id == ExtremeRoleId.Servant;
    }


    public sealed class Marlin : MultiAssignRoleBase, IRoleSpecialSetUp, IRoleResetMeeting
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
        private GridArrange grid;

        private Dictionary<byte, PoolablePlayer> PlayerIcon;
        public Marlin(
            ) : base(
                ExtremeRoleId.Marlin,
                ExtremeRoleType.Crewmate,
                ExtremeRoleId.Marlin.ToString(),
                ColorPalette.MarineBlue,
                false, false, false, false,
                tab: OptionTab.Combination)
        {}

        public void IntroBeginSetUp()
        {
            return;
        }

        public void IntroEndSetUp()
        {
            GameObject bottomLeft = new GameObject("BottomLeft");
            bottomLeft.transform.SetParent(
                FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.parent.parent);
            AspectPosition aspectPosition = bottomLeft.AddComponent<AspectPosition>();
            aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
            aspectPosition.anchorPoint = new Vector2(0.5f, 0.5f);
            aspectPosition.DistanceFromEdge = new Vector3(0.375f, 0.35f);
            aspectPosition.AdjustPosition();

            this.grid = bottomLeft.AddComponent<GridArrange>();
            this.grid.CellSize = new Vector2(0.625f, 0.75f);
            this.grid.MaxColumns = 10;
            this.grid.Alignment = GridArrange.StartAlign.Right;
            this.grid.cells = new();

            this.PlayerIcon = Player.CreatePlayerIcon(
                bottomLeft.transform, Vector3.one * 0.275f);
            this.updateShowIcon();
        }

        public void ResetOnMeetingEnd()
        {
            this.updateShowIcon();
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
                return ColorPalette.NeutralColor;
            }

            return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
        }


        protected override void CreateSpecificOption(
            IOption parentOps)
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
            this.PlayerIcon = new Dictionary<byte, PoolablePlayer>();
        }

        private void updateShowIcon()
        {
            foreach (var(playerId, poolPlayer) in this.PlayerIcon)
            {
                if (playerId == CachedPlayerControl.LocalPlayer.PlayerId) { continue; }

                SingleRoleBase role = ExtremeRoleManager.GameRole[playerId];
                if (role.IsCrewmate() ||
                    (role.IsNeutral() && !this.CanSeeNeutral) ||
                    (role.Id == ExtremeRoleId.Assassin && !this.canSeeAssassin))
                {
                    poolPlayer.gameObject.SetActive(false);
                }
                else
                {
                    poolPlayer.transform.localScale = Vector3.one * 0.275f;
                    poolPlayer.gameObject.SetActive(true);
                }
            }
            this.grid.ArrangeChilds();
        }
    }
}
