using System.Collections.Generic;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataChecker;

namespace ExtremeRoles.Module.Interface;

public interface IRoleDependencyRuleFactory
{
	public IReadOnlyList<RoleDependencyRule> Rules { get; }
}
