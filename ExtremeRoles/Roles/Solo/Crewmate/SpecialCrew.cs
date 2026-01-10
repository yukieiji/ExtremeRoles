
using ExtremeRoles.Roles.API;
using ExtremeRoles.Core.Service.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class SpecialCrew : SingleRoleBase
{
    public SpecialCrew(): base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.SpecialCrew,
			Palette.CrewmateBlue))
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
