using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API
{

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

        protected Color optionColor;
        protected string roleName = "";
        internal CombinationRoleManagerBase(
            string roleName,
            Color optionColor)
        {
            this.optionColor = optionColor;
            this.roleName = roleName;
        }

        public abstract void AssignSetUpInit();

        public abstract MultiAssignRoleBase GetRole(
            byte roleId, RoleTypes playerRoleType);

        protected override void CreateKillerOption(
            CustomOptionBase parentOps)
        {
            // 複数ロールの中に殺戮者がいる可能性がため、管理ロールで殺戮者の設定はしない
            return;
        }

        protected override void CreateVisonOption(
            CustomOptionBase parentOps)
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

        public override void AssignSetUpInit()
        {
            return;
        }

        public override MultiAssignRoleBase GetRole(
            byte roleId, RoleTypes playerRoleType)
        {

            foreach(var checkRole in this.Roles)
            {

                if (checkRole.BytedRoleId != roleId) { continue; }

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
                OptionHolder.SpawnRate, null, true);

            int thisMaxRoleNum =
                this.maxSetNum == int.MaxValue ? 
                (int)Math.Floor((decimal)OptionHolder.VanillaMaxPlayerNum / this.setPlayerNum) : this.maxSetNum;

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

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            IEnumerable<MultiAssignRoleBase> collection = Roles;

            foreach (var item in collection.Select(
                (Value, Index) => new { Value, Index }))
            {
                int optionOffset = this.OptionIdOffset + (
                    ExtremeRoleManager.OptionOffsetPerRole * (item.Index + 1));
                item.Value.ManagerOptionOffset = this.OptionIdOffset;
                item.Value.CreatRoleSpecificOption(
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

    public abstract class FlexibleCombinationRoleManagerBase : CombinationRoleManagerBase
    {

        private MultiAssignRoleBase baseRole;
        private int minimumRoleNum = 0;
        private bool canAssignImposter = true;

        public FlexibleCombinationRoleManagerBase(
            MultiAssignRoleBase role,
            int minimumRoleNum = 2,
            bool canAssignImposter = true) : 
                base(role.Id.ToString(), role.NameColor)
        {
            this.minimumRoleNum = minimumRoleNum;
            this.baseRole = role;
            this.canAssignImposter = canAssignImposter;
        }
        public override void AssignSetUpInit()
        {

            var allOptions = OptionHolder.AllOption;

            foreach (var role in this.Roles)
            {
                role.CanHasAnotherRole = allOptions[
                    GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign)].GetValue();

                if (!allOptions.ContainsKey(
                        GetRoleOptionId(
                            CombinationRoleCommonOption.IsAssignImposter))) { continue; }

                bool isEvil = allOptions[
                    GetRoleOptionId(CombinationRoleCommonOption.IsAssignImposter)].GetValue();

                var spawnOption = allOptions[
                        GetRoleOptionId(CombinationRoleCommonOption.ImposterSelectedRate)];
                isEvil = isEvil && (UnityEngine.Random.RandomRange(1, 100) < (int)Decimal.Multiply(
                    spawnOption.GetValue(), spawnOption.Selections.ToList().Count));

                if (isEvil)
                {
                    role.Team = ExtremeRoleType.Impostor;
                    role.NameColor = Palette.ImpostorRed;
                    role.CanKill = true;
                    role.UseVent = true;
                    role.UseSabotage = true;
                    role.HasTask = false;
                }
                else
                {
                    role.Team = ExtremeRoleType.Crewmate;
                    role.CanKill = false;
                    role.UseVent = false;
                    role.UseSabotage = false;
                    role.HasTask = true;
                }
                role.Initialize();
            }
        }

        public override MultiAssignRoleBase GetRole(
            byte roleId, RoleTypes playerRoleType)
        {

            MultiAssignRoleBase role = null;
            
            if (this.baseRole.BytedRoleId != roleId) { return role; }

            this.baseRole.CanHasAnotherRole = OptionHolder.AllOption[
                GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign)].GetValue();

            role = (MultiAssignRoleBase)this.baseRole.Clone();

            switch (playerRoleType)
            {
                case RoleTypes.Impostor:
                case RoleTypes.Shapeshifter:
                    role.Team = ExtremeRoleType.Impostor;
                    role.NameColor = Palette.ImpostorRed;
                    role.CanKill = true;
                    role.UseVent = true;
                    role.UseSabotage = true;
                    role.HasTask = false;
                    return role;
                default:
                    return role;
            }


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
                OptionHolder.SpawnRate, null, true);

            var roleAssignNumOption = CustomOption.Create(
                GetRoleOptionId(CombinationRoleCommonOption.AssignsNum),
                Design.ConcatString(
                    this.roleName,
                    CombinationRoleCommonOption.AssignsNum.ToString()),
                this.minimumRoleNum, this.minimumRoleNum,
                OptionHolder.VanillaMaxPlayerNum - 1, 1,
                roleSetOption, isHidden: this.minimumRoleNum <= 1);

            var roleSetNumOption = CustomOption.Create(
                GetRoleOptionId(RoleCommonOption.RoleNum),
                Design.ConcatString(
                    this.roleName,
                    RoleCommonOption.RoleNum.ToString()),
                1, 1, (OptionHolder.VanillaMaxPlayerNum - 1), 1,
                roleSetOption);

            roleAssignNumOption.SetUpdateOption(roleSetNumOption);

            if (this.canAssignImposter)
            {
                var isImposterAssignOps = CustomOption.Create(
                    GetRoleOptionId(CombinationRoleCommonOption.IsAssignImposter),
                    Design.ConcatString(
                        this.roleName,
                        CombinationRoleCommonOption.IsAssignImposter.ToString()),
                    false, roleSetOption);

                CustomOption.Create(
                    GetRoleOptionId(CombinationRoleCommonOption.ImposterSelectedRate),
                    Design.ConcatString(
                        this.roleName,
                        CombinationRoleCommonOption.ImposterSelectedRate.ToString()),
                    OptionHolder.SpawnRate, isImposterAssignOps);
            }

            CustomOption.Create(
                GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign),
                Design.ConcatString(
                    this.roleName,
                    CombinationRoleCommonOption.IsMultiAssign.ToString()),
                false, roleSetOption, isHidden: this.minimumRoleNum <= 1);

            return roleSetOption;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            int optionOffset = this.OptionIdOffset + ExtremeRoleManager.OptionOffsetPerRole;
            this.baseRole.ManagerOptionOffset = this.OptionIdOffset;
            this.baseRole.CreatRoleSpecificOption(
                parentOps,
                optionOffset);
        }

        protected override void CommonInit()
        {
            this.Roles.Clear();
            int roleAssignNum = 1;
            var allOptions = OptionHolder.AllOption;

            this.baseRole.CanHasAnotherRole = allOptions[
                GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign)].GetValue();

            if (allOptions.ContainsKey(GetRoleOptionId(CombinationRoleCommonOption.AssignsNum)))
            {
                roleAssignNum = allOptions[
                    GetRoleOptionId(CombinationRoleCommonOption.AssignsNum)].GetValue();
            }

            for (int i = 0; i < roleAssignNum; ++i)
            {
                this.Roles.Add((MultiAssignRoleBase)this.baseRole.Clone());
            }
        }

    }

}
