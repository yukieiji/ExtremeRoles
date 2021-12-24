using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Alice : SingleRoleAbs
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


        protected override void CreateSpecificOption(
            CustomOption parentOps)
        {}

        protected override void RoleSpecificInit()
        {}

    }
}
