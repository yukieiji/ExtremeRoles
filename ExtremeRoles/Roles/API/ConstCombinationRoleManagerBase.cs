using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;

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
        string roleName,
        Color optionColor,
        int setPlayerNum,
        int maxSetNum = int.MaxValue) : base (roleName, optionColor)
    {
        this.setPlayerNum = setPlayerNum;
        this.maxSetNum = maxSetNum;
    }

    public sealed override string GetOptionName()
        => Translation.GetString(this.optionKey);

    public override void AssignSetUpInit(int curImpNum)
    {
        return;
    }

    public override MultiAssignRoleBase GetRole(
        int roleId, RoleTypes playerRoleType)
    {

        foreach(var checkRole in this.Roles)
        {

            if (checkRole.Id != (ExtremeRoleId)roleId) { continue; }

            checkRole.CanHasAnotherRole = AllOptionHolder.Instance.GetValue<bool>(
                GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign));

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

    protected override IOptionInfo CreateSpawnOption()
    {
        // ExtremeRolesPlugin.Instance.Log.LogInfo($"Color: {this.optionColor}");
        var roleSetOption = new SelectionCustomOption(
            GetRoleOptionId(RoleCommonOption.SpawnRate),
            this.optionKey,
            AllOptionCreator.SpawnRate, null, true,
            tab: OptionTab.Combination);

        int thisMaxRoleNum =
            this.maxSetNum == int.MaxValue ? 
            (int)Math.Floor((decimal)GameSystem.VanillaMaxPlayerNum / this.setPlayerNum) : this.maxSetNum;

        new IntCustomOption(
            GetRoleOptionId(RoleCommonOption.RoleNum),
            string.Concat(
                this.RoleName,
                RoleCommonOption.RoleNum.ToString()),
            1, 1, thisMaxRoleNum, 1,
            roleSetOption,
            tab: OptionTab.Combination);
        new BoolCustomOption(
            GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign),
            string.Concat(
                this.RoleName,
                CombinationRoleCommonOption.IsMultiAssign.ToString()),
            false, roleSetOption,
            tab: OptionTab.Combination);

        return roleSetOption;
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        IEnumerable<MultiAssignRoleBase> collection = Roles;

        foreach (var item in collection.Select(
            (Value, Index) => new { Value, Index }))
        {
            int optionOffset = this.OptionIdOffset + (
                ExtremeRoleManager.OptionOffsetPerRole * (item.Index + 1));
            item.Value.SetManagerOptionOffset(this.OptionIdOffset);
            item.Value.CreateRoleSpecificOption(
                parentOps,
                optionOffset);
        }
    }

    protected override void CommonInit()
    {
        foreach (var role in this.Roles)
        {
            role.CanHasAnotherRole = AllOptionHolder.Instance.GetValue<bool>(
                GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign));
            role.Initialize();
        }
    }
}
