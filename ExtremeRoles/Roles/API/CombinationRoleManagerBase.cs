using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Roles.API;


public enum CombinationRoleCommonOption
{
    IsMultiAssign = 30,
    AssignsNum,
    IsAssignImposter,
    ImposterSelectedRate,
}

public abstract class CombinationRoleManagerBase : RoleOptionBase
{
    public List<MultiAssignRoleBase> Roles = new List<MultiAssignRoleBase>();

    protected Color OptionColor;
    protected string RoleName = "";
    internal CombinationRoleManagerBase(
        string roleName,
        Color optionColor)
    {
        this.OptionColor = optionColor;
        this.RoleName = roleName;
    }

    public abstract string GetOptionName();
    public abstract void AssignSetUpInit(int curImpNum);

    public abstract MultiAssignRoleBase GetRole(
        int roleId, RoleTypes playerRoleType);

    protected override void CreateKillerOption(
        IOptionInfo parentOps)
    {
        // 複数ロールの中に殺戮者がいる可能性がため、管理ロールで殺戮者の設定はしない
        return;
    }

    protected override void CreateVisionOption(
        IOptionInfo parentOps)
    {
        // 複数のロールがまとまっているため、管理ロールで視界の設定はしない
        return;
    }
    protected override void RoleSpecificInit()
    {
        // 複数のロールがまとまっているため、設定はしない
        return;
    }

}
