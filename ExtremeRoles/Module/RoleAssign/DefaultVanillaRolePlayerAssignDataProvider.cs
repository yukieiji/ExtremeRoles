using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class DefaultVanillaRolePlayerAssignDataProvider : IVanillaRolePlayerAssignDataProvider
{
	public IEnumerable<VanillaRolePlayerAssignData> Data => GameData.Instance.AllPlayers.GetFastEnumerator().Select(
			x => new VanillaRolePlayerAssignData(x));
}
