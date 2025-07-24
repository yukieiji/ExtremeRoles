
using ExtremeRoles.Roles.API;


using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Crewmate.SpecialCrew;

public sealed class SpecialCrewRole : SingleRoleBase
{
    public SpecialCrew(): base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.SpecialCrew,
			Palette.CrewmateBlue),
        false, true, false, false)
    { }

    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
    {
        return;
    }

    protected override void RoleSpecificInit()
    {
        return;
    }

}
