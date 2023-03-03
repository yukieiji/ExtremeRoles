using ExtremeRoles.Performance;
using System;

using UnityEngine;

namespace ExtremeRoles.Module.AbilityBehavior
{
    public sealed class ChargableAbilityBehavior : AbilityBehaviorBase
    {
        private Func<bool> ability;
        private Func<bool> canUse;
        private Func<bool> canActivating;
        private Action abilityOff;
        private Action forceAbilityOff;

        private bool isActive;

        private float maxCharge;
        private float chargeTimer;
        private float currentCharge;

        public ChargableAbilityBehavior(
            string text, Sprite img,
            Func<bool> canUse,
            Func<bool> ability,
            Func<bool> canActivating = null,
            Action abilityOff = null,
            Action forceAbilityOff = null) : base(text, img)
        {
            this.ability =  ability;
            this.canUse = canUse;

            this.abilityOff = abilityOff;
            this.forceAbilityOff = forceAbilityOff ?? abilityOff;
            this.canActivating = canActivating ?? new Func<bool>(() => { return true; });

            this.isActive = false;
        }

        public override void Initialize(ActionButton button)
        {
            return;
        }

        public override void SetActiveTime(float time)
        {
            this.maxCharge = time;
            this.currentCharge = time;
            base.SetActiveTime(time);
        }

        public override void AbilityOff()
        {
            this.isActive = false;
            this.currentCharge = this.maxCharge;
            this.abilityOff?.Invoke();
        }

        public override void ForceAbilityOff()
        {
            this.currentCharge = Mathf.Clamp(
                this.chargeTimer, 0.0f, this.ActiveTime);
            this.isActive = false;
            this.forceAbilityOff?.Invoke();
        }

        public override bool IsCanAbilityActiving() => this.canActivating.Invoke();

        public override bool IsUse() =>
            (this.canUse.Invoke() || this.isActive) && this.currentCharge > 0f;

        public override bool TryUseAbility(
            float timer, AbilityState curState, out AbilityState newState)
        {
            newState = curState;
            bool result = false;
            if (curState == AbilityState.Activating)
            {
                this.ForceAbilityOff();
                newState = AbilityState.Ready;
                this.isActive = false;
                result = true;
            }
            else if (
                timer <= 0f &&
                curState == AbilityState.Ready &&
                this.ability.Invoke())
            {
                this.chargeTimer = this.currentCharge;
                this.isActive = true;
                base.SetActiveTime(this.chargeTimer);
                newState = this.ActiveTime <= 0.0f ?
                    AbilityState.CoolDown : AbilityState.Activating;
                result = true;
            }
            return result;
        }

        public override AbilityState Update(AbilityState curState)
        {
            if (this.isActive)
            {
                this.chargeTimer -= Time.deltaTime;
            }
            if (CachedPlayerControl.LocalPlayer.PlayerControl.AllTasksCompleted())
            {
                this.currentCharge = this.ActiveTime;
            }
            return curState;
        }
    }
}
