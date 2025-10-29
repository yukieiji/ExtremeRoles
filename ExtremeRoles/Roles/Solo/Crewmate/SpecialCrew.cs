
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class SpecialCrew : SingleRoleBase
{
    public SpecialCrew(): base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.SpecialCrew,
			Palette.CrewmateBlue),
        false, true, false, false)
    { }

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
	{
		return;
    }

    protected override void RoleSpecificInit()
    {
        return;
    }

}
