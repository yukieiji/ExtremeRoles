using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Roles.API;
using Microsoft.Extensions.DependencyInjection;

namespace ExtremeRoles.Roles.Combination.InvestigatorOffice;

public sealed class InvestigatorOfficeManager : ConstCombinationRoleManagerBase
{

    public const string Name = "InvestigatorOffice";

    public InvestigatorOfficeManager() : base(
		CombinationRoleType.InvestigatorOffice,
        Name, DefaultColor, 2,
        (GameSystem.VanillaMaxPlayerNum - 1) / 2)
    {
        Roles.Add(new Investigator());
        Roles.Add(new Assistant());
    }

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
	{
        base.CreateSpecificOption(categoryScope);

		var innerCategoryBuilder = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<AutoRoleOptionCategoryFactory>();
		var inner = innerCategoryBuilder.CreateInnnerRoleCategory(ExtremeRoleId.InvestigatorApprentice, categoryScope);

		InvestigatorApprentice.InvestigatorApprenticeOptionHolder.CreateOption(inner.Builder);
    }

}
