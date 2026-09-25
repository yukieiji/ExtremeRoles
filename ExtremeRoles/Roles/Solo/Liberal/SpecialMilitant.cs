using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GameMode.RoleSelector;
using Microsoft.Extensions.DependencyInjection;

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
		var liberalOption = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptionLoader>();
		LiberalSettingOverrider.OverrideDefault(this, liberalOption);
	}
}
