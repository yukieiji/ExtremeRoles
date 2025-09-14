using System;
using ExtremeRoles.GhostRoles.API;
using System.Collections.Generic;

namespace ExtremeRoles.GhostRoles;

public sealed class GhostRoleOptionBuilderProvider(
	IServiceProvider provider,
	IReadOnlyDictionary<ExtremeGhostRoleId, Type> builder) : IGhostRoleOptionBuilderProvider
{
	private readonly IReadOnlyDictionary<ExtremeGhostRoleId, Type> builder = builder;
	public IGhostRoleOptionBuilder Get(ExtremeGhostRoleId id)
	{
		var type = this.builder[id];
		var builder = provider.GetService(type) as IGhostRoleOptionBuilder;
		if (builder is null)
		{
			throw new ArgumentException();
		}
		return builder;
	}
}
