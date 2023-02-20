using UnityEngine;

namespace ExtremeRoles.Module.Interface
{
    public interface IAbilityBehavior
    {
        public float CoolTime { get; }

        public float ActiveTime { get; }

        public Sprite AbilityImg { get; }

        public string AbilityText { get; }

        public void Initialize(ActionButton button);

        public void SetCoolTime(float newTime);

        public void SetActiveTime(float newTime);

        public void ForceAbilityOff();

        public void AbilityOff();

        public bool TryUseAbility(float timer, AbilityState curState, out AbilityState newState);

        public bool IsUse();

        public bool IsCanAbilityActiving();

        public AbilityState Update(AbilityState curState);
    }
}
