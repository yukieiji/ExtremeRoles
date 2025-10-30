
using ExtremeRoles.Roles.API;


using ExtremeRoles.Module.CustomOption.Factory;

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
