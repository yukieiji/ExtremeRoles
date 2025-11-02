using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory.Old;
using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Roles.API;

public sealed class ImpostorRatio
{
	public enum Ratio : byte
	{
		OneToOne,
		TwoToOne,
		OneToTwo,
		TwoToTwo,
		OneToThree,
		ThreeToOne,
		ThreeToTwo,
		TwoToThree,
		ThreeToThree,
	}

	public int CrewmateNum { get; }
	public int ImpostorNum { get; }
	public int TotalNum => CrewmateNum + ImpostorNum;

	public ImpostorRatio(Ratio ratio)
	{
		switch (ratio)
		{
			case Ratio.OneToOne:
				this.CrewmateNum = 1;
				this.ImpostorNum = 1;
				break;
			case Ratio.TwoToOne:
				this.CrewmateNum = 2;
				this.ImpostorNum = 1;
				break;
			case Ratio.OneToTwo:
				this.CrewmateNum = 1;
				this.ImpostorNum = 2;
				break;
			case Ratio.TwoToTwo:
				this.CrewmateNum = 2;
				this.ImpostorNum = 2;
				break;
			case Ratio.OneToThree:
				this.CrewmateNum = 1;
				this.ImpostorNum = 3;
				break;
			case Ratio.ThreeToOne:
				this.CrewmateNum = 3;
				this.ImpostorNum = 1;
				break;
			case Ratio.ThreeToTwo:
				this.CrewmateNum = 3;
				this.ImpostorNum = 2;
				break;
			case Ratio.TwoToThree:
				this.CrewmateNum = 2;
				this.ImpostorNum = 3;
				break;
			case Ratio.ThreeToThree:
				this.CrewmateNum = 3;
				this.ImpostorNum = 3;
				break;
		}
	}

	public ImpostorRatio(int option) : this((Ratio)option)
	{
	}
}

public abstract class FlexibleCombinationRoleManagerBase : CombinationRoleManagerBase
{

    public MultiAssignRoleBase BaseRole { get; }
    private readonly int minimumRoleNum = 0;
    private readonly bool canAssignImposter = true;

    public FlexibleCombinationRoleManagerBase(
		CombinationRoleType roleType,
        MultiAssignRoleBase role,
        int minimumRoleNum = 2,
        bool canAssignImposter = true) :
            base(roleType, role.Core.Id.ToString(), role.GetNameColor(true))
    {
        this.BaseRole = role;
        this.minimumRoleNum = minimumRoleNum;
        this.canAssignImposter = canAssignImposter;
    }

    public string GetBaseRoleFullDescription() =>
        Tr.GetString($"{BaseRole.Core.Id}FullDescription");

    public override void AssignSetUpInit(int curImpNum)
    {
		var cate = this.Loader;
		bool isMultiAssign = cate.GetValue<CombinationRoleCommonOption, bool>(
			CombinationRoleCommonOption.IsMultiAssign);
		bool isImposterAssign = cate.TryGetValueOption<CombinationRoleCommonOption, bool>(
			CombinationRoleCommonOption.IsAssignImposter,
			out var impOpt);
		bool isRatioAssign = cate.TryGetValueOption<CombinationRoleCommonOption, bool>(
			CombinationRoleCommonOption.IsRatioTeamAssign, out var ratioOpt) && ratioOpt.Value;

		foreach (var role in this.Roles)
        {
            role.CanHasAnotherRole = isMultiAssign;
			if (!isImposterAssign || isRatioAssign)
			{
				role.Initialize();
				continue;
			}

            bool isEvil = impOpt.Value;

            int spawnOption = cate.GetValue<CombinationRoleCommonOption, int>(
                CombinationRoleCommonOption.ImposterSelectedRate);
			isEvil = isEvil && spawnOption >= RandomGenerator.Instance.Next(1, 101);

            if (isEvil)
            {
				roleToImpostor(role);
				++curImpNum;
            }
            else
            {
				roleToCrewmate(role);
            }
            role.Initialize();
        }
    }

    public override MultiAssignRoleBase GetRole(
        int roleId, RoleTypes playerRoleType)
    {
        if (this.BaseRole.Core.Id != (ExtremeRoleId)roleId)
		{
			return null;
		}

		this.BaseRole.CanHasAnotherRole = this.Loader.GetValue<CombinationRoleCommonOption, bool>(
			CombinationRoleCommonOption.IsMultiAssign);

		MultiAssignRoleBase role = (MultiAssignRoleBase)this.BaseRole.Clone();

		return 
			VanillaRoleProvider.IsImpostorRole(playerRoleType) ? 
			roleToImpostor(role) : role;
    }

    protected override OldAutoParentSetOptionCategoryFactory CreateSpawnOption()
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
			ignorePrefix: true,
			isHidden: this.RoleType is CombinationRoleType.Traitor);

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

			factory.CreateSelectionOption<CombinationRoleCommonOption, ImpostorRatio.Ratio>(
				CombinationRoleCommonOption.AssignRatio,
				assignRatioOption,
				ignorePrefix: true);
        }
        return factory;
    }

    protected override void CreateSpecificOption(
        OldAutoParentSetOptionCategoryFactory factory)
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

		if (cate.TryGetValueOption<CombinationRoleCommonOption, bool>(
				CombinationRoleCommonOption.IsRatioTeamAssign, out var enableRatioOpt) &&
			enableRatioOpt.Value)
		{
			int selection = cate.GetValue<CombinationRoleCommonOption, int>(
				CombinationRoleCommonOption.AssignRatio);
			var ratio = new ImpostorRatio(selection);
			roleAssignNum = ratio.TotalNum;

			for (int i = 0; i < roleAssignNum; ++i)
			{
				var role = (MultiAssignRoleBase)this.BaseRole.Clone();
				
				role = i < ratio.CrewmateNum ? roleToCrewmate(role) : roleToImpostor(role);

				this.Roles.Add(role);
			}

		}
		else
		{
			for (int i = 0; i < roleAssignNum; ++i)
			{
				this.Roles.Add((MultiAssignRoleBase)this.BaseRole.Clone());
			}
		}
    }

	private static MultiAssignRoleBase roleToImpostor(MultiAssignRoleBase role)
	{
		role.Core.Team = ExtremeRoleType.Impostor;
		role.Core.Color = Palette.ImpostorRed;
		role.CanKill = true;
		role.UseVent = true;
		role.UseSabotage = true;
		role.HasTask = false;
		return role;
	}

	private static MultiAssignRoleBase roleToCrewmate(MultiAssignRoleBase role)
	{
		role.Core.Team = ExtremeRoleType.Crewmate;
		role.CanKill = false;
		role.UseVent = false;
		role.UseSabotage = false;
		role.HasTask = true;
		return role;
	}
}
