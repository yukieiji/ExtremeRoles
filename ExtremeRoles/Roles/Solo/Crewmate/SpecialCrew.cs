using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class SpecialCrew : SingleRoleBase
    {
        public SpecialCrew(): base(
            ExtremeRoleId.SpecialCrew,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.SpecialCrew.ToString(),
            Palette.CrewmateBlue,
            false, true, false, false)
        {}

        protected override void CreateSpecificOption(IOption parentOps)
        {
            return;
        }
        
        protected override void RoleSpecificInit()
        {
            return;
        }

    }
}
