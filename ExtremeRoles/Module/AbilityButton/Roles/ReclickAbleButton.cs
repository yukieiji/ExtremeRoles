using System;
using UnityEngine;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.AbilityButton.Roles
{
    public class ReclickableButton : RoleAbilityButtonBase
    {
        public ReclickableButton(
            string buttonText,
            Func<bool> ability,
            Func<bool> canUse,
            Sprite sprite,
            Vector3 positionOffset,
            Action abilityCleanUp,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false) : base(
                buttonText,
                ability,
                canUse,
                sprite,
                positionOffset,
                abilityCleanUp,
                abilityCheck,
                hotkey, mirror)
        {}

        protected override void AbilityButtonUpdate()
        {
            if (this.CanUse() || this.IsAbilityOn)
            {
                this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.EnabledColor;
                this.Button.graphic.material.SetFloat("_Desat", 0f);
            }
            else
            {
                this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.DisabledClear;
                this.Button.graphic.material.SetFloat("_Desat", 1f);
            }

            if (this.Timer >= 0)
            {
                if (IsAbilityOn || (
                        !CachedPlayerControl.LocalPlayer.PlayerControl.inVent &&
                        CachedPlayerControl.LocalPlayer.PlayerControl.moveable))
                {
                    this.Timer -= Time.deltaTime;
                }
                if (IsAbilityOn)
                {
                    if (!this.AbilityCheck())
                    {
                        this.Timer = 0;
                        this.IsAbilityOn = false;
                    }
                }
            }

            if (this.Timer <= 0 && IsAbilityOn)
            {
                this.abilityOff();
            }

            Button.SetCoolDown(
                this.Timer,
                (this.IsAbilityOn) ? this.AbilityActiveTime : this.CoolTime);
        }

        protected override void OnClickEvent()
        {
            if (this.IsAbilityOn)
            {
                this.abilityOff();
            }

            else if (
                this.CanUse() &&
                this.Timer < 0f &&
                !this.IsAbilityOn)
            {

                if (this.UseAbility())
                {
                    this.Timer = this.AbilityActiveTime;
                    Button.cooldownTimerText.color = this.TimerOnColor;
                    this.IsAbilityOn = true;
                }
            }
        }

        private void abilityOff()
        {
            this.IsAbilityOn = false;
            this.Button.cooldownTimerText.color = Palette.EnabledColor;
            this.CleanUp();
            this.ResetCoolTimer();
        }
    }
}
