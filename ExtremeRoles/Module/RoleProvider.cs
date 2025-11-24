using System;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Liberal;

namespace ExtremeRoles.Module;

public sealed class RoleProvider(IServiceProvider provider) : IRoleProvider
{
	private readonly IServiceProvider provider = provider;

	public SingleRoleBase Get(ExtremeRoleId id)
	{
		var type = id switch
		{ 
			ExtremeRoleId.Leader => typeof(Leader),
			ExtremeRoleId.Dove => typeof(Leader),
			ExtremeRoleId.Militant => typeof(Leader),
			_ => throw new NotSupportedException()
		};

		var role = provider.GetService(type) as SingleRoleBase;
		
		if (role is null)
		{
			throw new InvalidCastException();
		}

		return role;
	}
}
