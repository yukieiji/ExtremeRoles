using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Roles.API;

public abstract class ConstCombinationRoleManagerBase : CombinationRoleManagerBase
{
    private int setPlayerNum = 0;
    private int maxSetNum = int.MaxValue;

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
			var core = checkRole.Core;
            if (core.Id != (ExtremeRoleId)roleId)
			{
				continue;
			}

            checkRole.CanHasAnotherRole = cate.GetValue<CombinationRoleCommonOption, bool>(
				CombinationRoleCommonOption.IsMultiAssign);

			switch (core.Team)
            {
                case ExtremeRoleType.Crewmate:
					if (VanillaRoleProvider.IsCrewmateRole(playerRoleType))
					{
						return checkRole;
					}
					break;
                case ExtremeRoleType.Neutral:
                    if (VanillaRoleProvider.IsDefaultCrewmateRole(playerRoleType))
                    {
                        return checkRole;
                    }
                    break;
                case ExtremeRoleType.Impostor:
                    if (VanillaRoleProvider.IsImpostorRole(playerRoleType))
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

    protected override OptionCategoryScope<AutoParentSetBuilder> CreateSpawnOption(AutoRoleOptionCategoryFactory factory)
    {
		// ExtremeRolesPlugin.Instance.Log.LogInfo($"Color: {this.optionColor}");

		var cate = factory.CreateRoleCategory(
			this.RoleType,
			this.RoleName,
			OptionTab.CombinationTab,
			this.OptionColor == DefaultColor ? null : this.OptionColor);

		var builder = cate.Builder;
        var roleSetOption = builder.Create0To100Percentage10StepOption(
			RoleCommonOption.SpawnRate,
			ignorePrefix: true);

		int thisMaxRoleNum =
            this.maxSetNum == int.MaxValue ?
            (int)Math.Floor((decimal)GameSystem.VanillaMaxPlayerNum / this.setPlayerNum) : this.maxSetNum;

		builder.CreateIntOption(
			RoleCommonOption.RoleNum,
            1, 1, thisMaxRoleNum, 1,
			ignorePrefix: true);

		builder.CreateBoolOption(
			CombinationRoleCommonOption.IsMultiAssign, false,
			ignorePrefix: true);

		builder.CreateIntOption(RoleCommonOption.AssignWeight,
			500, 1, 1000, 1, ignorePrefix: true);

        return cate;
    }

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
    {
        IEnumerable<MultiAssignRoleBase> collection = Roles;

        foreach (var item in collection.Select(
            (Value, Index) => new { Value, Index }))
        {
			var role = item.Value;
			var innerCategoryBuilder = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<AutoRoleOptionCategoryFactory>();
			var inner = innerCategoryBuilder.CreateInnnerRoleCategory(role.Core.Id, categoryScope);

			role.CreateRoleSpecificOption(categoryScope, false);
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
