#nullable enable

using System.Collections.Generic;

namespace ExtremeRoles.Module.RoleAssign;

public class VanillaRolePlayerOption
{
	public VanillaRolePlayerMockOption? MockOption { get; set; } = null;
}


public record VanillaRolePlayerMockOption(int PlayerNum, List<string>? MockPlayerNames);