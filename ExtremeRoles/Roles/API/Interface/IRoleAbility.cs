using System;
using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;

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

        public void RoleAbilityResetOnMeetingStart(); // サイドキック作成時に呼ばれるためnullエラーを考慮すること

        public void RoleAbilityResetOnMeetingEnd();

    }

    public static class IRoleAbilityMixin
    {
        private const float defaultCoolTime = 30.0f;
        private const float minCoolTime = 0.5f;
        private const float maxCoolTime = 120.0f;
        private const float minActiveTime = 0.5f;
        private const float maxActiveTime = 60.0f;
        private const float step = 0.5f;

        public static void CreateNormalAbilityButton(
            this IRoleAbility self,
            string buttonName,
            Sprite sprite,
            Action abilityCleanUp = null,
            Func<bool> checkAbility = null,
            KeyCode hotkey = KeyCode.F)
        {

            self.Button = new ReusableAbilityButton(
                buttonName,
                self.UseAbility,
                self.IsAbilityUse,
                sprite,
                abilityCleanUp,
                checkAbility,
                hotkey);

            self.RoleAbilityInit();
        }

        public static void CreateAbilityCountButton(
            this IRoleAbility self,
            string buttonName,
            Sprite sprite,
            Action abilityCleanUp = null,
            Func<bool> checkAbility = null,
            KeyCode hotkey = KeyCode.F)
        {   
            self.Button = new AbilityCountButton(
                buttonName,
                self.UseAbility,
                self.IsAbilityUse,
                sprite, abilityCleanUp,
                checkAbility, hotkey);

            self.RoleAbilityInit();

        }


        public static void CreateReclickableAbilityButton(
            this IRoleAbility self,
            string buttonName,
            Sprite sprite,
            Action abilityCleanUp,
            Func<bool> checkAbility = null,
            KeyCode hotkey = KeyCode.F)
        {
            self.Button = new ReclickableButton(
                buttonName,
                self.UseAbility,
                self.IsAbilityUse,
                sprite, abilityCleanUp,
                checkAbility, hotkey);

            self.RoleAbilityInit();
        }

        public static void CreateChargeAbilityButton(
            this IRoleAbility self,
            string buttonName,
            Sprite sprite,
            Action abilityCleanUp,
            Func<bool> checkAbility = null,
            KeyCode hotkey = KeyCode.F)
        {

            self.Button = new ChargableButton(
                buttonName,
                self.UseAbility,
                self.IsAbilityUse,
                sprite,
                abilityCleanUp,
                checkAbility,
                hotkey);

            self.RoleAbilityInit();
        }

        public static void CreatePassiveAbilityButton(
            this IRoleAbility self,
            string activateButtonName,
            string deactivateButtonName,
            Sprite activateSprite,
            Sprite deactivateSprite,
            Action abilityCleanUp,
            Func<bool> checkAbility = null,
            KeyCode hotkey = KeyCode.F)
        {
            self.Button = new PassiveAbilityButton(
                activateButtonName,
                deactivateButtonName,
                self.UseAbility,
                self.IsAbilityUse,
                activateSprite,
                deactivateSprite,
                abilityCleanUp,
                checkAbility,
                hotkey);

            self.RoleAbilityInit();
        }


        public static void CreateCommonAbilityOption(
            this IRoleAbility self,
            IOption parentOps,
            float defaultActiveTime = float.MaxValue)
        {

            SingleRoleBase role = (SingleRoleBase)self;

            new FloatCustomOption(
                self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime),
                string.Concat(
                    role.RoleName,
                    RoleAbilityCommonOption.AbilityCoolTime.ToString()),
                defaultCoolTime, minCoolTime, maxCoolTime, step,
                parentOps, format: OptionUnit.Second,
                tab: role.Tab);

            if (defaultActiveTime != float.MaxValue)
            {
                defaultActiveTime = Mathf.Clamp(
                    defaultActiveTime, minActiveTime, maxActiveTime);

                new FloatCustomOption(
                    self.GetRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime),
                    string.Concat(
                        role.RoleName,
                        RoleAbilityCommonOption.AbilityActiveTime.ToString()),
                    defaultActiveTime, minActiveTime, maxActiveTime, step,
                    parentOps, format: OptionUnit.Second,
                    tab: role.Tab);
            }

        }

        public static void CreateAbilityCountOption(
            this IRoleAbility self,
            IOption parentOps,
            int defaultAbilityCount,
            int maxAbilityCount,
            float defaultActiveTime = float.MaxValue)
        {

            SingleRoleBase role = (SingleRoleBase)self;

            self.CreateCommonAbilityOption(
                parentOps,
                defaultActiveTime);

            new IntCustomOption(
                self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCount),
                string.Concat(
                    role.RoleName,
                    RoleAbilityCommonOption.AbilityCount.ToString()),
                defaultAbilityCount, 1,
                maxAbilityCount, 1,
                parentOps, format: OptionUnit.Shot,
                tab: role.Tab);

        }

        public static int GetRoleOptionId(
            this IRoleAbility self,
            RoleAbilityCommonOption option) => ((RoleOptionBase)self).GetRoleOptionId((int)option);

        public static bool IsCommonUse(this IRoleAbility _)
        {
            return PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.CanMove;
        }

        public static void ResetOnMeetingEnd(this IRoleAbility self)
        {
            if (self.Button != null)
            {
                self.Button.ResetCoolTimer();
            }
            self.RoleAbilityResetOnMeetingEnd();
        }

        public static void ResetOnMeetingStart(this IRoleAbility self)
        {
            if (self.Button != null)
            {
                self.Button.SetActive(false);
                self.Button.ForceAbilityOff();
            }
            self.RoleAbilityResetOnMeetingStart();
        }


        public static void RoleAbilityInit(this IRoleAbility self)
        {

            if (self.Button == null) { return; }

            var allOpt = OptionHolder.AllOption;
            self.Button.SetAbilityCoolTime(
                allOpt[self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)].GetValue());

            int checkOptionId = self.GetRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime);

            if (allOpt.ContainsKey(checkOptionId))
            {
                self.Button.SetAbilityActiveTime(
                    allOpt[checkOptionId].GetValue());
            }

            var button = self.Button as AbilityCountButton;

            if (button != null)
            {
                button.UpdateAbilityCount(
                    allOpt[self.GetRoleOptionId(
                        RoleAbilityCommonOption.AbilityCount)].GetValue());
            }

            self.Button.ResetCoolTimer();
        }
    }

}
