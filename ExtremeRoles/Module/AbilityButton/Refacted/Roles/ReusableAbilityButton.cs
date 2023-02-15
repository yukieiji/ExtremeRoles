using System;
using UnityEngine;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.AbilityButton.Refacted.Roles
{
    public class ReusableAbilityButton : RoleAbilityButtonBase
    {
        public ReusableAbilityButton(
            string buttonText,
            Func<bool> ability,
            Func<bool> canUse,
            Sprite sprite,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F) : base(
                buttonText,
                ability,
                canUse,
                sprite,
                abilityCleanUp,
                abilityCheck,
                hotkey)
        { }

        protected override void UpdateAbility()
        {
            if (this.State == AbilityState.Activating)
            {
                if (!this.AbilityCheck.Invoke())
                {
                    this.SetStatus(AbilityState.Ready);
                }
            }
        }

        protected override bool IsEnable() => this.CanUse.Invoke();

        protected override void DoClick()
        {
            if(this.CanUse.Invoke() &&
                this.Timer <= 0f &&
                this.State == AbilityState.Ready &&
                this.UseAbility.Invoke())
            {
                SetStatus(
                    this.HasCleanUp() ?
                    AbilityState.Activating :
                    AbilityState.CoolDown);
            }
        }
    }
}
