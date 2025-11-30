using TMPro;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class Dove : SingleRoleBase, IRoleUpdate
{
	private readonly DoveCommonAbilityHandler handler;

	public Dove(
		DoveCommonAbilityHandler abilityHandler,
		LiberalDefaultOptipnLoader option) : base(
		RoleCore.BuildLiberal(
			ExtremeRoleId.Dove,
			ColorPalette.AgencyYellowGreen),
		false, true,
		false, false)
	{
		this.handler = abilityHandler;
		LiberalSettingOverrider.OverrideDefault(this, option);
	}

	public void Update(PlayerControl rolePlayer)
	{
		this.handler.Update(rolePlayer);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{

	}

	protected override void RoleSpecificInit()
	{

	}
}
