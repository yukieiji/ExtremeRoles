using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public enum SpecialMilitantOption
{
	UseVent = 0,
}

public sealed class SpecialMilitant : SingleRoleBase
{
	public SpecialMilitant() : base(
		RoleArgs.BuildLiberalMilitant(ExtremeRoleId.SpecialMilitant))
	{
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(SpecialMilitantOption.UseVent, false);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.UseVent = loader.GetValue<SpecialMilitantOption, bool>(SpecialMilitantOption.UseVent);

		this.HasOtherKillCool = loader.GetValue<KillerCommonOption, bool>(
			KillerCommonOption.HasOtherKillCool);
		if (this.HasOtherKillCool)
		{
			this.KillCoolTime = loader.GetValue<KillerCommonOption, float>(
				KillerCommonOption.KillCoolDown);
		}

		this.HasOtherKillRange = loader.GetValue<KillerCommonOption, bool>(
			KillerCommonOption.HasOtherKillRange);
		if (this.HasOtherKillRange)
		{
			this.KillRange = loader.GetValue<KillerCommonOption, int>(
				KillerCommonOption.KillRange);
		}
	}
}
