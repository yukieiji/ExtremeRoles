using TMPro;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.GameMode.RoleSelector;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class Militant : SingleRoleBase
{
	public Militant(LiberalDefaultOptipnLoader option) : base(
		RoleCore.BuildLiberal(
			ExtremeRoleId.Militant,
			ColorPalette.AgencyYellowGreen),
		true, false,
		false, false)
	{

		LiberalSettingOverrider.OverrideDefault(this, option);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{

	}

	protected override void RoleSpecificInit()
	{

	}
}
