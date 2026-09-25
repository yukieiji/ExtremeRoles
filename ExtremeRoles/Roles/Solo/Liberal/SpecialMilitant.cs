using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class SpecialMilitant : SingleRoleBase
{
	public enum SpecialMilitantOption
	{
		UseVent = 0,
		KillMoney
	}

	public int KillMoney { get; private set; }

	public SpecialMilitant() : base(
		RoleArgs.BuildLiberalMilitant(ExtremeRoleId.SpecialMilitant))
	{
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(SpecialMilitantOption.UseVent, false);
		factory.CreateIntOption(SpecialMilitantOption.KillMoney, 10, 1, 1000, 1);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.UseVent = loader.GetValue<SpecialMilitantOption, bool>(SpecialMilitantOption.UseVent);
		this.KillMoney = loader.GetValue<SpecialMilitantOption, int>(SpecialMilitantOption.KillMoney);
	}
}
