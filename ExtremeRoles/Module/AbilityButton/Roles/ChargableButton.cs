using System;
using UnityEngine;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.AbilityButton.Roles
{
    public sealed class ChargableButton : RoleAbilityButtonBase
    {
        private float currentCharge;
        private bool hasBaseCleanUp = false;

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
        {

            this.hasBaseCleanUp = HasCleanUp();

            if (this.hasBaseCleanUp)
            {
                this.AbilityCleanUp += addCleanUpFunction();
            }
            else
            {
                this.AbilityCleanUp = addCleanUpFunction();
            }
        }

        public override void SetAbilityActiveTime(float time)
        {
            this.currentCharge = time;
            base.SetAbilityActiveTime(time);
        }

        public override void ResetCoolTimer()
        {
            this.currentCharge = this.ActiveTime;
            base.ResetCoolTimer();
        }

        public override void ForceAbilityOff()
        {
            this.currentCharge = Mathf.Clamp(
                Timer, 0.0f, this.ActiveTime);
            base.ForceAbilityOff();
        }

        protected override void UpdateAbility()
        {
            if (CachedPlayerControl.LocalPlayer.PlayerControl.AllTasksCompleted())
            {
                this.currentCharge = this.ActiveTime;
            }
        }

        protected override bool IsEnable() =>
            (this.CanUse.Invoke() || this.State == AbilityState.Activating) &&
            this.currentCharge > 0f;

        protected override void DoClick()
        {

            if (!IsEnable()) { return; }

            if (this.State == AbilityState.Activating)
            {
                this.AbilityCleanUp.Invoke();
                this.SetStatus(AbilityState.Ready);
            }
            else if (
                this.Timer <= 0f &&
                this.IsAbilityReady() &&
                this.UseAbility.Invoke())
            {
                this.SetStatus(
                    this.HasCleanUp() && this.hasBaseCleanUp ?
                    AbilityState.Activating :
                    AbilityState.CoolDown);
            }
        }

        private Action addCleanUpFunction()
        {
            return () =>
            {
                this.currentCharge = this.Timer;
                if (this.currentCharge > 0.0f)
                {
                    this.SetStatus(AbilityState.Ready);
                }
                else
                {
                    ResetCoolTimer();
                }
            };
        }
    }
}
