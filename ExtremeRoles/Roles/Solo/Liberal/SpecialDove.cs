using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Roles.API.Interface;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class SpecialDove : SingleRoleBase, IRoleUpdate
{
	private DoveCommonAbilityHandler? handler;

	public SpecialDove() : base(
		RoleArgs.BuildLiberalDove(ExtremeRoleId.SpecialDove))
	{
	}

	public void Update(PlayerControl rolePlayer)
	{
		this.handler?.Update(rolePlayer);
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		this.handler?.ClearTask(rolePlayer);
	}

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		this.handler?.ClearTask(rolePlayer);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{

	}

	protected override void RoleSpecificInit()
	{
		var liberalOption = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptionLoader>();
		LiberalSettingOverrider.OverrideDefault(this, liberalOption);

		this.handler = new DoveCommonAbilityHandler(liberalOption);
	}
}
