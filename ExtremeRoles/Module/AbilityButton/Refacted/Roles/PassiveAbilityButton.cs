using System;
using UnityEngine;

namespace ExtremeRoles.Module.AbilityButton.Refacted.Roles
{
    public sealed class PassiveAbilityButton : RoleAbilityButtonBase
    {
        private string activateButtonText;
        private string deactivateButtonText;

        private Sprite deactivateButtonSprite;
        private Sprite activateButtonSprite;

        public PassiveAbilityButton(
            string activateButtonText,
            string deactivateButtonText,
            Func<bool> ability,
            Func<bool> canUse,
            Sprite activateSprite,
            Sprite deactivateSprite,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F) : base(
                activateButtonText,
                ability,
                canUse,
                activateSprite,
                abilityCleanUp,
                abilityCheck,
                hotkey)
        {
            this.activateButtonText = activateButtonText;
            this.deactivateButtonText = deactivateButtonText;

            this.activateButtonSprite = activateSprite;
            this.deactivateButtonSprite = deactivateSprite;

            if (HasCleanUp())
            {
                this.AbilityCleanUp += this.createPassiveAbilityOffAction();
            }
            else
            {
                this.AbilityCleanUp = this.createPassiveAbilityOffAction();
            }
        }

        protected override bool IsEnable() =>
            (this.CanUse.Invoke() || this.State == AbilityState.Activating) && 
            this.Timer <= 0.0f;

        protected override void DoClick()
        {
            if (!this.IsEnable()) { return; }

            switch (this.State)
            {
                case AbilityState.Ready:
                    if (this.UseAbility.Invoke())
                    {
                        passiveAbilityOn();
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

        private void passiveAbilityOn()
        {
            this.SetButtonImg(this.deactivateButtonSprite);
            this.SetButtonText(this.deactivateButtonText);
            this.SetStatus(AbilityState.Activating);
        }

        private Action createPassiveAbilityOffAction()
        {
            return () =>
            {
                this.SetButtonImg(this.activateButtonSprite);
                this.SetButtonText(this.activateButtonText);
                this.ResetCoolTimer();
            };
        }
    }
}
