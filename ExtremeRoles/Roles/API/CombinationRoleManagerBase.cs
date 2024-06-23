using System;
using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.NewOption;
using ExtremeRoles.Module.NewOption.Factory;
using ExtremeRoles.Module.NewOption.Interfaces;

namespace ExtremeRoles.Roles.API;


public enum CombinationRoleCommonOption
{
    IsMultiAssign = 50,
    AssignsNum,
    IsAssignImposter,
    ImposterSelectedRate,
}

public abstract class CombinationRoleManagerBase : RoleOptionBase
{
	public List<MultiAssignRoleBase> Roles = new List<MultiAssignRoleBase>();

	protected readonly Color OptionColor;
	protected readonly string RoleName = "";
	protected readonly CombinationRoleType RoleType;

	public sealed override IOptionLoader Loader
	{
		get
		{
			if (!NewOptionManager.Instance.TryGetCategory(
					OptionTab.Combination,
					ExtremeRoleManager.GetCombRoleGroupId(this.RoleType),
					out var cate))
			{
				throw new ArgumentException("Can't find category");
			}
			return cate;
		}
	}

    internal CombinationRoleManagerBase(
		CombinationRoleType type,
        string roleName,
        Color optionColor)
    {
        this.OptionColor = optionColor;
        this.RoleName = roleName;
		this.RoleType = type;
    }

    public abstract string GetOptionName();
    public abstract void AssignSetUpInit(int curImpNum);

    public abstract MultiAssignRoleBase GetRole(
        int roleId, RoleTypes playerRoleType);

    protected sealed override void CreateKillerOption(
        AutoParentSetOptionCategoryFactory factory,
		IOption parent = null,
		bool ignorePrefix=true)
    {
        // 複数ロールの中に殺戮者がいる可能性がため、管理ロールで殺戮者の設定はしない
        return;
    }

    protected sealed override void CreateVisionOption(
        AutoParentSetOptionCategoryFactory factory, bool ignorePrefix)
    {
        // 複数のロールがまとまっているため、管理ロールで視界の設定はしない
        return;
    }
    protected sealed override void RoleSpecificInit()
    {
        // 複数のロールがまとまっているため、設定はしない
        return;
    }

}
