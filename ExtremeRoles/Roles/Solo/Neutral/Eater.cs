using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;

using ExtremeRoles.Module.AbilityButton.Roles;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Eater : SingleRoleBase, IRoleAbility, IRoleUpdate
    {



        public enum EaterOption
        {
            DeadBodyEateRange,
        }

        public RoleAbilityButtonBase Button
        { 
            get => this.eatButton;
            set
            {
                this.eatButton = value;
            }
        }

        private RoleAbilityButtonBase eatButton;
        
        private float range;

        public Eater() : base(
           ExtremeRoleId.Eater,
           ExtremeRoleType.Neutral,
           ExtremeRoleId.Eater.ToString(),
           ColorPalette.TotocalcioGreen,
           false, false, false, false)
        { }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Helper.Translation.GetString("bet"),
                Loader.CreateSpriteFromResources(
                    Path.TestButton));
        }

        public bool IsAbilityUse()
        {

            return this.IsCommonUse();
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            
        }

        public bool UseAbility()
        {
            
            return true;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (this.eatButton == null) { return; }



        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            var multiAssignRole = targetRole as MultiAssignRoleBase;

            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.IsSameTeam(multiAssignRole.AnotherRole);
                }
            }
            if (OptionHolder.Ship.IsSameNeutralSameWin)
            {
                return this.Id == targetRole.Id;
            }
            else
            {
                return (this.Id == targetRole.Id) && this.IsSameControlId(targetRole);
            }
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            CreateFloatOption(
                EaterOption.DeadBodyEateRange,
                1.0f, 0.0f, 2.0f, 0.1f,
                parentOps);

            
        }

        protected override void RoleSpecificInit()
        {

        }
    }
}
