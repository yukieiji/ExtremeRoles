using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class SecurityGuard : SingleRoleBase
    {
        public SecurityGuard() : base(
            ExtremeRoleId.SecurityGuard,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.SecurityGuard.ToString(),
            Palette.CrewmateBlue,
            false, true, false, false)
        { }

        protected override void CreateSpecificOption(CustomOptionBase parentOps)
        {
            return;
        }

        protected override void RoleSpecificInit()
        {
            return;
        }

    }
}
