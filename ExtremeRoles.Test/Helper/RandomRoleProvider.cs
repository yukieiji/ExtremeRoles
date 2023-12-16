using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Test.Helper;

public static class RandomRoleProvider
{
	public static SingleRoleBase GetNormalRole()
		=> ExtremeRoleManager.NormalRole.Values.OrderBy(x => RandomGenerator.Instance.Next()).First();
	public static CombinationRoleManagerBase GetCombRole()
		=> ExtremeRoleManager.CombRole.Values.OrderBy(x => RandomGenerator.Instance.Next()).First();
}
