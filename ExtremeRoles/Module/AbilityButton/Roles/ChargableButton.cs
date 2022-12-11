using System;
using UnityEngine;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.AbilityButton.Roles
{
    public class ChargableButton : RoleAbilityButtonBase
    {
        private float currentCharge;

        public ChargableButton(
            string buttonText,
            Func<bool> ability,
            Func<bool> canUse,
            Sprite sprite,
            Action abilityCleanUp,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F
            ) : base(
                buttonText,
                ability,
                canUse,
                sprite,
                abilityCleanUp,
                abilityCheck,
                hotkey)
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

            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            if (this.Timer >= 0)
            {
                if (IsAbilityOn ||
                    localPlayer.IsKillTimerEnabled ||
                    localPlayer.ForceKillTimerContinue)
                {
                    this.Timer -= Time.deltaTime;
                }
                if (IsAbilityOn)
                {
                    if (!this.AbilityCheck())
                    {
                        this.currentCharge = Mathf.Clamp(
                            this.Timer, 0.0f, this.AbilityActiveTime);
                        this.Timer = 0;
                        this.IsAbilityOn = false;
                    }
                }
            }
            if (localPlayer.AllTasksCompleted())
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
            this.currentCharge = this.Timer;
            this.Button.cooldownTimerText.color = Palette.EnabledColor;
            this.CleanUp();
            this.IsAbilityOn = false;
            if (this.currentCharge > 0.0f)
            {
                this.Timer = 0.0f;
            }
            else
            {
                this.ResetCoolTimer();
            }
        }

        public static Minigame OpenMinigame(Minigame prefab)
        {
            Minigame minigame = UnityEngine.Object.Instantiate(
                prefab, Camera.main.transform, false);
            minigame.transform.SetParent(Camera.main.transform, false);
            minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
            minigame.Begin(null);

            return minigame;
        }
    }
}
