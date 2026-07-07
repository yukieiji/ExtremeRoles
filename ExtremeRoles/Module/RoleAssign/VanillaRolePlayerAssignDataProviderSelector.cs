using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class VanillaRolePlayerAssignDataProviderSelector(
	VanillaRolePlayerOption option,
	IServiceProvider provider) : IVanillaRolePlayerAssignDataProvider
{
	private readonly IServiceProvider provider = provider;

	public IEnumerable<VanillaRolePlayerAssignData> Data => option.MockOption is not null
		? this.provider.GetRequiredService<MockVanillaRolePlayerAssignDataProvider>().Data
		: this.provider.GetRequiredService<DefaultVanillaRolePlayerAssignDataProvider>().Data;
}
