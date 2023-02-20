using System;

using UnityEngine;

using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.AbilityBehavior
{
    public sealed class ReusableAbilityBehavior : IAbilityBehavior
    {
        public float CoolTime => this.coolTime;

        public float ActiveTime => this.activeTime;

        public Sprite AbilityImg => this.img;

        public string AbilityText => this.text;

        private float coolTime = 10.0f;
        private float activeTime = 0.0f;
        private Sprite img;
        private string text;

        private Func<bool> ability;
        private Func<bool> canUse;
        private Func<bool> canActivating;
        private Action forceAbilityOff;
        private Action abilityCleanUp;

        public ReusableAbilityBehavior(
            string text, Sprite img,
            Func<bool> canUse,
            Func<bool> ability,
            Func<bool> canActivating = null,
            Action abilityCleanUp = null,
            Action forceAbilityOff = null)
        {
            this.img = img;
            this.text = text;

            this.ability =  ability;
            this.canUse = canUse;

            this.forceAbilityOff = forceAbilityOff;
            this.abilityCleanUp = abilityCleanUp;

            this.canActivating = canActivating ?? new Func<bool>(() => { return true; });
        }

        public void Initialize(ActionButton button)
        {
            return;
        }

        public void AbilityOff()
        {
            this.abilityCleanUp?.Invoke();
        }

        public void ForceAbilityOff()
        {
            this.forceAbilityOff?.Invoke();
        }

        public bool IsCanAbilityActiving() => this.canActivating.Invoke();

        public bool IsUse() => this.canUse.Invoke();

        public void SetActiveTime(float newTime)
        {
            this.activeTime = newTime;
        }

        public void SetCoolTime(float newTime)
        {
            this.coolTime = newTime;
        }

        public bool TryUseAbility(
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

            newState = this.activeTime <= 0.0f ? 
                AbilityState.CoolDown : AbilityState.Activating;

            return true;
        }

        public AbilityState Update(AbilityState curState)
        {
            return curState;
        }
    }
}
