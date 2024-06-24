
using ExtremeRoles.Roles.API;


using ExtremeRoles.Module.NewOption.Factory;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class SpecialCrew : SingleRoleBase
{
    public SpecialCrew(): base(
        ExtremeRoleId.SpecialCrew,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.SpecialCrew.ToString(),
        Palette.CrewmateBlue,
        false, true, false, false)
    {}

    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
    {
        return;
    }

    protected override void RoleSpecificInit()
    {
        return;
    }

}
