using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Alice : SingleRoleAbs, IRoleAbility
    {
        public Alice(): base(
            ExtremeRoleId.Alice,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Alice.ToString(),
            ColorPalette.AliceGold,
            true, false, true, true)
        {}

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
           if (ExtremeRoleManager.GameRole[killerPlayer.PlayerId].IsImposter())
           {
                this.IsWin = true;
           }
        }

        public void CreateAbility()
        {
            RoleAbilityButton abilityButton = new RoleAbilityButton();
            abilityButton.ButtonInit(
                Resources.ResourcesPaths.TestButton,
                Helper.Design.ConcatString(this.RoleName, "Ability"));
            abilityButton.SetEnabled();
            abilityButton.gameObject.SetActive(true);
            abilityButton.SetCoolDown(0f, 1f);
        }

        public void UseAbility()
        {
            Helper.Logging.Debug("Ability Test");
        }

        protected override void CreateSpecificOption(
            CustomOption parentOps)
        {}

        protected override void RoleSpecificInit()
        {}

    }
}
