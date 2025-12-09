using TMPro;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;

using ExtremeRoles.GameMode.RoleSelector;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class Militant : SingleRoleBase
{
	public Militant(LiberalDefaultOptipnLoader option) : base(
		RoleCore.BuildLiberal(
			ExtremeRoleId.Militant,
			ColorPalette.LiberalColor),
		true, false,
		false, false)
	{

		LiberalSettingOverrider.OverrideDefault(this, option);

		this.HasOtherKillRange = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.MilitantHasOtherKillRange);
		if (this.HasOtherKillRange)
		{
			this.KillRange = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.MilitantKillRange);
		}

		this.HasOtherKillCool = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.MilitantHasOtherKillCool);
		if (this.HasOtherKillCool)
		{
			this.KillCoolTime = option.GetValue<LiberalGlobalSetting, float>(LiberalGlobalSetting.MilitantKillCool);
		}
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{

	}

	protected override void RoleSpecificInit()
	{

	}
}
