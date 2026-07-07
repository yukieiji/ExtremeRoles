#nullable enable

namespace ExtremeRoles.Module.RoleAssign;

public sealed class VanillaRolePlayerOption
{
	public VanillaRolePlayerMockOption? MockOption { get; set; } = null;
}


public sealed record VanillaRolePlayerMockOption(int PlayerNum);