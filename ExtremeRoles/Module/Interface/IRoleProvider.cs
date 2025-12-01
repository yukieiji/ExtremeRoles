using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.Interface;

public interface IRoleProvider
{
	public SingleRoleBase Get(ExtremeRoleId id);
}
