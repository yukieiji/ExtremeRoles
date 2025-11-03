using ExtremeRoles.Module;

using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Bakary : SingleRoleBase
{
    public enum BakaryOption
    {
        ChangeCooking,
        GoodBakeTime,
        BadBakeTime,
    }

    public Bakary() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Bakary,
			ColorPalette.BakaryWheatColor),
        false, true, false, false)
    { }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        var changeCooking = factory.CreateBoolOption(
            BakaryOption.ChangeCooking,
            true);
		var changeCookinkActive = new InvertActive(changeCooking);

        factory.CreateFloatOption(
            BakaryOption.GoodBakeTime,
            60.0f, 45.0f, 75.0f, 0.5f,
			changeCookinkActive, format: OptionUnit.Second);
        factory.CreateFloatOption(
            BakaryOption.BadBakeTime,
            120.0f, 105.0f, 135.0f, 0.5f,
			changeCookinkActive, format: OptionUnit.Second);
    }

    protected override void RoleSpecificInit()
    {
		var loader = this.Loader;

		ExtremeSystemTypeManager.Instance.TryAdd(
			ExtremeSystemType.BakeryReport,
			new BakerySystem(
				loader.GetValue<BakaryOption, float>(
					BakaryOption.GoodBakeTime),
				loader.GetValue<BakaryOption, float>(
					BakaryOption.BadBakeTime),
				loader.GetValue<BakaryOption, bool>(
					BakaryOption.ChangeCooking)));
	}
}
