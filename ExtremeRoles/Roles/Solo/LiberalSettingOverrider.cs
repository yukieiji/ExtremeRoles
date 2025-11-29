using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo;

public static class LiberalSettingOverrider
{
	public static void OverrideDefault(SingleRoleBase role, LiberalDefaultOptipnLoader option)
	{
		role.IsApplyEnvironmentVision = false;
		role.UseVent = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.UseVent);
		role.Vision = option.GetValue<LiberalGlobalSetting, float>(LiberalGlobalSetting.LiberalVison);
	}
}
