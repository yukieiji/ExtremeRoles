using System;
using UnityEngine;

namespace ExtremeRoles.Module.AbilityButton.Roles.Roles
{
    public sealed class ReclickableButton : RoleAbilityButtonBase
    {
        public ReclickableButton(
            string buttonText,
            Func<bool> ability,
            Func<bool> canUse,
            Sprite sprite,
            Action abilityCleanUp,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F) : base(
                buttonText,
                ability,
                canUse,
                sprite,
                abilityCleanUp,
                abilityCheck,
                hotkey)
        {

            var addCleanUpFunc = () => ResetCoolTimer();

            if (HasCleanUp())
            {
                this.AbilityCleanUp += addCleanUpFunc;
            }
            else
            {
                this.AbilityCleanUp = addCleanUpFunc;
            }
        }

        protected override bool IsEnable() =>
            this.CanUse.Invoke() || this.State == AbilityState.Activating;

        protected override void DoClick()
        {
            if (!this.IsEnable()) { return; }

            switch (this.State)
            {
                case AbilityState.Ready:
                    if (this.Timer <= 0.0f &&
                        this.UseAbility.Invoke())
                    {
                        this.SetStatus(AbilityState.Activating);
                    }
                    break;
                case AbilityState.Activating:
                    this.AbilityCleanUp.Invoke();
                    break;
                default:
                    break;
            }
        }

        protected override void UpdateAbility()
        { }
    }
}
