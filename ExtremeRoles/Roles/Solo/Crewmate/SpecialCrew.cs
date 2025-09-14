
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Factory.Old;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class SpecialCrew : SingleRoleBase
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
