using System;
using UnityEngine;

namespace ExtremeRoles.Module.RoleAbilityButton
{
    public class PassiveAbilityButton : RoleAbilityButtonBase
    {

        private string activateButtonText;
        private string deactivateButtonText;

        private Sprite deactivateButtonSprite;
        private Sprite activateButtonSprite;

        public PassiveAbilityButton(
            string activateButtonText,
            string deactivateButtonText,
            Func<bool> ability,
            Func<bool> canUse,
            Sprite activateSprite,
            Sprite deactivateSprite,
            Vector3 positionOffset,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false) : base(
                activateButtonText,
                ability,
                canUse,
                activateSprite,
                positionOffset,
                abilityCleanUp,
                abilityCheck,
                hotkey, mirror)
        {
            this.activateButtonText = activateButtonText;
            this.deactivateButtonText = deactivateButtonText;

            this.activateButtonSprite = activateSprite;
            this.deactivateButtonSprite = deactivateSprite;
        
        }

        protected override void AbilityButtonUpdate()
        {
            if ((this.CanUse() || this.IsAbilityOn) && this.Timer < 0f)
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
                if (IsAbilityOn || (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable))
                {
                    this.Timer -= Time.deltaTime;
                }
                if (IsAbilityOn)
                {
                    if (!this.AbilityCheck())
                    {
                        passiveAbilityOff();
                        this.Timer = 0;
                    }
                }
            }

            Button.SetCoolDown(
                this.Timer,
                (this.IsAbilityOn) ? this.AbilityActiveTime : this.CoolTime);
        }

        protected override void OnClickEvent()
        {
            if (this.CanUse() && this.Timer < 0f)
            {
                if (this.IsAbilityOn)
                {
                    passiveAbilityOff();
                }
                else
                {
                    if (this.UseAbility())
                    {
                        passiveAbilityOn();
                    }
                }
            }
        }

        private void passiveAbilityOn()
        {
            this.Timer = this.AbilityActiveTime;
            this.ButtonSprite = this.deactivateButtonSprite;
            this.ButtonText = this.deactivateButtonText;
            Button.cooldownTimerText.color = this.TimerOnColor;
            this.IsAbilityOn = true;
        }

        private void passiveAbilityOff()
        {
            this.IsAbilityOn = false;
            this.ButtonSprite = this.activateButtonSprite;
            this.ButtonText = this.activateButtonText;
            this.Button.cooldownTimerText.color = Palette.EnabledColor;
            this.CleanUp();
            this.ResetCoolTimer();
        }
    }
}
