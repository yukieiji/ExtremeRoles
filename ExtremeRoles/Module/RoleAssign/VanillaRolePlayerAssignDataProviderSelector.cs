using System;
using System.Collections.Generic;
using ExtremeRoles.Module.Interface;
using Microsoft.Extensions.DependencyInjection;

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
