using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.GhostRoles;

public sealed class GhostRoleOptionValue<T>(IOptionLoader loader, T roleOption) where T : IOptionValue
{
	public float ButtonCoolTime { get; } = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime);
	public bool IsReportAbility { get; } = loader.GetValue<GhostRoleOption, bool>(GhostRoleOption.IsReportAbility);
	public T RoleOption { get; } = roleOption;
}
