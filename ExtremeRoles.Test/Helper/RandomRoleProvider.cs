using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Test.Helper;

public static class RandomRoleProvider
{
	public static IReadOnlySet<ExtremeRoleId> IgnoreRole = new HashSet<ExtremeRoleId>()
	{
		ExtremeRoleId.Monika
	};
	public static IReadOnlySet<CombinationRoleType> IgnoreCombRole = new HashSet<CombinationRoleType>()
	{
		CombinationRoleType.Avalon
	};

	public static SingleRoleBase GetNormalRole()
		=> ExtremeRoleManager.NormalRole.Values.OrderBy(
			x => IgnoreRole.Contains(x.Id) ? 
			int.MaxValue : RandomGenerator.Instance.Next()).First();
	public static byte GetCombRole()
		=> ExtremeRoleManager.CombRole.Keys.OrderBy(
			x => IgnoreCombRole.Contains((CombinationRoleType)x) ? 
				int.MaxValue : RandomGenerator.Instance.Next()).First();
}
