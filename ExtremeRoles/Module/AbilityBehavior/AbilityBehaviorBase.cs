using UnityEngine;

namespace ExtremeRoles.Module.AbilityBehavior
{
    public abstract class AbilityBehaviorBase
    {
        public float CoolTime { get; private set; }
        public float ActiveTime { get; private set; }

        public Sprite AbilityImg { get; private set; }
        public string AbilityText { get; private set; }

        public void SetActiveTime(float newTime)
        {
            this.ActiveTime = newTime;
        }

        public void SetCoolTime(float newTime)
        {
            this.CoolTime = newTime;
        }

        public void SetButtonImage(Sprite image)
        {
            this.AbilityImg = image;
        }

        public void SetButtonText(string text)
        {
            this.AbilityText = text;
        }

        public abstract void Initialize(ActionButton button);

        public abstract void ForceAbilityOff();

        public abstract void AbilityOff();

        public abstract bool TryUseAbility(float timer, AbilityState curState, out AbilityState newState);

        public abstract bool IsUse();

        public abstract bool IsCanAbilityActiving();

        public abstract AbilityState Update(AbilityState curState);
    }
}
