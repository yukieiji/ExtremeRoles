using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API
{
    public abstract class ConstCombinationRoleManagerBase : CombinationRoleManagerBase
    {
        private int setPlayerNum = 0;
        private int maxSetNum = int.MaxValue;

        public ConstCombinationRoleManagerBase(
            string roleName,
            Color optionColor,
            int setPlayerNum,
            int maxSetNum = int.MaxValue) : base (roleName, optionColor)
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

            foreach(var checkRole in this.Roles)
            {

                if (checkRole.Id != (ExtremeRoleId)roleId) { continue; }

                checkRole.CanHasAnotherRole = OptionHolder.AllOption[
                    GetRoleOptionId(
                        CombinationRoleCommonOption.IsMultiAssign)].GetValue();

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

        protected override IOption CreateSpawnOption()
        {
            // ExtremeRolesPlugin.Instance.Log.LogInfo($"Color: {this.optionColor}");
            var roleSetOption = new SelectionCustomOption(
                GetRoleOptionId(RoleCommonOption.SpawnRate),
                Design.ColoedString(
                    this.optionColor,
                    string.Concat(
                        this.roleName,
                        RoleCommonOption.SpawnRate.ToString())),
                OptionHolder.SpawnRate, null, true,
                tab: OptionTab.Combination);

            int thisMaxRoleNum =
                this.maxSetNum == int.MaxValue ? 
                (int)Math.Floor((decimal)OptionHolder.VanillaMaxPlayerNum / this.setPlayerNum) : this.maxSetNum;

            new IntCustomOption(
                GetRoleOptionId(RoleCommonOption.RoleNum),
                string.Concat(
                    this.roleName,
                    RoleCommonOption.RoleNum.ToString()),
                1, 1, thisMaxRoleNum, 1,
                roleSetOption,
                tab: OptionTab.Combination);
            new BoolCustomOption(
                GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign),
                string.Concat(
                    this.roleName,
                    CombinationRoleCommonOption.IsMultiAssign.ToString()),
                false, roleSetOption,
                tab: OptionTab.Combination);

            return roleSetOption;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
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
                role.CanHasAnotherRole = OptionHolder.AllOption[
                    GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign)].GetValue();
                role.Initialize();
            }
        }
    }
}
