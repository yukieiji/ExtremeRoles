using System;
using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory.Old;

namespace ExtremeRoles.Roles.API;


public enum CombinationRoleCommonOption
{
    IsMultiAssign = 80,
    AssignsNum,
	IsRatioTeamAssign,
    IsAssignImposter,
    ImposterSelectedRate,
	AssignRatio
}

public abstract class CombinationRoleManagerBase : RoleOptionBase
{
	public List<MultiAssignRoleBase> Roles = new List<MultiAssignRoleBase>();

	protected readonly Color OptionColor;
	protected readonly string RoleName = "";
	protected readonly CombinationRoleType RoleType;

	protected static Color DefaultColor => new Color(255f, 255f, 255f);

	public sealed override IOptionLoader Loader
	{
		get
		{
			if (!OptionManager.Instance.TryGetCategory(
					OptionTab.CombinationTab,
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

    public string GetOptionName()
	{
		string name = Tr.GetString(this.RoleName);
		return this.OptionColor == DefaultColor ? name : Design.ColoredString(this.OptionColor, name);
	}

	public abstract void AssignSetUpInit(int curImpNum);

    public abstract MultiAssignRoleBase GetRole(
        int roleId, RoleTypes playerRoleType);

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
