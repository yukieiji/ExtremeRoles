using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.NewOption;
using ExtremeRoles.Module.NewOption.Factory;

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

    public sealed override string GetOptionName()
        => Design.ColoedString(
            this.OptionColor,
            Translation.GetString(this.RoleName));

    public string GetBaseRoleFullDescription() =>
        Translation.GetString($"{BaseRole.Id}FullDescription");

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

            var spawnOption = cate.GetValueOption<CombinationRoleCommonOption, int>(
                CombinationRoleCommonOption.ImposterSelectedRate);
            isEvil = isEvil &&
                (UnityEngine.Random.RandomRange(0, 110) < (int)decimal.Multiply(
                    spawnOption.Value, spawnOption.Range)) &&
                curImpNum < GameOptionsManager.Instance.CurrentGameOptions.GetInt(
                    Int32OptionNames.NumImpostors);

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
		var factory = NewOptionManager.Instance.CreateAutoParentSetOptionCategory(
			ExtremeRoleManager.GetCombRoleGroupId(this.RoleType),
			this.RoleName,
			OptionTab.Combination,
			this.OptionColor);

		var roleSetOption = factory.CreateSelectionOption(
			RoleCommonOption.SpawnRate,
			OptionCreator.SpawnRate,
			ignorePrefix: true);

		int maxSetNum = this.BaseRole.IsImpostor() ?
			GameSystem.MaxImposterNum :
			(GameSystem.VanillaMaxPlayerNum - 1);

		var roleSetNumOption = factory.CreateIntOption(
			RoleCommonOption.RoleNum,
			1, 1, maxSetNum, 1,
			ignorePrefix: true);

		int roleAssignNum = this.BaseRole.IsImpostor() ?
			GameSystem.MaxImposterNum :
			GameSystem.VanillaMaxPlayerNum - 1;
		var roleAssignNumOption = factory.CreateIntOption(
			CombinationRoleCommonOption.AssignsNum,
			this.minimumRoleNum, this.minimumRoleNum,
			roleAssignNum, 1,
			isHidden: this.minimumRoleNum <= 1,
			ignorePrefix: true);

		factory.CreateBoolOption(
			CombinationRoleCommonOption.IsMultiAssign, false,
			ignorePrefix: true);

		roleAssignNumOption.AddWithUpdate(roleSetNumOption);

		factory.CreateIntOption(RoleCommonOption.AssignWeight,
			500, 1, 1000, 1, ignorePrefix: true);

        if (this.canAssignImposter)
        {
			var isImposterAssignOps = factory.CreateBoolOption(
				CombinationRoleCommonOption.IsAssignImposter,
				false, ignorePrefix: true);

			factory.CreateSelectionOption(
				CombinationRoleCommonOption.ImposterSelectedRate,
				OptionCreator.SpawnRate, isImposterAssignOps,
				ignorePrefix: true);
        }
        return factory;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        this.BaseRole.CreateRoleSpecificOption(
            factory);
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
