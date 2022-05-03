using System;
using System.Collections.Generic;

using UnityEngine;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;


namespace ExtremeRoles.Roles.Combination
{
    public class DetectiveOffice : ConstCombinationRoleManagerBase
    {

        public const string Name = "DetectiveOffice";

        public DetectiveOffice() : base(
            Name, new Color(255f, 255f, 255f), 2)
        {
            this.Roles.Add(new Detective());
            this.Roles.Add(new Assistant());
        }
    }

    public class Detective : MultiAssignRoleBase, IRoleMurderPlayerHock, IRoleResetMeeting, IRoleReportHock, IRoleUpdate
    {
        public struct CrimeInfo
        {
            public Vector2 Position;
            public DateTime KilledTime;
            public float ReportTime;
            public ExtremeRoleType KillerTeam;
            public ExtremeRoleId KillerRole;
        }

        public class CrimeInfoContainer
        {
            public Dictionary<byte, (Vector2, DateTime)> deadBodyInfo = new Dictionary<byte, (Vector2, DateTime)>();
            public Dictionary<byte, float> timer = new Dictionary<byte, float>();

            public CrimeInfoContainer()
            {
                this.Clear();
            }

            public void Clear()
            {
                this.deadBodyInfo.Clear();
                this.timer.Clear();
            }

            public void AddDeadBody(
                PlayerControl deadPlayer)
            {
                this.deadBodyInfo.Add(
                    deadPlayer.PlayerId,
                    (
                        deadPlayer.GetTruePosition(),
                        DateTime.UtcNow
                    ));
                this.timer.Add(
                    deadPlayer.PlayerId,
                    0.0f);
            }

            public CrimeInfo? GetCrimeInfo(byte playerId)
            {
                if (!this.deadBodyInfo.ContainsKey(playerId))
                { 
                    return null;
                }

                var (pos, time) = this.deadBodyInfo[playerId];
                var role = ExtremeRoleManager.GameRole[playerId];

                return new CrimeInfo()
                {
                    Position = pos,
                    KilledTime = time,
                    ReportTime = this.timer[playerId],
                    KillerTeam = role.Team,
                    KillerRole = role.Id,
                };
            }

            public void Update()
            {
                foreach (byte playerId in this.timer.Keys)
                {
                    this.timer[playerId] = this.timer[playerId] += Time.fixedDeltaTime;
                }
            }
        }

        private CrimeInfo? targetCrime;
        private bool isAssistantReport = false;
        private CrimeInfoContainer info;
        private Arrow crimeArrow;
        private float searchTime;
        private float searchAssistantTime;
        private float timer = 0.0f;
        private float range;

        public Detective() : base(
            ExtremeRoleId.Detective,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Detective.ToString(),
            Palette.White,
            false, true, false, false)
        { }

        public void ResetOnMeetingEnd()
        {
            this.info.Clear();
        }

        public void ResetOnMeetingStart()
        {
            if (this.crimeArrow != null)
            {
                this.crimeArrow.SetActive(false);
            }
        }

        public void HockReportButton(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter)
        {
            this.targetCrime = null;
            this.isAssistantReport = false;
        }

        public void HockBodyReport(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter,
            GameData.PlayerInfo reportBody)
        {
            this.targetCrime = this.info.GetCrimeInfo(reportBody.PlayerId);
            this.isAssistantReport = ExtremeRoleManager.GameRole[
                reporter.PlayerId].Id == ExtremeRoleId.Assistant;
        }

        public void HockMuderPlayer(
            PlayerControl source, PlayerControl target)
        {
            this.info.AddDeadBody(target);
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (this.targetCrime != null)
            {
                if (this.crimeArrow == null)
                {
                    this.crimeArrow = new Arrow(
                        Color.white);
                }

                var crime = (CrimeInfo)this.targetCrime;
                Vector2 crimePos = crime.Position;

                this.crimeArrow.SetActive(true);
                this.crimeArrow.UpdateTarget(crimePos);
                this.crimeArrow.Update();

                Vector2 playerPos = rolePlayer.GetTruePosition();

                if (!PhysicsHelpers.AnythingBetween(
                        crimePos, playerPos,
                        Constants.ShipAndAllObjectsMask, false) &&
                    Vector2.Distance(crimePos, playerPos) < this.range)
                {
                    if (this.timer > this.searchAssistantTime && this.isAssistantReport)
                    {

                    }
                    else if (this.timer > this.searchTime)
                    {
                    
                    }
                    this.timer += Time.fixedDeltaTime;
                }
                else
                {
                    this.timer = 0;
                }
            }
            if (this.crimeArrow != null)
            {
                this.crimeArrow.SetActive(false);
            }
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new System.NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            this.info = new CrimeInfoContainer();
            this.info.Clear();
        }
    }

    public class Assistant : MultiAssignRoleBase, IRoleReportHock
    {
        public Assistant() : base(
            ExtremeRoleId.Assistant,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Assistant.ToString(),
            Palette.White,
            false, true, false, false)
        {

        }
        public void HockReportButton(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter)
        {
            throw new System.NotImplementedException();
        }

        public void HockBodyReport(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter,
            GameData.PlayerInfo reportBody)
        {
            throw new System.NotImplementedException();
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new System.NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new System.NotImplementedException();
        }
    }

    public class DetectiveApprentice : SingleRoleBase, IRoleAbility, IRoleReportHock
    {

        public RoleAbilityButtonBase Button
        { 
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public DetectiveApprentice() : base(
            ExtremeRoleId.DetectiveApprentice,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.DetectiveApprentice.ToString(),
            Palette.White,
            false, true, false, false)
        {

        }

        public void CreateAbility()
        {
            throw new System.NotImplementedException();
        }

        public bool IsAbilityUse()
        {
            throw new System.NotImplementedException();
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            throw new System.NotImplementedException();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            throw new System.NotImplementedException();
        }

        public bool UseAbility()
        {
            throw new System.NotImplementedException();
        }

        public void HockReportButton(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter)
        {
            throw new System.NotImplementedException();
        }

        public void HockBodyReport(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter,
            GameData.PlayerInfo reportBody)
        {
            throw new System.NotImplementedException();
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new System.NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
