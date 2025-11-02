
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Factory.Old;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class SpecialImpostor : SingleRoleBase
{
    public SpecialImpostor(): base(
		RoleCore.BuildImpostor(ExtremeRoleId.SpecialImpostor),
        true, false, true, true)
    {}

    protected override void CreateSpecificOption(OldAutoParentSetOptionCategoryFactory factory)
    {
        return;
    }

    protected override void RoleSpecificInit()
    {
        return;
    }

}
