
using ExtremeRoles.Roles.API;
using ExtremeRoles.Core.Service.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class SpecialImpostor : SingleRoleBase
{
    public SpecialImpostor(): base(
		RoleArgs.BuildImpostor(ExtremeRoleId.SpecialImpostor))
    {}

    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
    {
        return;
    }

    protected override void RoleSpecificInit()
    {
        return;
    }

}
