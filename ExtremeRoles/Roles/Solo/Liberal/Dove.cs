using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Core.CustomOption.Factory;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class Dove : SingleRoleBase, IRoleUpdate
{
	private readonly DoveCommonAbilityHandler handler;

	public Dove(
		DoveCommonAbilityHandler abilityHandler,
		LiberalDefaultOptionLoader option) : base(
		RoleArgs.BuildLiberalDove(ExtremeRoleId.Dove))
	{
		this.handler = abilityHandler;
		LiberalSettingOverrider.OverrideDefault(this, option);
	}

	public void Update(PlayerControl rolePlayer)
	{
		this.handler.Update(rolePlayer);
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		this.handler.ClearTask(rolePlayer);
	}

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		this.handler.ClearTask(rolePlayer);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{

	}

	protected override void RoleSpecificInit()
	{

	}
}
