using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using System;
using System.Collections.Generic;

namespace ExtremeRoles.GhostRoles;

public sealed class GhostRoleProvider(
	IServiceProvider provider,
	IGhostRoleInfoContainer info) : IGhostRoleProvider
{
	private readonly IServiceProvider provider = provider;
	private readonly IReadOnlyDictionary<ExtremeGhostRoleId, Type> role = info.Role; 
	public GhostRoleBase Get(ExtremeGhostRoleId id)
	{
		var type = this.role[id];
		var role = provider.GetService(type) as GhostRoleBase;
		if (role is null)
		{
			throw new ArgumentException();
		}
		return role;
	}
}
