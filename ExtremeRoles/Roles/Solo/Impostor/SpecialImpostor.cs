
using ExtremeRoles.Roles.API;


using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class SpecialImpostor : SingleRoleBase
{
    public SpecialImpostor(): base(
		RoleCore.BuildImpostor(ExtremeRoleId.SpecialImpostor),
        true, false, true, true)
    {}

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
    {
        var factory = categoryScope.Builder;
        return;
    }

    protected override void RoleSpecificInit()
    {
        return;
    }

}
