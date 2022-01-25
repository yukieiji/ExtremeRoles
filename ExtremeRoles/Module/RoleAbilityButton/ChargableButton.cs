using System;
using UnityEngine;

namespace ExtremeRoles.Module.RoleAbilityButton
{
    public class ChargableButton : RoleAbilityButtonBase
    {
        private float currentCharge;

        public ChargableButton(
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
        { }

        public override void SetAbilityActiveTime(float time)
        {
            this.currentCharge = time;
            base.SetAbilityActiveTime(time);
        }
        public override void ResetCoolTimer()
        {
            this.currentCharge = this.AbilityActiveTime;
            base.ResetCoolTimer();
        }

        protected override void AbilityButtonUpdate()
        {
            if ((this.CanUse() || this.IsAbilityOn) && this.currentCharge > 0f)
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
                        this.currentCharge = this.Timer;
                        this.Timer = 0;
                        this.IsAbilityOn = false;
                    }
                }
            }
            if (PlayerControl.LocalPlayer.AllTasksCompleted())
            {
                this.currentCharge = this.AbilityActiveTime;
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
                    this.Timer = this.currentCharge;
                    Button.cooldownTimerText.color = this.TimerOnColor;
                    this.IsAbilityOn = true;
                }
            }
        }

        private void abilityOff()
        {
            this.IsAbilityOn = false;
            this.currentCharge = this.Timer;
            this.Button.cooldownTimerText.color = Palette.EnabledColor;
            this.CleanUp();
            if (this.currentCharge > 0.0f)
            {
                this.Timer = 0.0f;
            }
            else
            {
                this.ResetCoolTimer();
            }
        }

    }
}
