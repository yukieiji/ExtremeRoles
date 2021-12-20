using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

namespace ExtremeRoles.Roles
{
    public enum ExtremeRoleType
    {
        Null = -2,
        Neutral = -1,
        Crewmate = 0,
        Impostor = 1
    }
    public enum RoleCommonSetting
    {
        RoleNum = 15,
        SpawnRate = 16,
        HasOtherVison = 17,
        Vison = 18,
        ApplyEnvironmentVisionEffect = 19,
    }
    public enum KillerCommonSetting
    {
        HasOtherKillRange = 11,
        KillRange = 12,
        HasOtherKillCool = 13,
        KillCoolDown = 14,
    }
    public enum CombinationRoleCommonSetting
    {
        CombinationRoleNum = 23,
        IsMultiAssign = 24,
    }

    interface IRoleAbility
    {
        void CreateAbility();
        void UseAbility();
    }

    abstract public class RoleAbs
    {

        public bool CanKill = false;

        protected int OptionIdOffset = 0;

        
        public int GetRoleSettingId(
            RoleCommonSetting setting) => GetRoleSettingId((int)setting);
        
        public int GetRoleSettingId(
            KillerCommonSetting setting) => GetRoleSettingId((int)setting);

        public int GetRoleSettingId(
            CombinationRoleCommonSetting setting) => GetRoleSettingId((int)setting);

        public int GetRoleSettingId(int setting) => this.OptionIdOffset + setting;

        public void GameInit()
        {
            CommonInit();
            RoleSpecificInit();
        }

        public void CreateRoleAllOption(
            int optionIdOffset)
        {
            this.OptionIdOffset = optionIdOffset;
            var parentOps = CreateSpawnOption();
            CreateVisonOption(parentOps);
            CreateSpecificOption(parentOps);
            if (this.CanKill)
            {
                CreateKillerOption(parentOps);
            }
        }
        public void CreatRoleSpecificOption(
            CustomOption parentOps,
            int optionIdOffset)
        {
            this.OptionIdOffset = optionIdOffset;
            CreateVisonOption(parentOps);
            CreateSpecificOption(parentOps);
            if (this.CanKill)
            {
                CreateKillerOption(parentOps);
            }
        }
        protected abstract void CreateKillerOption(
            CustomOption parentOps);
        protected abstract CustomOption CreateSpawnOption();

        protected abstract void CreateSpecificOption(
            CustomOption parentOps);
        protected abstract void CreateVisonOption(
            CustomOption parentOps);

        protected abstract void CommonInit();

        protected abstract void RoleSpecificInit();

    }

    public abstract class SingleRoleAbs : RoleAbs
    {
        public bool IsVanilaRole = false;
        public bool HasTask = true;
        public bool UseVent = false;
        public bool UseSabotage = false;
        public bool HasOtherVison = false;
        public bool HasOtherKillCool = false;
        public bool HasOtherKillRange = false;
        public bool IsApplyEnvironmentVision = true;
        public bool IsWin = false;

        public float Vison = 0f;
        public float KillCoolTime = 0f;
        public int KillRange = 1;

        public string RoleName;

        public Color NameColor;
        public ExtremeRoleId Id;
        public byte BytedRoleId;
        public ExtremeRoleType Teams;

        public SingleRoleAbs()
        { }
        public SingleRoleAbs(
            ExtremeRoleId id,
            ExtremeRoleType team,
            string roleName,
            Color roleColor,
            bool canKill,
            bool hasTask,
            bool useVent,
            bool useSabotage,
            bool isVanilaRole = false)
        {
            this.Id = id;
            this.BytedRoleId = (byte)this.Id;
            this.Teams = team;
            this.RoleName = roleName;
            this.NameColor = roleColor;
            this.CanKill = canKill;
            this.HasTask = hasTask;
            this.UseVent = useVent;
            this.UseSabotage = useSabotage;

            this.IsVanilaRole = isVanilaRole;
        }

        public virtual SingleRoleAbs Clone()
        {
            SingleRoleAbs copy = (SingleRoleAbs)this.MemberwiseClone();
            Color baseColor = this.NameColor;

            copy.NameColor = new Color(
                baseColor.r,
                baseColor.g,
                baseColor.b,
                baseColor.a);

            return copy;
        }

        public bool IsCrewmate() => this.Teams == ExtremeRoleType.Crewmate;

        public bool IsImposter() => this.Teams == ExtremeRoleType.Impostor;

        public bool IsNeutral() => this.Teams == ExtremeRoleType.Neutral;

        public string GetColoredRoleName() => Design.Cs(
            this.NameColor, this.RoleName);

        public virtual bool IsTeamsWin() => this.IsWin;

        public virtual void DaedAction(
            DeathReason reason,
            ref PlayerControl rolePlayer)
        {
            return;
        }

        public virtual void ExiledAction(
            GameData.PlayerInfo rolePlayer)
        {
            return;
        }

        public virtual void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
            return;
        }

        public virtual bool TryRolePlayerKill(
            PlayerControl rolePlayer,
            PlayerControl fromPlayer) => true;

        protected override void CreateKillerOption(
            CustomOption parentOps)
        {
            var killCoolSetting = CustomOption.Create(
                GetRoleSettingId(KillerCommonSetting.HasOtherKillCool),
                Design.ConcatString(
                    this.RoleName,
                    KillerCommonSetting.HasOtherKillCool.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleSettingId(KillerCommonSetting.KillCoolDown),
                Design.ConcatString(
                    this.RoleName,
                    KillerCommonSetting.KillCoolDown.ToString()),
                30f, 2.5f, 120f, 2.5f,
                killCoolSetting, format: "unitSeconds");

            var killRangeSetting = CustomOption.Create(
                GetRoleSettingId(KillerCommonSetting.HasOtherKillRange),
                Design.ConcatString(
                    this.RoleName,
                    KillerCommonSetting.HasOtherKillRange.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleSettingId(KillerCommonSetting.KillRange),
                Design.ConcatString(
                    this.RoleName,
                    KillerCommonSetting.KillRange.ToString()),
                OptionsHolder.KillRange,
                killRangeSetting);
        }
        protected override CustomOption CreateSpawnOption()
        {
            var roleSetOption = CustomOption.Create(
                GetRoleSettingId(RoleCommonSetting.SpawnRate),
                Design.Cs(
                    this.NameColor,
                    Design.ConcatString(
                        this.Id.ToString(),
                        RoleCommonSetting.SpawnRate.ToString())),
                OptionsHolder.SpawnRate, null, true);

            CustomOption.Create(
                GetRoleSettingId(RoleCommonSetting.RoleNum),
                Design.ConcatString(
                    this.Id.ToString(),
                    RoleCommonSetting.RoleNum.ToString()),
                1, 1, OptionsHolder.VanillaMaxPlayerNum, 1, roleSetOption);

            return roleSetOption;
        }

        protected override void CreateVisonOption(
            CustomOption parentOps)
        {
            var visonOption = CustomOption.Create(
                GetRoleSettingId(RoleCommonSetting.HasOtherVison),
                Design.ConcatString(
                    this.Id.ToString(),
                    RoleCommonSetting.HasOtherVison.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleSettingId(RoleCommonSetting.Vison),
                Design.ConcatString(
                    this.Id.ToString(),
                    RoleCommonSetting.Vison.ToString()),
                2f, 0.25f, 5f, 0.25f,
                visonOption, format: "unitMultiplier");
            CustomOption.Create(
               GetRoleSettingId(RoleCommonSetting.ApplyEnvironmentVisionEffect),
               Design.ConcatString(
                   this.Id.ToString(),
                   RoleCommonSetting.ApplyEnvironmentVisionEffect.ToString()),
               this.IsCrewmate(), visonOption);
        }
        protected override void CommonInit()
        {
            var allOption = OptionsHolder.AllOptions;

            this.HasOtherVison = allOption[
                GetRoleSettingId(RoleCommonSetting.HasOtherVison)].GetBool();
            this.Vison = allOption[
                GetRoleSettingId(RoleCommonSetting.Vison)].GetFloat();
            this.IsApplyEnvironmentVision = allOption[
                GetRoleSettingId(RoleCommonSetting.ApplyEnvironmentVisionEffect)].GetBool();

            if (this.CanKill)
            {
                this.HasOtherKillCool = allOption[
                    GetRoleSettingId(KillerCommonSetting.HasOtherKillCool)].GetBool();
                this.KillCoolTime = allOption[
                    GetRoleSettingId(KillerCommonSetting.KillCoolDown)].GetFloat();
                this.HasOtherKillRange = allOption[
                    GetRoleSettingId(KillerCommonSetting.HasOtherKillRange)].GetBool();
                this.KillRange = allOption[
                    GetRoleSettingId(KillerCommonSetting.KillRange)].GetSelection();
            }
        }
    }
    public abstract class MultiAssignRoleAbs : SingleRoleAbs
    {
        public byte GameId = 0;
        public SingleRoleAbs AnotherRole = null;
        public bool CanHasAnotherRole = false;

        public MultiAssignRoleAbs(
            ExtremeRoleId id,
            ExtremeRoleType team,
            string roleName,
            Color roleColor,
            bool canKill,
            bool hasTask,
            bool useVent,
            bool useSabotage,
            bool isVanilaRole = false) : base(
                id, team, roleName, roleColor,
                canKill, hasTask, useVent,
                useSabotage, isVanilaRole)
        { }

        public void SetAnotherRole(SingleRoleAbs role)
        {
            if (this.CanHasAnotherRole)
            {
                this.AnotherRole = role;
                OverrideAnotherRoleSetting();
            }
        }
        protected virtual void OverrideAnotherRoleSetting()
        {
            this.Teams = this.AnotherRole.Teams;
            this.RoleName = string.Format("{0} + {1}",
                this.RoleName, this.AnotherRole.RoleName);
            this.NameColor = this.NameColor + this.AnotherRole.NameColor;
            this.CanKill = this.AnotherRole.CanKill;
            this.HasTask = this.AnotherRole.HasTask;
            this.UseVent = this.AnotherRole.UseVent;
            this.UseSabotage = this.AnotherRole.UseSabotage;
        }
    }

    // 別々の役職でコンビの物を管理する
    public abstract class CombinationRoleAbs : RoleAbs
    {

        public List<MultiAssignRoleAbs> Roles = new List<MultiAssignRoleAbs>();

        protected Color SettingColor;

        protected string RoleName = "";

        public CombinationRoleAbs(
            string roleName,
            Color settingColor)
        {
            this.SettingColor = settingColor;
            this.RoleName = roleName;
        }

    }
    public abstract class FixedCombRoleManagerBase : CombinationRoleAbs
    {

        protected int SetPlayerNum = 0;

        public FixedCombRoleManagerBase(
            string roleName,
            Color settingColor,
            int setPlayerNum) : base(
                roleName, settingColor)
        {
            this.SetPlayerNum = setPlayerNum;
        }

        protected override CustomOption CreateSpawnOption()
        {
            // ExtremeRolesPlugin.Instance.Log.LogInfo($"Color: {this.SettingColor}");
            var roleSetOption = CustomOption.Create(
                GetRoleSettingId(RoleCommonSetting.SpawnRate),
                Design.Cs(
                    this.SettingColor,
                    Design.ConcatString(
                        this.RoleName,
                        RoleCommonSetting.SpawnRate.ToString())),
                OptionsHolder.SpawnRate, null, true);

            int thisMaxRoleNum = (int)Math.Floor((decimal)OptionsHolder.VanillaMaxPlayerNum / this.SetPlayerNum);

            CustomOption.Create(
                GetRoleSettingId(RoleCommonSetting.RoleNum),
                Design.ConcatString(
                    this.RoleName,
                    RoleCommonSetting.RoleNum.ToString()),
                this.SetPlayerNum, this.SetPlayerNum, thisMaxRoleNum, this.SetPlayerNum,
                roleSetOption);
            CustomOption.Create(
                GetRoleSettingId(CombinationRoleCommonSetting.IsMultiAssign),
                Design.ConcatString(
                    this.RoleName,
                    CombinationRoleCommonSetting.IsMultiAssign.ToString()),
                false, roleSetOption);

            return roleSetOption;
        }
        protected override void CreateKillerOption(
            CustomOption parentOps)
        {
            // 複数ロールの中に殺戮者がいる可能性がため、管理ロールで殺戮者の設定はしない
            return;
        }
        
        protected override void CreateSpecificOption(
            CustomOption parentOps)
        {
            IEnumerable<SingleRoleAbs> collection = Roles;
            
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
            CustomOption parentOps)
        {
            // 複数のロールがまとまっているため、管理ロールで視界の設定はしない
            return;
        }

        protected override void CommonInit()
        {
            foreach (var role in Roles)
            {
                role.CanHasAnotherRole = OptionsHolder.AllOptions[
                    GetRoleSettingId(CombinationRoleCommonSetting.IsMultiAssign)].GetBool();
                role.GameId = 0;
                role.GameInit();
            }
        }
    }

    // 同じ役職でコンビの物を管理する
    public abstract class FlexibleCombRoleManagerBase : CombinationRoleAbs
    {

        protected int SetPlayerNum = 0;
        protected MultiAssignRoleAbs BaseRole;

        public FlexibleCombRoleManagerBase(
            string roleName,
            Color settingColor,
            MultiAssignRoleAbs baseRole) : base(roleName, settingColor)
        { 
            this.BaseRole = baseRole;
        }

        protected override CustomOption CreateSpawnOption()
        {
            // ExtremeRolesPlugin.Instance.Log.LogInfo($"Color: {this.SettingColor}");
            var roleSetOption = CustomOption.Create(
                GetRoleSettingId(RoleCommonSetting.SpawnRate),
                Design.Cs(
                    this.SettingColor,
                    Design.ConcatString(
                        this.RoleName,
                        RoleCommonSetting.SpawnRate.ToString())),
                OptionsHolder.SpawnRate, null, true);

            List<float> selections = new List<float>();
            for (float s = 2; s <= OptionsHolder.VanillaMaxPlayerNum; ++s)
            {
                selections.Add(s);
            }

            var roleCombNumOption = new ChainUpdateCustomOption(
                GetRoleSettingId(CombinationRoleCommonSetting.CombinationRoleNum),
                Design.ConcatString(
                    this.RoleName,
                    CombinationRoleCommonSetting.CombinationRoleNum.ToString()),
                selections.Cast<object>().ToArray(), 1,
                roleSetOption, false, false, "");

            var roleSetNumOption = CustomOption.Create(
                GetRoleSettingId(RoleCommonSetting.RoleNum),
                Design.ConcatString(
                    this.RoleName,
                    RoleCommonSetting.RoleNum.ToString()),
                2, 2, OptionsHolder.VanillaMaxPlayerNum, 2,
                roleSetOption);

            roleCombNumOption.ChainUpdateOption.Add(
                roleSetNumOption);

            CustomOption.Create(
                GetRoleSettingId(CombinationRoleCommonSetting.IsMultiAssign),
                Design.ConcatString(
                    this.RoleName,
                    CombinationRoleCommonSetting.IsMultiAssign.ToString()),
                false, roleSetOption);

            return roleSetOption;
        }

        protected override void CreateVisonOption(
            CustomOption parentOps)
        {
            var visonOption = CustomOption.Create(
                GetRoleSettingId(RoleCommonSetting.HasOtherVison),
                Design.ConcatString(
                    this.BaseRole.Id.ToString(),
                    RoleCommonSetting.HasOtherVison.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleSettingId(RoleCommonSetting.Vison),
                Design.ConcatString(
                    this.BaseRole.Id.ToString(),
                    RoleCommonSetting.Vison.ToString()),
                2f, 0.25f, 5f, 0.25f,
                visonOption, format: "unitMultiplier");
            CustomOption.Create(
               GetRoleSettingId(RoleCommonSetting.ApplyEnvironmentVisionEffect),
               Design.ConcatString(
                   this.BaseRole.Id.ToString(),
                   RoleCommonSetting.ApplyEnvironmentVisionEffect.ToString()),
               true, visonOption);
        }

        protected override void CommonInit()
        {

            var allOption = OptionsHolder.AllOptions;
            
            Roles.Clear();
            
            if (Map.IsGameLobby) { return; }

            int combinationNum = allOption[
                GetRoleSettingId(CombinationRoleCommonSetting.CombinationRoleNum)].GetInt();

            for (int i=0; i < combinationNum; ++i)
            {
                Roles.Add(
                    (MultiAssignRoleAbs)BaseRole.Clone());
            }

            foreach (var role in Roles)
            {
                role.CanHasAnotherRole = allOption[
                    GetRoleSettingId(CombinationRoleCommonSetting.IsMultiAssign)].GetBool();
                role.GameId = 0;
                role.GameInit();

                role.HasOtherVison = allOption[
                GetRoleSettingId(RoleCommonSetting.HasOtherVison)].GetBool();
                role.Vison = allOption[
                    GetRoleSettingId(RoleCommonSetting.Vison)].GetFloat();
                role.IsApplyEnvironmentVision = allOption[
                    GetRoleSettingId(RoleCommonSetting.ApplyEnvironmentVisionEffect)].GetBool();

            }
        }
    }

}
