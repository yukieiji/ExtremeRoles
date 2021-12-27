using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API
{
    public abstract class CombinationRoleManagerBase : RoleOptionBase
    {

        public List<MultiAssignRoleBase> Roles = new List<MultiAssignRoleBase>();

        private int setPlayerNum = 0;
        private Color optionColor;

        private string roleName = "";

        public CombinationRoleManagerBase(
            string roleName,
            Color optionColor,
            int setPlayerNum)
        {
            this.optionColor = optionColor;
            this.setPlayerNum = setPlayerNum;
            this.roleName = roleName;
        }

        protected override CustomOptionBase CreateSpawnOption()
        {
            // ExtremeRolesPlugin.Instance.Log.LogInfo($"Color: {this.optionColor}");
            var roleSetOption = CustomOption.Create(
                GetRoleOptionId(RoleCommonOption.SpawnRate),
                Design.ColoedString(
                    this.optionColor,
                    Design.ConcatString(
                        this.roleName,
                        RoleCommonOption.SpawnRate.ToString())),
                OptionsHolder.SpawnRate, null, true);

            int thisMaxRoleNum = (int)Math.Floor((decimal)OptionsHolder.VanillaMaxPlayerNum / this.setPlayerNum);

            CustomOption.Create(
                GetRoleOptionId(RoleCommonOption.RoleNum),
                Design.ConcatString(
                    this.roleName,
                    RoleCommonOption.RoleNum.ToString()),
                1, 1, thisMaxRoleNum, 1,
                roleSetOption);
            CustomOption.Create(
                GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign),
                Design.ConcatString(
                    this.roleName,
                    CombinationRoleCommonOption.IsMultiAssign.ToString()),
                false, roleSetOption);

            return roleSetOption;
        }
        protected override void CreateKillerOption(
            CustomOptionBase parentOps)
        {
            // 複数ロールの中に殺戮者がいる可能性がため、管理ロールで殺戮者の設定はしない
            return;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            IEnumerable<SingleRoleBase> collection = Roles;

            foreach (var item in collection.Select(
                (Value, Index) => new { Value, Index }))
            {
                int optionOffset = this.OptionIdOffset + (
                    ExtremeRoleManager.OptionOffsetPerRole * (item.Index + 1));
                item.Value.CreatRoleSpecificOption(
                    parentOps,
                    optionOffset);
            }
        }

        protected override void CreateVisonOption(
            CustomOptionBase parentOps)
        {
            // 複数のロールがまとまっているため、管理ロールで視界の設定はしない
            return;
        }

        protected override void CommonInit()
        {
            foreach (var role in Roles)
            {
                role.CanHasAnotherRole = OptionsHolder.AllOptions[
                    GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign)].GetValue();
                role.GameInit();
            }
        }

    }
}
