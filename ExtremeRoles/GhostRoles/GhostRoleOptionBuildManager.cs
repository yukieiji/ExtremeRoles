using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles.API;
using Microsoft.Extensions.DependencyInjection;
using System;
using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory;

namespace ExtremeRoles.GhostRoles;

public sealed class GhostRoleOptionBuildManager(
	IGhostRoleCoreProvider coreProvider, 
	IGhostRoleOptionBuilderProvider buildProvider) : IGhostRoleOptionBuildManager
{
	private readonly IGhostRoleCoreProvider coreProvider = coreProvider;
	private readonly IGhostRoleOptionBuilderProvider builderProvider = buildProvider;
	public void Build()
	{
		foreach (var (id, core) in coreProvider.All)
		{
			using (var factory = getFactory(core))
			{
				var builder = builderProvider.Get(id);
				builder.Build(factory);
#if DEBUG
				if (!factory.Contain((int)GhostRoleOption.IsReportAbility))
				{
					throw new ArgumentException("Can't create [GhostRoleOption.IsReportAbility] option!!");
				}
#endif
			}
		}
	}

	private static OptionFactory getFactory(GhostRoleCore core)
	{
		var factory = OptionManager.CreateAutoParentSetOptionCategory(
			ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IRoleParentOptionIdGenerator>().Get(core.Id),
			core.Name, core.Tab, core.Color);
		factory.Create0To100Percentage10StepOption(
			RoleCommonOption.SpawnRate,
			ignorePrefix: true);

		int spawnNum = core.DefaultTeam is ExtremeRoleType.Impostor ? GameSystem.MaxImposterNum : GameSystem.VanillaMaxPlayerNum - 1;

		factory.CreateIntOption(
			RoleCommonOption.RoleNum,
			1, 1, spawnNum, 1,
			ignorePrefix: true);

		factory.CreateIntOption(RoleCommonOption.AssignWeight, 500, 1, 1000, 1, ignorePrefix: true);

		return factory;
	}
}
