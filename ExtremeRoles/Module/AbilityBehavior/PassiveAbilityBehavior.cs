using System;

using UnityEngine;

namespace ExtremeRoles.Module.AbilityBehavior
{
    public sealed class PassiveAbilityBehavior : AbilityBehaviorBase
    {
        private Func<bool> ability;
        private Func<bool> canUse;
        private Func<bool> canActivating;
        private Action abilityOff;

        private bool isActive;

        public PassiveAbilityBehavior(
            string text, Sprite img,
            Func<bool> canUse,
            Func<bool> ability,
            Func<bool> canActivating = null,
            Action abilityOff = null) : base(text, img)
        {
            this.ability =  ability;
            this.canUse = canUse;

            this.abilityOff = abilityOff;
            this.canActivating = canActivating ?? new Func<bool>(() => { return true; });

            this.isActive = false;
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
            this.AbilityOff();
        }

        public override bool IsCanAbilityActiving() => this.canActivating.Invoke();

        public override bool IsUse()  => this.canUse.Invoke();

        public override bool TryUseAbility(
            float timer, AbilityState curState, out AbilityState newState)
        {
            newState = curState;

            if (timer > 0 || curState != AbilityState.Ready)
            {
                return false;
            }

            if (this.isActive)
            {
                this.AbilityOff();
                swapCoolTime();
            }
            else
            {
                if (!this.ability.Invoke())
                {
                    return false;
                }
                swapCoolTime();
            }

            this.isActive = !this.isActive;
            newState = AbilityState.CoolDown;

            return true;
        }

        public override AbilityState Update(AbilityState curState)
        {
            return curState;
        }

        private void swapCoolTime()
        {
            float curCoolTime = this.CoolTime;
            float curActiveTime = this.ActiveTime;

            this.SetActiveTime(curCoolTime);
            this.SetCoolTime(curActiveTime);
        }
    }
}
