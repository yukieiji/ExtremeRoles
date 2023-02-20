using UnityEngine;

using static ExtremeRoles.Module.AbilityButton.AbilityButtonBase;

namespace ExtremeRoles.Module.Interface
{
    public interface IAbilityBehavior
    {
        public float CoolTime { get; protected set; }

        public float ActiveTime { get; protected set; }

        public Sprite AbilityImg { get; protected set; }

        public string AbilityText { get; protected set; }

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
