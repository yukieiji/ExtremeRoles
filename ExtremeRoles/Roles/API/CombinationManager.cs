using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API
{
    public abstract class CombinationRoleManagerBase : RoleSettingBase
    {

        public List<MultiAssignRoleBase> Roles = new List<MultiAssignRoleBase>();

        private int setPlayerNum = 0;
        private Color settingColor;

        private string roleName = "";

        public CombinationRoleManagerBase(
            string roleName,
            Color settingColor,
            int setPlayerNum)
        {
            this.settingColor = settingColor;
            this.setPlayerNum = setPlayerNum;
            this.roleName = roleName;
        }

        protected override CustomOptionBase CreateSpawnOption()
        {
            // ExtremeRolesPlugin.Instance.Log.LogInfo($"Color: {this.SettingColor}");
            var roleSetOption = CustomOption.Create(
                GetRoleSettingId(RoleCommonSetting.SpawnRate),
                Design.ColoedString(
                    this.settingColor,
                    Design.ConcatString(
                        this.roleName,
                        RoleCommonSetting.SpawnRate.ToString())),
                OptionsHolder.SpawnRate, null, true);

            int thisMaxRoleNum = (int)Math.Floor((decimal)OptionsHolder.VanillaMaxPlayerNum / this.setPlayerNum);

            CustomOption.Create(
                GetRoleSettingId(RoleCommonSetting.RoleNum),
                Design.ConcatString(
                    this.roleName,
                    RoleCommonSetting.RoleNum.ToString()),
                1, 1, thisMaxRoleNum, 1,
                roleSetOption);
            CustomOption.Create(
                GetRoleSettingId(CombinationRoleCommonSetting.IsMultiAssign),
                Design.ConcatString(
                    this.roleName,
                    CombinationRoleCommonSetting.IsMultiAssign.ToString()),
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
                    GetRoleSettingId(CombinationRoleCommonSetting.IsMultiAssign)].GetValue();
                role.GameId = 0;
                role.GameInit();
            }
        }

    }
}
