using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Alice : SingleRoleBase, IRoleAbility
    {

        public enum AliceOption
        {
            RevartCommonTaskNum,
            RevartLongTaskNum,
            RevartShotTaskNum,
        }

        public RoleAbilityButton Button
        { 
            get => this.aliceShipBroken;
            set
            {
                this.aliceShipBroken = value;
            }
        }

        private RoleAbilityButton aliceShipBroken;

        public Alice(): base(
            ExtremeRoleId.Alice,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Alice.ToString(),
            ColorPalette.AliceGold,
            true, false, true, true)
        {}

        public void CreateAbility()
        {
            this.Button = this.CreateAbilityButton(
                Helper.Resources.LoadSpriteFromResources(
                    Resources.ResourcesPaths.TestButton, 115f));
        }

        public override bool IsSameTeam(SingleRoleBase targetRole) => targetRole.Id == this.Id;

        public bool IsAbilityUse()
        {
            return this.IsCommonUse();
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
           if (ExtremeRoleManager.GameRole[killerPlayer.PlayerId].IsImposter())
           {
                this.IsWin = true;
           }
        }

        public void UseAbility()
        {
            Helper.Logging.Debug("Ability On");
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            this.CreateRoleAbilityOption(parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();
        }
    }
}
