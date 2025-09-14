using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.GhostRoles;

using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory;

public sealed class GhostRoleOptionFactoryBuilder(GhostRoleBase role)
{
	private readonly GhostRoleBase role = role;

	public OptionFactory Build()
	{
		var factory = OptionManager.CreateAutoParentSetOptionCategory(
			ExtremeGhostRoleManager.GetRoleGroupId(this.role.Core.Id),
			this.role.Core.Name, this.role.Core.Tab, this.role.Core.Color);
		factory.Create0To100Percentage10StepOption(
			RoleCommonOption.SpawnRate,
			ignorePrefix: true);

		int spawnNum = this.role.Team.IsImpostor() ? GameSystem.MaxImposterNum : GameSystem.VanillaMaxPlayerNum - 1;

		factory.CreateIntOption(
			RoleCommonOption.RoleNum,
			1, 1, spawnNum, 1,
			ignorePrefix: true);

		factory.CreateIntOption(RoleCommonOption.AssignWeight, 500, 1, 1000, 1, ignorePrefix: true);

		return factory;
	}
}
