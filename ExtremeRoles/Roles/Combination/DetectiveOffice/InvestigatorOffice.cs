using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Combination.InvestigatorOffice;

public sealed class InvestigatorOfficeManager : ConstCombinationRoleManagerBase
{

    public const string Name = "DetectiveOffice";

    public InvestigatorOfficeManager() : base(
		CombinationRoleType.DetectiveOffice,
        Name, DefaultColor, 2,
        (GameSystem.VanillaMaxPlayerNum - 1) / 2)
    {
        Roles.Add(new Investigator());
        Roles.Add(new Assistant());
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        base.CreateSpecificOption(factory);
		factory.IdOffset = Roles.Count * ExtremeRoleManager.OptionOffsetPerRole;
		factory.OptionPrefix = ExtremeRoleId.DetectiveApprentice.ToString();
		InvestigatorApprentice.InvestigatorApprenticeOptionHolder.CreateOption(factory);
    }

}
