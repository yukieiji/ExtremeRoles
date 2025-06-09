using System;

using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class ExtremeRoleAssginDataPreparer(IServiceProvider provider) : IRoleAssignDataPreparer
{
	private readonly IServiceProvider provider = provider;

	public PreparationData Prepare()
		=> new PreparationData(
			provider.GetRequiredService<PlayerRoleAssignData>(),
			provider.GetRequiredService<ISpawnDataManager>(),
			provider.GetRequiredService<ISpawnLimiter>());
}
