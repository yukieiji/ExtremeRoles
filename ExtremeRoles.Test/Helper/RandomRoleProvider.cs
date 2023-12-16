using System.Linq;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Test.Helper;

public static class RandomRoleProvider
{
	public static SingleRoleBase GetNormalRole()
		=> ExtremeRoleManager.NormalRole.Values.OrderBy(x => RandomGenerator.Instance.Next()).First();
	public static CombinationRoleManagerBase GetCombRole()
		=> ExtremeRoleManager.CombRole.Values.OrderBy(x => RandomGenerator.Instance.Next()).First();
}
