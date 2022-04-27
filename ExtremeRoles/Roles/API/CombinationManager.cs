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

        public abstract void AssignSetUpInit(int curImpNum);

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

        public override void AssignSetUpInit(int curImpNum)
        {
            return;
        }

        public override MultiAssignRoleBase GetRole(
            byte roleId, RoleTypes playerRoleType)
        {

            foreach(var checkRole in this.Roles)
            {

                if ((byte)checkRole.Id != roleId) { continue; }

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
            var roleSetOption = new SelectionCustomOption(
                GetRoleOptionId(RoleCommonOption.SpawnRate),
                Design.ColoedString(
                    this.optionColor,
                    string.Concat(
                        this.roleName,
                        RoleCommonOption.SpawnRate.ToString())),
                OptionHolder.SpawnRate, null, true);

            int thisMaxRoleNum =
                this.maxSetNum == int.MaxValue ? 
                (int)Math.Floor((decimal)OptionHolder.VanillaMaxPlayerNum / this.setPlayerNum) : this.maxSetNum;

            new IntCustomOption(
                GetRoleOptionId(RoleCommonOption.RoleNum),
                string.Concat(
                    this.roleName,
                    RoleCommonOption.RoleNum.ToString()),
                1, 1, thisMaxRoleNum, 1,
                roleSetOption);
            new BoolCustomOption(
                GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign),
                string.Concat(
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

    public abstract class FlexibleCombinationRoleManagerBase : CombinationRoleManagerBase
    {

        public MultiAssignRoleBase BaseRole;
        private int minimumRoleNum = 0;
        private bool canAssignImposter = true;

        public FlexibleCombinationRoleManagerBase(
            MultiAssignRoleBase role,
            int minimumRoleNum = 2,
            bool canAssignImposter = true) : 
                base(role.Id.ToString(), role.NameColor)
        {
            this.BaseRole = role;
            this.minimumRoleNum = minimumRoleNum;
            this.canAssignImposter = canAssignImposter;
        }
        public override void AssignSetUpInit(int curImpNum)
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
                isEvil = isEvil && 
                    (UnityEngine.Random.RandomRange(0, 110) < (int)Decimal.Multiply(
                        spawnOption.GetValue(), spawnOption.Selections.ToList().Count)) &&
                    curImpNum < PlayerControl.GameOptions.NumImpostors;

                if (isEvil)
                {
                    role.Team = ExtremeRoleType.Impostor;
                    role.NameColor = Palette.ImpostorRed;
                    role.CanKill = true;
                    role.UseVent = true;
                    role.UseSabotage = true;
                    role.HasTask = false;
                    ++curImpNum;
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
            
            if ((byte)this.BaseRole.Id != roleId) { return role; }

            this.BaseRole.CanHasAnotherRole = OptionHolder.AllOption[
                GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign)].GetValue();

            role = (MultiAssignRoleBase)this.BaseRole.Clone();

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
            var roleSetOption = new SelectionCustomOption(
                GetRoleOptionId(RoleCommonOption.SpawnRate),
                Design.ColoedString(
                    this.optionColor,
                    string.Concat(
                        this.roleName,
                        RoleCommonOption.SpawnRate.ToString())),
                OptionHolder.SpawnRate, null, true);

            var roleAssignNumOption = new IntCustomOption(
                GetRoleOptionId(CombinationRoleCommonOption.AssignsNum),
                string.Concat(
                    this.roleName,
                    CombinationRoleCommonOption.AssignsNum.ToString()),
                this.minimumRoleNum, this.minimumRoleNum,
                OptionHolder.VanillaMaxPlayerNum - 1, 1,
                roleSetOption, isHidden: this.minimumRoleNum <= 1);

            var roleSetNumOption = new IntCustomOption(
                GetRoleOptionId(RoleCommonOption.RoleNum),
                string.Concat(
                    this.roleName,
                    RoleCommonOption.RoleNum.ToString()),
                1, 1, (OptionHolder.VanillaMaxPlayerNum - 1), 1,
                roleSetOption);

            roleAssignNumOption.SetUpdateOption(roleSetNumOption);

            if (this.canAssignImposter)
            {
                var isImposterAssignOps = new BoolCustomOption(
                    GetRoleOptionId(CombinationRoleCommonOption.IsAssignImposter),
                    string.Concat(
                        this.roleName,
                        CombinationRoleCommonOption.IsAssignImposter.ToString()),
                    false, roleSetOption);

                new SelectionCustomOption(
                    GetRoleOptionId(CombinationRoleCommonOption.ImposterSelectedRate),
                    string.Concat(
                        this.roleName,
                        CombinationRoleCommonOption.ImposterSelectedRate.ToString()),
                    OptionHolder.SpawnRate, isImposterAssignOps);
            }

            new BoolCustomOption(
                GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign),
                string.Concat(
                    this.roleName,
                    CombinationRoleCommonOption.IsMultiAssign.ToString()),
                false, roleSetOption, isHidden: this.minimumRoleNum <= 1);

            return roleSetOption;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            int optionOffset = this.OptionIdOffset + ExtremeRoleManager.OptionOffsetPerRole;
            this.BaseRole.SetManagerOptionOffset(this.OptionIdOffset);
            this.BaseRole.CreateRoleSpecificOption(
                parentOps,
                optionOffset);
        }

        protected override void CommonInit()
        {
            this.Roles.Clear();
            int roleAssignNum = 1;
            var allOptions = OptionHolder.AllOption;

            this.BaseRole.CanHasAnotherRole = allOptions[
                GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign)].GetValue();

            if (allOptions.ContainsKey(GetRoleOptionId(CombinationRoleCommonOption.AssignsNum)))
            {
                roleAssignNum = allOptions[
                    GetRoleOptionId(CombinationRoleCommonOption.AssignsNum)].GetValue();
            }

            for (int i = 0; i < roleAssignNum; ++i)
            {
                this.Roles.Add((MultiAssignRoleBase)this.BaseRole.Clone());
            }
        }

    }

}
