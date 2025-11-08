using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;
using System;

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class Leader : SingleRoleBase
{
	public Leader() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Leader,
			ColorPalette.AgencyYellowGreen),
		false, false, false, false)
	{ }

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
	}

	protected override void RoleSpecificInit()
	{

	}
}
