using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Combination;

public sealed class SkaterManager : FlexibleCombinationRoleManagerBase
{
    public SkaterManager() : base(new Skater(), 1)
    { }

}

public sealed class Skater : MultiAssignRoleBase, IRoleSpecialSetUp
{
    public override string RoleName =>
        string.Concat(this.roleNamePrefix, this.RawRoleName);


    private string roleNamePrefix;

    public Skater(
        ) : base(
            ExtremeRoleId.Skater,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Skater.ToString(),
            ColorPalette.SupporterGreen,
            false, true, false, false,
            tab: OptionTab.Combination)
    {}

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        var imposterSetting = OptionManager.Instance.Get<bool>(
            GetManagerOptionId(CombinationRoleCommonOption.IsAssignImposter));
        CreateKillerOption(imposterSetting);
    }

    protected override void RoleSpecificInit()
    {
        this.roleNamePrefix = this.CreateImpCrewPrefix();
    }

	public void IntroBeginSetUp()
	{ }

	public void IntroEndSetUp()
	{
	}
}
