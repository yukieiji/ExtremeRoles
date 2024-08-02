using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;

using ExtremeRoles.Module.CustomOption.Factory;


namespace ExtremeRoles.Roles.API;

public abstract class ConstCombinationRoleManagerBase : CombinationRoleManagerBase
{
    private int setPlayerNum = 0;
    private int maxSetNum = int.MaxValue;
    private string optionKey => Design.ColoedString(
        this.OptionColor,
        string.Concat(
            this.RoleName,
            RoleCommonOption.SpawnRate.ToString()));

	public ConstCombinationRoleManagerBase(
		CombinationRoleType type,
		string roleName,
        Color optionColor,
        int setPlayerNum,
        int maxSetNum = int.MaxValue) : base (type, roleName, optionColor)
    {
        this.setPlayerNum = setPlayerNum;
        this.maxSetNum = maxSetNum;
    }

    public sealed override string GetOptionName()
        => OldTranslation.GetString(this.optionKey);

    public override void AssignSetUpInit(int curImpNum)
    {
        return;
    }

    public override MultiAssignRoleBase GetRole(
        int roleId, RoleTypes playerRoleType)
    {
		var cate = this.Loader;
		foreach (var checkRole in this.Roles)
        {

            if (checkRole.Id != (ExtremeRoleId)roleId) { continue; }

            checkRole.CanHasAnotherRole = cate.GetValue<CombinationRoleCommonOption, bool>(
				CombinationRoleCommonOption.IsMultiAssign);

			switch (checkRole.Team)
            {
                case ExtremeRoleType.Crewmate:
                case ExtremeRoleType.Neutral:
                    if (playerRoleType == RoleTypes.Crewmate)
                    {
                        return checkRole;
                    }
                    break;
                case ExtremeRoleType.Impostor:
                    if ((playerRoleType == RoleTypes.Impostor) ||
                        (playerRoleType == RoleTypes.Shapeshifter))
                    {
                        return checkRole;
                    }
                    break;
                default:
                    break;
            }
        }

        return null;

    }

    protected override AutoParentSetOptionCategoryFactory CreateSpawnOption()
    {
		// ExtremeRolesPlugin.Instance.Log.LogInfo($"Color: {this.optionColor}");

		var factory = OptionManager.CreateAutoParentSetOptionCategory(
			ExtremeRoleManager.GetCombRoleGroupId(this.RoleType),
			this.RoleName,
			OptionTab.CombinationTab,
			this.OptionColor == DefaultColor ? null : this.OptionColor);

        var roleSetOption = factory.Create0To100Percentage10StepOption(
			RoleCommonOption.SpawnRate,
			ignorePrefix: true);

		int thisMaxRoleNum =
            this.maxSetNum == int.MaxValue ?
            (int)Math.Floor((decimal)GameSystem.VanillaMaxPlayerNum / this.setPlayerNum) : this.maxSetNum;

		factory.CreateIntOption(
			RoleCommonOption.RoleNum,
            1, 1, thisMaxRoleNum, 1,
			ignorePrefix: true);

		factory.CreateBoolOption(
			CombinationRoleCommonOption.IsMultiAssign, false,
			ignorePrefix: true);

		factory.CreateIntOption(RoleCommonOption.AssignWeight,
			500, 1, 1000, 1, ignorePrefix: true);

        return factory;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IEnumerable<MultiAssignRoleBase> collection = Roles;

        foreach (var item in collection.Select(
            (Value, Index) => new { Value, Index }))
        {
			var role = item.Value;
			// 同じオプションを参照したい時があるのでオフセット値を入れておく
			int offset = (item.Index + 1) * ExtremeRoleManager.OptionOffsetPerRole;

			factory.IdOffset = (item.Index + 1) * ExtremeRoleManager.OptionOffsetPerRole;
			factory.OptionPrefix = role.RawRoleName;

			role.CreateRoleSpecificOption(factory, false);
			role.OffsetInfo = new MultiAssignRoleBase.OptionOffsetInfo(
				this.RoleType, offset);
        }
    }

    protected override void CommonInit()
    {
		var cate = this.Loader;
        foreach (var role in this.Roles)
        {
            role.CanHasAnotherRole =
				cate.GetValue<CombinationRoleCommonOption, bool>(
					CombinationRoleCommonOption.IsMultiAssign);
            role.Initialize();
        }
    }
}
