using System;
using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Roles.API.Interface
{
    public enum RoleAbilityCommonSetting
    {
        AbilityCoolTime = 21,
        AbilityActiveTime = 22,
    }
    public interface IRoleAbility
    {
        public RoleAbilityButton Button
        {
            get;
            set;
        }

        public void CreateAbility();

        public void UseAbility();

        public bool IsAbilityUse();
    }

    public static class IRoleAbilityMixin
    {
        public static RoleAbilityButton CreateAbilityButton(
            this IRoleAbility self,
            Sprite sprite,
            Vector3? positionOffset = null,
            Action abilityCleanUp = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false)
        {   
            Vector3 offset = positionOffset ?? new Vector3(-1.8f, -0.06f, 0);

            return new RoleAbilityButton(
                self.UseAbility,
                self.IsAbilityUse,
                sprite,
                offset,
                abilityCleanUp,
                hotkey,
                mirror);
        }

        public static int GetRoleSettingId(
            this IRoleAbility self,
            RoleAbilityCommonSetting setting) => ((IRoleSetting)self).GetRoleSettingId((int)setting);

        public static void CreateRoleAbilityOption(
            this IRoleAbility self,
            CustomOptionBase parentOps,
            bool hasActiveTime=false)
        {

            CustomOption.Create(
                self.GetRoleSettingId(RoleAbilityCommonSetting.AbilityCoolTime),
                Design.ConcatString(
                    ((SingleRoleBase)self).RoleName,
                    RoleAbilityCommonSetting.AbilityCoolTime.ToString()),
                30f, 2.5f, 120f, 2.5f,
                parentOps, format: "unitSeconds");
            if (hasActiveTime)
            {
                CustomOption.Create(
                    self.GetRoleSettingId(RoleAbilityCommonSetting.AbilityActiveTime),
                    Design.ConcatString(
                        ((SingleRoleBase)self).RoleName,
                        RoleAbilityCommonSetting.AbilityActiveTime.ToString()),
                    30f, 2.5f, 120f, 2.5f,
                    parentOps, format: "unitSeconds");
            }
        }

        public static void RoleAbilityInit(this IRoleAbility self)
        {

            if (self.Button == null) { return; }

            var allOps = OptionsHolder.AllOptions;
            self.Button.SetAbilityCoolTime(
                allOps[self.GetRoleSettingId(RoleAbilityCommonSetting.AbilityCoolTime)].GetValue());

            int checkSettingId = self.GetRoleSettingId(RoleAbilityCommonSetting.AbilityActiveTime);

            if (allOps.ContainsKey(checkSettingId))
            {
                self.Button.SetAbilityActiveTime(
                    allOps[checkSettingId].GetValue());
            }
            
            self.Button.ResetCoolTimer();
        }

        public static bool IsCommonUse(this IRoleAbility _)
        {
            return PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.CanMove;
        }
    }

}
