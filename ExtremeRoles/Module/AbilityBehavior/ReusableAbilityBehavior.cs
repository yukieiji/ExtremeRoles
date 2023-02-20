using System;

using UnityEngine;

namespace ExtremeRoles.Module.AbilityBehavior
{
    public sealed class ReusableAbilityBehavior : AbilityBehaviorBase
    {
        private Func<bool> ability;
        private Func<bool> canUse;
        private Func<bool> canActivating;
        private Action forceAbilityOff;
        private Action abilityOff;

        public ReusableAbilityBehavior(
            string text, Sprite img,
            Func<bool> canUse,
            Func<bool> ability,
            Func<bool> canActivating = null,
            Action abilityOff = null,
            Action forceAbilityOff = null) : base(text, img)
        {
            this.ability =  ability;
            this.canUse = canUse;

            this.forceAbilityOff = forceAbilityOff;
            this.abilityOff = abilityOff;

            this.canActivating = canActivating ?? new Func<bool>(() => { return true; });
        }

        public override void Initialize(ActionButton button)
        {
            return;
        }

        public override void AbilityOff()
        {
            this.abilityOff?.Invoke();
        }

        public override void ForceAbilityOff()
        {
            this.forceAbilityOff?.Invoke();
        }

        public override bool IsCanAbilityActiving() => this.canActivating.Invoke();

        public override bool IsUse() => this.canUse.Invoke();

        public override bool TryUseAbility(
            float timer, AbilityState curState, out AbilityState newState)
        {
            newState = curState;

            if (timer > 0 || curState != AbilityState.Ready)
            {
                return false;
            }

            if (!this.ability.Invoke())
            {
                return false;
            }

            newState = this.ActiveTime <= 0.0f ? 
                AbilityState.CoolDown : AbilityState.Activating;

            return true;
        }

        public override AbilityState Update(AbilityState curState)
        {
            return curState;
        }
    }
}
