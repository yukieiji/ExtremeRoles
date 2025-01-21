using AmongUs.GameOptions;

using ExtremeRoles.Helper;


using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Roles.API;

public abstract class FlexibleCombinationRoleManagerBase : CombinationRoleManagerBase
{

    public MultiAssignRoleBase BaseRole;
    private int minimumRoleNum = 0;
    private bool canAssignImposter = true;

    public FlexibleCombinationRoleManagerBase(
		CombinationRoleType roleType,
        MultiAssignRoleBase role,
        int minimumRoleNum = 2,
        bool canAssignImposter = true) :
            base(roleType, role.Id.ToString(), role.GetNameColor(true))
    {
        this.BaseRole = role;
        this.minimumRoleNum = minimumRoleNum;
        this.canAssignImposter = canAssignImposter;
    }

    public string GetBaseRoleFullDescription() =>
        Tr.GetString($"{BaseRole.Id}FullDescription");

    public override void AssignSetUpInit(int curImpNum)
    {
		var cate = this.Loader;

		foreach (var role in this.Roles)
        {
            role.CanHasAnotherRole = cate.GetValue<CombinationRoleCommonOption, bool>(
				CombinationRoleCommonOption.IsMultiAssign);

			if (!cate.TryGetValueOption<CombinationRoleCommonOption, bool>(
                   CombinationRoleCommonOption.IsAssignImposter,
                    out var impOpt)) { continue; }

            bool isEvil = impOpt.Value;

            int spawnOption = cate.GetValue<CombinationRoleCommonOption, int>(
                CombinationRoleCommonOption.ImposterSelectedRate);
			isEvil = isEvil && spawnOption >= RandomGenerator.Instance.Next(1, 101);

            if (isEvil)
            {
                role.Team = ExtremeRoleType.Impostor;
                role.SetNameColor(Palette.ImpostorRed);
                role.CanKill = true;
                role.UseVent = true;
                role.UseSabotage = true;
                role.HasTask = false;
                ++curImpNum;
            }
            else
            {
                role.Team = ExtremeRoleType.Crewmate;
                role.CanKill = false;
                role.UseVent = false;
                role.UseSabotage = false;
                role.HasTask = true;
            }
            role.Initialize();
        }
    }

    public override MultiAssignRoleBase GetRole(
        int roleId, RoleTypes playerRoleType)
    {

        MultiAssignRoleBase role = null;

        if (this.BaseRole.Id != (ExtremeRoleId)roleId) { return role; }

		this.BaseRole.CanHasAnotherRole = this.Loader.GetValue<CombinationRoleCommonOption, bool>(
			CombinationRoleCommonOption.IsMultiAssign);

		role = (MultiAssignRoleBase)this.BaseRole.Clone();

        switch (playerRoleType)
        {
            case RoleTypes.Impostor:
            case RoleTypes.Shapeshifter:
			case RoleTypes.Phantom:
                role.Team = ExtremeRoleType.Impostor;
                role.SetNameColor(Palette.ImpostorRed);
                role.CanKill = true;
                role.UseVent = true;
                role.UseSabotage = true;
                role.HasTask = false;
                return role;
            default:
                return role;
        }


    }

    protected override AutoParentSetOptionCategoryFactory CreateSpawnOption()
    {
		var factory = OptionManager.CreateAutoParentSetOptionCategory(
			ExtremeRoleManager.GetCombRoleGroupId(this.RoleType),
			this.RoleName,
			OptionTab.CombinationTab,
			this.OptionColor);

		var roleSetOption = factory.Create0To100Percentage10StepOption(
			RoleCommonOption.SpawnRate,
			ignorePrefix: true);

		int maxSetNum = this.BaseRole.IsImpostor() ?
			GameSystem.MaxImposterNum :
			(GameSystem.VanillaMaxPlayerNum - 1);

		var roleSetNumOption = factory.CreateIntOption(
			RoleCommonOption.RoleNum,
			1, 1, maxSetNum, 1,
			ignorePrefix: true);

		bool isHideMultiAssign = this.minimumRoleNum <= 1;
		int roleAssignNum = this.BaseRole.IsImpostor() ?
			GameSystem.MaxImposterNum :
			GameSystem.VanillaMaxPlayerNum - 1;
		var roleAssignNumOption = factory.CreateIntOption(
			CombinationRoleCommonOption.AssignsNum,
			this.minimumRoleNum, this.minimumRoleNum,
			roleAssignNum, 1,
			isHidden: isHideMultiAssign,
			ignorePrefix: true);

		factory.CreateBoolOption(
			CombinationRoleCommonOption.IsMultiAssign, false,
			isHidden: isHideMultiAssign,
			ignorePrefix: true);

		roleAssignNumOption.AddWithUpdate(roleSetNumOption);

		factory.CreateIntOption(RoleCommonOption.AssignWeight,
			500, 1, 1000, 1, ignorePrefix: true);

        if (this.canAssignImposter)
        {
			var assignRatioOption = factory.CreateBoolOption(
				CombinationRoleCommonOption.IsRatioTeamAssign,
				false, ignorePrefix: true);
			var isImposterAssignOps = factory.CreateBoolOption(
				CombinationRoleCommonOption.IsAssignImposter,
				false, assignRatioOption,
				ignorePrefix: true,
				invert: true);
			factory.CreateIntOption(
				CombinationRoleCommonOption.ImposterSelectedRate,
				10, 10, SingleRoleSpawnData.MaxSpawnRate, 10,
				isImposterAssignOps,
				format: OptionUnit.Percentage,
				ignorePrefix: true);

			factory.CreateSelectionOption(
				CombinationRoleCommonOption.AssignRatio,
				["1:1", "2:1", "1:2", "1:3", "3:1"],
				assignRatioOption,
				ignorePrefix: true);
        }
        return factory;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        this.BaseRole.CreateRoleSpecificOption(
            factory);
		this.BaseRole.OffsetInfo = new MultiAssignRoleBase.OptionOffsetInfo(
			this.RoleType, 0);
    }

    protected override void CommonInit()
    {
        this.Roles.Clear();
        int roleAssignNum = 1;

		var cate = this.Loader;

        this.BaseRole.CanHasAnotherRole = cate.GetValue<CombinationRoleCommonOption, bool>(
			CombinationRoleCommonOption.IsMultiAssign);

		if (cate.TryGetValueOption<CombinationRoleCommonOption, int>(
				CombinationRoleCommonOption.AssignsNum,
                out var opt))
        {
            roleAssignNum = opt.Value;
        }

        for (int i = 0; i < roleAssignNum; ++i)
        {
            this.Roles.Add((MultiAssignRoleBase)this.BaseRole.Clone());
        }
    }

}
