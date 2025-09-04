using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Combination.DetectiveOffice;

public sealed class DetectiveOfficeManager : ConstCombinationRoleManagerBase
{

    public const string Name = "DetectiveOffice";

    public DetectiveOfficeManager() : base(
		CombinationRoleType.DetectiveOffice,
        Name, DefaultColor, 2,
        (GameSystem.VanillaMaxPlayerNum - 1) / 2)
    {
        Roles.Add(new Detective());
        Roles.Add(new Assistant());
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        base.CreateSpecificOption(factory);
		factory.IdOffset = Roles.Count * ExtremeRoleManager.OptionOffsetPerRole;
		factory.OptionPrefix = ExtremeRoleId.DetectiveApprentice.ToString();
		DetectiveApprentice.DetectiveApprenticeOptionHolder.CreateOption(factory);
    }

}
