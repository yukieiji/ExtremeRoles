﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataChecker;


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
			.AddTransient<IRoleDependencyRuleFactory, RoleDependencyRuleFactory>();

		// EventManager
		collection.AddSingleton<IEventManager, Module.Event.EventManager>();

		return collection.BuildServiceProvider();
	}
}
