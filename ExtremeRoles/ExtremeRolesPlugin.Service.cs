using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataChecker;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.Solo.Liberal;
using Microsoft.Extensions.DependencyInjection;
using System;


namespace ExtremeRoles;

public partial class ExtremeRolesPlugin
{
	public static IServiceProvider BuildProvider()
	{
		var collection = new ServiceCollection();

		collection
			.AddTransient<IRoleAssignee, ExtremeRoleAssignee>()
			.AddTransient<IVanillaRoleProvider, VanillaRoleProvider>()
			.AddTransient<ISpawnLimiter, ExtremeSpawnLimiter>()
			.AddTransient<IRoleAssignDataPreparer, ExtremeRoleAssginDataPreparer>()
			.AddTransient<ISpawnDataManager, RoleSpawnDataManager>();


		collection
			.AddTransient<IRoleAssignDataBuilder, ExtremeRoleAssignDataBuilder>()
			.AddTransient<IRoleAssignDataBuildBehaviour, CombinationRoleAssignDataBuilder>()
			.AddTransient<IRoleAssignDataBuildBehaviour, SingleRoleAssignDataBuilder>()
			.AddTransient<IRoleAssignDataBuildBehaviour, NotAssignedPlayerAssignDataBuilder>();

		collection.AddTransient<PlayerRoleAssignData>();

		// 追加ここから
		// IAssignFilterInitializer とその実装を登録
		collection.AddTransient<IAssignFilterInitializer, AssignFilterInitializer>();

		// IRoleAssignValidator とその実装を登録
		collection
			.AddTransient<IRoleAssignValidator, RoleAssignValidator>()

			.AddTransient<IRoleAssignDataChecker, RoleAssignDependencyChecker>()
			.AddTransient<IRoleDependencyRuleFactory, RoleDependencyRuleFactory>()

			.AddTransient<IRoleProvider, RoleProvider>();

		// Liberal
		collection
			.AddSingleton<LiberalDefaultOptipnLoader>()
			.AddTransient(
				x => ExtremeSystemTypeManager.Instance.CreateOrGet(ExtremeSystemType.LiberalMoneyBank, () =>
				{
					var option = x.GetRequiredService<LiberalDefaultOptipnLoader>();
					return new LiberalMoneyBankSystem(option);
				})
			)
			.AddTransient<LeaderAbilityHandler>()
			.AddTransient<Leader>()
			.AddTransient<Dove>()
			.AddTransient<Militant>();

		collection.AddTransient<ExtremeGameEndChecker>();

		// EventManager
		collection.AddSingleton<IEventManager, Module.Event.EventManager>();

		return collection.BuildServiceProvider();
	}
}
