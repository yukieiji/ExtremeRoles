using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.AbilityButton.Roles;

namespace ExtremeRoles.Roles.Combination
{
    public class TraitorManager : FlexibleCombinationRoleManagerBase
    {
        public TraitorManager() : base(new Traitor(), 1, false)
        { }

        protected override void CommonInit()
        {
            this.Roles.Clear();
            int roleAssignNum = 1;
            var allOptions = OptionHolder.AllOption;

            this.BaseRole.CanHasAnotherRole = true;

            if (allOptions.ContainsKey(GetRoleOptionId(CombinationRoleCommonOption.AssignsNum)))
            {
                roleAssignNum = allOptions[
                    GetRoleOptionId(CombinationRoleCommonOption.AssignsNum)].GetValue();
            }

            for (int i = 0; i < roleAssignNum; ++i)
            {
                this.Roles.Add((MultiAssignRoleBase)this.BaseRole.Clone());
            }
        }

    }

    public class Traitor : MultiAssignRoleBase, IRoleAbility, IRoleUpdate
    {
        public enum AbilityType : byte
        {
            Admin,
            Security,
            Vital,
        }

        private bool canUseButton;
        private ExtremeRoleId crewRole;
        private AbilityType curAbilityType;

        public RoleAbilityButtonBase Button
        { 
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public Traitor(
            ) : base(
                ExtremeRoleId.Traitor,
                ExtremeRoleType.Crewmate,
                ExtremeRoleId.Traitor.ToString(),
                ColorPalette.TraitorShikon,
                true, false, false, false)
        { }

        public void CreateAbility()
        {
            throw new System.NotImplementedException();
        }

        public bool UseAbility()
        {
            throw new System.NotImplementedException();
        }

        public void CleanUp()
        {

        }

        public bool IsAbilityUse() => this.IsCommonUse();


        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (!this.canUseButton && this.Button != null)
            {
                this.Button.SetActive(false);
            }
        }

        public override bool TryRolePlayerKillTo(PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            this.canUseButton = true;
            return true;
        }

        public override void OverrideAnotherRoleSetting()
        {
            this.CanHasAnotherRole = false;

            this.Team = ExtremeRoleType.Neutral;
            this.crewRole = this.AnotherRole.Id;
            
            byte rolePlayerId = byte.MaxValue;

            foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
            {
                if (this.GameControlId == role.GameControlId)
                {
                    rolePlayerId = playerId;
                    break;
                }
            }
            if (rolePlayerId == byte.MaxValue) { return; }

            if (CachedPlayerControl.LocalPlayer.PlayerId == rolePlayerId)
            {
                var abilityRole = this.AnotherRole as IRoleAbility;
                if (abilityRole != null)
                {
                    abilityRole.ResetOnMeetingStart();
                }
                var meetingResetRole = this.AnotherRole as IRoleResetMeeting;
                if (meetingResetRole != null)
                {
                    meetingResetRole.ResetOnMeetingStart();
                }
            }

            var resetRole = this.AnotherRole as IRoleSpecialReset;
            if (resetRole != null)
            {
                resetRole.AllReset(
                    Player.GetPlayerControlById(rolePlayerId));
            }
        }

        public override string GetIntroDescription()
        {
            return string.Format(
                base.GetIntroDescription(),
                Translation.GetString(this.crewRole.ToString()));
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
        }

        protected override void RoleSpecificInit()
        {
            this.canUseButton = false;
            this.curAbilityType = AbilityType.Admin;
        }
    }
}
