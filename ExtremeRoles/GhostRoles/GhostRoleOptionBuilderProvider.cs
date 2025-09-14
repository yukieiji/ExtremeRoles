using System;
using System.Collections.Generic;
using ExtremeRoles.GhostRoles.API.Interface;

namespace ExtremeRoles.GhostRoles;

public sealed class GhostRoleOptionBuilderProvider(
	IServiceProvider provider,
	IGhostRoleInfoContainer info) : IGhostRoleOptionBuilderProvider
{
	private readonly IReadOnlyDictionary<ExtremeGhostRoleId, Type> builder = info.OptionBuilder;
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
