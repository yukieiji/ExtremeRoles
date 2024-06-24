using ExtremeRoles.Module;

using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API;


using ExtremeRoles.Module.NewOption.Factory;

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
        ExtremeRoleId.Bakary,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Bakary.ToString(),
        ColorPalette.BakaryWheatColor,
        false, true, false, false)
    { }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        var changeCooking = factory.CreateBoolOption(
            BakaryOption.ChangeCooking,
            true);

        factory.CreateFloatOption(
            BakaryOption.GoodBakeTime,
            60.0f, 45.0f, 75.0f, 0.5f,
            changeCooking, format: OptionUnit.Second,
            invert: true);
        factory.CreateFloatOption(
            BakaryOption.BadBakeTime,
            120.0f, 105.0f, 135.0f, 0.5f,
            changeCooking, format: OptionUnit.Second,
            invert: true);
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
