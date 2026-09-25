using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class SpecialMilitant : SingleRoleBase
{
	public SpecialMilitant() : base(
		RoleArgs.BuildLiberalMilitant(ExtremeRoleId.SpecialMilitant))
	{
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{

	}

	protected override void RoleSpecificInit()
	{

	}
}
