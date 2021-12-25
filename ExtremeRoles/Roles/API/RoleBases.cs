using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;


namespace ExtremeRoles.Roles.API
{
    public abstract class SingleRoleBase : RoleSettingBase
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

        public SingleRoleBase()
        { }
        public SingleRoleBase(
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

        public virtual SingleRoleBase Clone()
        {
            SingleRoleBase copy = (SingleRoleBase)this.MemberwiseClone();
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

        public string GetColoredRoleName() => Design.ColoedString(
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
            CustomOptionBase parentOps)
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
        protected override CustomOptionBase CreateSpawnOption()
        {
            var roleSetOption = CustomOption.Create(
                GetRoleSettingId(RoleCommonSetting.SpawnRate),
                Design.ColoedString(
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
            CustomOptionBase parentOps)
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
                GetRoleSettingId(RoleCommonSetting.HasOtherVison)].GetValue();
            this.Vison = allOption[
                GetRoleSettingId(RoleCommonSetting.Vison)].GetValue();
            this.IsApplyEnvironmentVision = allOption[
                GetRoleSettingId(RoleCommonSetting.ApplyEnvironmentVisionEffect)].GetValue();

            if (this.CanKill)
            {
                this.HasOtherKillCool = allOption[
                    GetRoleSettingId(KillerCommonSetting.HasOtherKillCool)].GetValue();
                this.KillCoolTime = allOption[
                    GetRoleSettingId(KillerCommonSetting.KillCoolDown)].GetValue();
                this.HasOtherKillRange = allOption[
                    GetRoleSettingId(KillerCommonSetting.HasOtherKillRange)].GetValue();
                this.KillRange = allOption[
                    GetRoleSettingId(KillerCommonSetting.KillRange)].GetValue();
            }
        }
    }
    public abstract class MultiAssignRoleBase : SingleRoleBase
    {
        public byte GameId = 0;
        public SingleRoleBase AnotherRole = null;
        public bool CanHasAnotherRole = false;

        public MultiAssignRoleBase(
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

        public void SetAnotherRole(SingleRoleBase role)
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
}
