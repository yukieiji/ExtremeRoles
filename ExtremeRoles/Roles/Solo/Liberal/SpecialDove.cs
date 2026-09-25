using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;


public sealed class SpecialDove : SingleRoleBase, IRoleUpdate
{
	public enum SpecialDoveOption
	{
		UseVent = 0,
		TaskCompletedMoney,
	}

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
		factory.CreateBoolOption(SpecialDoveOption.UseVent, false);
		factory.CreateIntOption(SpecialDoveOption.TaskCompletedMoney, 5, 1, 1000, 1);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.UseVent = loader.GetValue<SpecialDoveOption, bool>(SpecialDoveOption.UseVent);
		float taskDelta = loader.GetValue<SpecialDoveOption, int>(SpecialDoveOption.TaskCompletedMoney);

		this.handler = new DoveCommonAbilityHandler(taskDelta, 0.0f);
	}
}
