using System;
using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;

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
        public RoleAbilityButtonBase Button
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

        public static void CreateNormalAbilityButton(
            this IRoleAbility self,
            string buttonName,
            Sprite sprite,
            Vector3? positionOffset = null,
            Action abilityCleanUp = null,
            Func<bool> checkAbility = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false)
        {
            Vector3 offset = positionOffset ?? new Vector3(-1.8f, -0.06f, 0);

            self.Button = new ReusableAbilityButton(
                buttonName,
                self.UseAbility,
                self.IsAbilityUse,
                sprite,
                offset,
                abilityCleanUp,
                checkAbility,
                hotkey,
                mirror);

            self.RoleAbilityInit();
        }

        public static void CreateAbilityCountButton(
            this IRoleAbility self,
            string buttonName,
            Sprite sprite,
            Vector3? positionOffset = null,
            Action abilityCleanUp = null,
            Func<bool> checkAbility = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false)
        {   
            Vector3 offset = positionOffset ?? new Vector3(-1.8f, -0.06f, 0);

            self.Button = new AbilityCountButton(
                buttonName,
                self.UseAbility,
                self.IsAbilityUse,
                sprite,
                offset,
                abilityCleanUp,
                checkAbility,
                hotkey,
                mirror);

            self.RoleAbilityInit();

        }


        public static void CreateReclickableAbilityButton(
            this IRoleAbility self,
            string buttonName,
            Sprite sprite,
            Action abilityCleanUp,
            Vector3? positionOffset = null,
            Func<bool> checkAbility = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false)
        {
            Vector3 offset = positionOffset ?? new Vector3(-1.8f, -0.06f, 0);

            self.Button = new ReclickableButton(
                buttonName,
                self.UseAbility,
                self.IsAbilityUse,
                sprite,
                offset,
                abilityCleanUp,
                checkAbility,
                hotkey,
                mirror);

            self.RoleAbilityInit();
        }

        public static void CreateChargeAbilityButton(
            this IRoleAbility self,
            string buttonName,
            Sprite sprite,
            Action abilityCleanUp,
            Vector3? positionOffset = null,
            Func<bool> checkAbility = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false)
        {
            Vector3 offset = positionOffset ?? new Vector3(-1.8f, -0.06f, 0);

            self.Button = new ChargableButton(
                buttonName,
                self.UseAbility,
                self.IsAbilityUse,
                sprite,
                offset,
                abilityCleanUp,
                checkAbility,
                hotkey,
                mirror);

            self.RoleAbilityInit();
        }

        public static void CreatePassiveAbilityButton(
            this IRoleAbility self,
            string activateButtonName,
            string deactivateButtonName,
            Sprite activateSprite,
            Sprite deactivateSprite,
            Action abilityCleanUp,
            Vector3? positionOffset = null,
            Func<bool> checkAbility = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false)
        {
            Vector3 offset = positionOffset ?? new Vector3(-1.8f, -0.06f, 0);

            self.Button = new PassiveAbilityButton(
                activateButtonName,
                deactivateButtonName,
                self.UseAbility,
                self.IsAbilityUse,
                activateSprite,
                deactivateSprite,
                offset,
                abilityCleanUp,
                checkAbility,
                hotkey,
                mirror);

            self.RoleAbilityInit();
        }


        public static void CreateCommonAbilityOption(
            this IRoleAbility self,
            CustomOptionBase parentOps,
            bool hasActiveTime = false)
        {
            CustomOption.Create(
                self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime),
                string.Concat(
                    ((SingleRoleBase)self).RoleName,
                    RoleAbilityCommonOption.AbilityCoolTime.ToString()),
                30f, 2.5f, 120f, 0.5f,
                parentOps, format: "unitSeconds");

            if (hasActiveTime)
            {
                CustomOption.Create(
                    self.GetRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime),
                    string.Concat(
                        ((SingleRoleBase)self).RoleName,
                        RoleAbilityCommonOption.AbilityActiveTime.ToString()),
                    2.5f, 0.5f, 60f, 0.5f,
                    parentOps, format: "unitSeconds");
            }

        }

        public static void CreateAbilityCountOption(
            this IRoleAbility self,
            CustomOptionBase parentOps,
            int maxAbilityCount,
            bool hasActiveTime = false)
        {

            self.CreateCommonAbilityOption(
                parentOps,
                hasActiveTime);

            CustomOption.Create(
                self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCount),
                string.Concat(
                    ((SingleRoleBase)self).RoleName,
                    RoleAbilityCommonOption.AbilityCount.ToString()),
                1, 1, maxAbilityCount, 1,
                parentOps);

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

            var allOps = OptionHolder.AllOption;
            self.Button.SetAbilityCoolTime(
                allOps[self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)].GetValue());

            int checkOptionId = self.GetRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime);

            if (allOps.ContainsKey(checkOptionId))
            {
                self.Button.SetAbilityActiveTime(
                    allOps[checkOptionId].GetValue());
            }

            var button = self.Button as AbilityCountButton;

            if (button != null)
            {
                button.UpdateAbilityCount(
                    allOps[self.GetRoleOptionId(
                        RoleAbilityCommonOption.AbilityCount)].GetValue());
            }

            self.Button.ResetCoolTimer();
        }
    }

}
