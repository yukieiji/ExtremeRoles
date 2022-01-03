using System;
using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Roles.API.Interface
{
    public enum RoleAbilityCommonOption
    {
        AbilityCoolTime = 35,
        AbilityCount,
        AbilityActiveTime,
    }
    public interface IRoleAbility
    {
        public RoleAbilityButton Button
        {
            get;
            set;
        }

        public void CreateAbility();

        public bool UseAbility();

        public bool IsAbilityUse();

        public void RoleAbilityResetOnMeetingStart();

        public void RoleAbilityResetOnMeetingEnd();

    }

    public static class IRoleAbilityMixin
    {
        public static void CreateAbilityButton(
            this IRoleAbility self,
            string buttonName,
            Sprite sprite,
            Vector3? positionOffset = null,
            Action abilityCleanUp = null,
            int abilityNum = int.MaxValue,
            Func<bool> checkAbility = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false)
        {   
            Vector3 offset = positionOffset ?? new Vector3(-1.8f, -0.06f, 0);

            self.Button = new RoleAbilityButton(
                buttonName,
                self.UseAbility,
                self.IsAbilityUse,
                sprite,
                offset,
                abilityNum,
                abilityCleanUp,
                checkAbility,
                hotkey,
                mirror);

            self.RoleAbilityInit();

        }

        public static void CreateRoleAbilityOption(
            this IRoleAbility self,
            CustomOptionBase parentOps,
            bool hasActiveTime=false,
            int maxAbilityCount=int.MaxValue)
        {

            CustomOption.Create(
                self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime),
                Design.ConcatString(
                    ((SingleRoleBase)self).RoleName,
                    RoleAbilityCommonOption.AbilityCoolTime.ToString()),
                30f, 2.5f, 120f, 2.5f,
                parentOps, format: "unitSeconds");

            if (hasActiveTime)
            {
                CustomOption.Create(
                    self.GetRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime),
                    Design.ConcatString(
                        ((SingleRoleBase)self).RoleName,
                        RoleAbilityCommonOption.AbilityActiveTime.ToString()),
                    2.5f, 0.5f, 30f, 0.5f,
                    parentOps, format: "unitSeconds");
            }

            if (maxAbilityCount != int.MaxValue)
            {
                CustomOption.Create(
                    self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCount),
                    Design.ConcatString(
                        ((SingleRoleBase)self).RoleName,
                        RoleAbilityCommonOption.AbilityCount.ToString()),
                    1, 1, maxAbilityCount, 1,
                    parentOps);
            }

        }

        public static int GetRoleOptionId(
            this IRoleAbility self,
            RoleAbilityCommonOption option) => ((IRoleOption)self).GetRoleOptionId((int)option);

        public static bool IsCommonUse(this IRoleAbility _)
        {
            return PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.CanMove;
        }

        public static void ResetOnMeetingEnd(this IRoleAbility self)
        {
            self.Button.ResetCoolTimer();
            self.RoleAbilityResetOnMeetingEnd();
        }

        public static void ResetOnMeetingStart(this IRoleAbility self)
        {
            self.Button.SetActive(false);
            self.Button.ForceAbilityOff();
            self.RoleAbilityResetOnMeetingStart();
        }


        public static void RoleAbilityInit(this IRoleAbility self)
        {

            if (self.Button == null) { return; }

            var allOps = OptionsHolder.AllOption;
            self.Button.SetAbilityCoolTime(
                allOps[self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)].GetValue());

            int checkOptionId = self.GetRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime);

            if (allOps.ContainsKey(checkOptionId))
            {
                self.Button.SetAbilityActiveTime(
                    allOps[checkOptionId].GetValue());
            }
            checkOptionId = self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCount);

            if (allOps.ContainsKey(checkOptionId))
            {
                self.Button.UpdateAbilityCount(
                    allOps[checkOptionId].GetValue());
            }

            self.Button.ResetCoolTimer();
        }
    }

}
