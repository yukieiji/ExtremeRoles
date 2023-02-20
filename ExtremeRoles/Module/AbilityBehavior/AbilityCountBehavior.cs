using System;

using UnityEngine;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Module.AbilityBehavior
{
    public sealed class AbilityCountBehavior : AbilityBehaviorBase
    {
        public int AbilityCount { get; private set; }

        private bool isReduceOnActive;

        private bool isUpdate = false;
        private Func<bool> ability;
        private Func<bool> canUse;
        private Func<bool> canActivating;
        private Action forceAbilityOff;
        private Action abilityCleanUp;

        private TMPro.TextMeshPro abilityCountText = null;
        private string buttonTextFormat = "buttonCountText";

        public AbilityCountBehavior(
            string text, Sprite img,
            Func<bool> canUse,
            Func<bool> ability,
            Func<bool> canActivating = null,
            Action abilityCleanUp = null,
            Action forceAbilityOff = null,
            bool isReduceOnActive = false) : base(text, img)
        {
            this.ability =  ability;
            this.canUse = canUse;
            this.isReduceOnActive = isReduceOnActive;

            this.forceAbilityOff = forceAbilityOff;
            this.abilityCleanUp = abilityCleanUp;

            this.canActivating = canActivating ?? new Func<bool>(() => { return true; });
        }

        public override void Initialize(ActionButton button)
        {
            var coolTimerText = button.cooldownTimerText;

            this.abilityCountText = UnityEngine.Object.Instantiate(
                coolTimerText, coolTimerText.transform.parent);
            this.abilityCountText.enableWordWrapping = false;
            this.abilityCountText.transform.localScale = Vector3.one * 0.5f;
            this.abilityCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);
            updateAbilityCountText();
        }

        public override void AbilityOff()
        {
            if (!this.isReduceOnActive)
            {
                this.reduceAbilityCount();
            }
            this.abilityCleanUp?.Invoke();
        }

        public override void ForceAbilityOff()
        {
            this.forceAbilityOff?.Invoke();
        }

        public override bool IsCanAbilityActiving() => this.canActivating.Invoke();

        public override bool IsUse()
            => this.canUse.Invoke() && this.AbilityCount > 0;

        public override bool TryUseAbility(
            float timer, AbilityState curState, out AbilityState newState)
        {
            newState = curState;

            if (timer > 0 || 
                curState != AbilityState.Ready || 
                this.AbilityCount <= 0)
            {
                return false;
            }

            if (!this.ability.Invoke())
            {
                return false;
            }

            if (this.isReduceOnActive)
            {
                this.reduceAbilityCount();
            }

            newState = this.ActiveTime <= 0.0f ? 
                AbilityState.CoolDown : AbilityState.Activating;

            return true;
        }

        public override AbilityState Update(AbilityState curState)
        {
            if (this.isUpdate)
            {
                this.isUpdate = false;
                return AbilityState.CoolDown;
            }
            
            return this.AbilityCount > 0 ? curState : AbilityState.None;
        }

        public void SetAbilityCount(int newAbilityNum)
        {
            this.AbilityCount = newAbilityNum;
            this.isUpdate = true;
        }

        public void SetButtonTextFormat(string newTextFormat)
        {
            this.buttonTextFormat = newTextFormat;
        }

        private void reduceAbilityCount()
        {
            --this.AbilityCount;
            if (this.abilityCountText != null)
            {
                updateAbilityCountText();
            }
        }

        private void updateAbilityCountText()
        {
            this.abilityCountText.text = string.Format(
                Translation.GetString(this.buttonTextFormat),
                this.AbilityCount);
        }
    }
}
