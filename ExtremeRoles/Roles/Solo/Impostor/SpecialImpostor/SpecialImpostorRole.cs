
using ExtremeRoles.Roles.API;


using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Impostor.SpecialImpostor;

public sealed class SpecialImpostorRole : SingleRoleBase
{
    public SpecialImpostor(): base(
		RoleCore.BuildImpostor(ExtremeRoleId.SpecialImpostor),
        true, false, true, true)
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
