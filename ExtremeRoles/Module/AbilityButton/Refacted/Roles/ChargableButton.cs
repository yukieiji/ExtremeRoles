using System;
using UnityEngine;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.AbilityButton.Refacted.Roles
{
    public sealed class ChargableButton : RoleAbilityButtonBase
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
        {
            if (HasCleanUp())
            {
                this.AbilityCleanUp += this.addCleanUpFunction();
            }
            else
            {
                this.AbilityCleanUp = this.addCleanUpFunction();
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
                this.Timer, 0.0f, this.ActiveTime);
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

            if (!this.IsEnable()) { return; }

            if (this.State == AbilityState.Activating)
            {
                this.AbilityCleanUp.Invoke();
                this.SetStatus(AbilityState.Ready);
            }
            else if (
                this.Timer <= 0f &&
                this.State == AbilityState.Ready &&
                this.UseAbility.Invoke())
            {
                this.SetStatus(
                    this.HasCleanUp() ? 
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
                    this.ResetCoolTimer();
                }
            };
        }
    }
}
