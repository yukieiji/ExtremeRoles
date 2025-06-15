using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;


namespace ExtremeRoles;

public partial class ExtremeRolesPlugin
{
	public static IServiceProvider BuildProvider()
	{
		var collection = new ServiceCollection();

		collection.AddTransient<IRoleAssignee, ExtremeRoleAssignee>();
		collection.AddTransient<IVanillaRoleProvider, VanillaRoleProvider>();
		collection.AddTransient<IRoleAssignDataBuilder, ExtremeRoleAssignDataBuilder>();
		collection.AddTransient<ISpawnLimiter, ExtremeSpawnLimiter>();
		collection.AddTransient<IRoleAssignDataPreparer, ExtremeRoleAssginDataPreparer>();
		collection.AddTransient<ISpawnDataManager, RoleSpawnDataManager>();

		collection.AddTransient<IRoleAssignDataBuildBehaviour, CombinationRoleAssignDataBuilder>();
		collection.AddTransient<IRoleAssignDataBuildBehaviour, SingleRoleAssignDataBuilder>();
		collection.AddTransient<IRoleAssignDataBuildBehaviour, NotAssignedPlayerAssignDataBuilder>();

		collection.AddTransient<PlayerRoleAssignData>();

		// 追加ここから
		// IAssignFilterInitializer とその実装を登録
		collection.AddTransient<IAssignFilterInitializer, AssignFilterInitializer>(); // AssignFilterInitializer の using が必要になる場合がある

		// IRoleAssignValidator とその実装を登録
		collection.AddTransient<IRoleAssignValidator, RoleAssignValidator>(); // RoleAssignValidator の using が必要になる場合がある

		return collection.BuildServiceProvider();
	}
}
