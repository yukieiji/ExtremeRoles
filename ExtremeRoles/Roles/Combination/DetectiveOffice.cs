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

    public class Detective : MultiAssignRoleBase, IRoleReportHock, IRoleUpdate
    {
        public Detective() : base(
            ExtremeRoleId.Detective,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Detective.ToString(),
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

        public void Update(PlayerControl rolePlayer)
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
