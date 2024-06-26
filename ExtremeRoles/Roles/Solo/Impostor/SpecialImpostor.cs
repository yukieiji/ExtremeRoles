﻿
using ExtremeRoles.Roles.API;


using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class SpecialImpostor : SingleRoleBase
{
    public SpecialImpostor(): base(
        ExtremeRoleId.SpecialImpostor,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.SpecialImpostor.ToString(),
        Palette.ImpostorRed,
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
