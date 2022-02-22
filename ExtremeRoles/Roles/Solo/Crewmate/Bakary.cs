using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Bakary : SingleRoleBase
    {
        public Bakary() : base(
            ExtremeRoleId.Bakary,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Bakary.ToString(),
            ColorPalette.BakaryWheatColor,
            false, true, false, false)
        { }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            return;
        }

        protected override void RoleSpecificInit()
        {
            return;
        }
    }
}
