using System;
using UnityEngine;

namespace ExtremeRoles.Module.AbilityButton.Refacted.Roles
{
    public sealed class AbilityCountButton : RoleAbilityButtonBase
    {
        public int CurAbilityNum
        {
            get => this.abilityNum;
        }

        private int abilityNum = 0;
        private TMPro.TextMeshPro abilityCountText = null;
        private Action baseCleanUp;
        private Action reduceCountAction;

        public AbilityCountButton(
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
        {

            var coolTimerText = this.GetCoolDownText();

            this.abilityCountText = UnityEngine.Object.Instantiate(
                coolTimerText, coolTimerText.transform.parent);
            updateAbilityCountText();
            this.abilityCountText.enableWordWrapping = false;
            this.abilityCountText.transform.localScale = Vector3.one * 0.5f;
            this.abilityCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);

            this.reduceCountAction = this.reduceAbilityCountAction();

            if (HasCleanUp())
            {
                this.baseCleanUp = new Action(this.AbilityCleanUp);
                this.AbilityCleanUp += this.reduceCountAction;
            }
            else
            {
                this.baseCleanUp = null;
                this.AbilityCleanUp = this.reduceCountAction;
            }
        }

        public void UpdateAbilityCount(int newCount)
        {
            this.abilityNum = newCount;
            this.updateAbilityCountText();
            if (this.State == AbilityState.None)
            {
                this.SetStatus(AbilityState.CoolDown);
            }
        }

        public override void ForceAbilityOff()
        {
            this.SetStatus(AbilityState.Ready);
            this.baseCleanUp?.Invoke();
        }

        protected override bool IsEnable() =>
            this.CanUse.Invoke() && this.abilityNum > 0;

        protected override void DoClick()
        {
            if (this.IsEnable() &&
                this.Timer <= 0f &&
                this.abilityNum > 0 &&
                this.State == AbilityState.Ready &&
                this.UseAbility.Invoke())
            {
                if (this.HasCleanUp())
                {
                    this.SetStatus(AbilityState.Activating);
                }
                else
                {
                    this.reduceCountAction.Invoke();
                    this.ResetCoolTimer();
                }
            }
        }

        protected override void UpdateAbility()
        {
            if (this.abilityNum <= 0)
            {
                this.SetStatus(AbilityState.None);
            }
        }

        private Action reduceAbilityCountAction()
        {
            return () =>
            {
                --this.abilityNum;
                if (this.abilityCountText != null)
                {
                    updateAbilityCountText();
                }
            };
        }

        private void updateAbilityCountText()
        {
            this.abilityCountText.text = Helper.Translation.GetString("buttonCountText") + string.Format(
                Helper.Translation.GetString(OptionUnit.Shot.ToString()), this.abilityNum);
        }
    }
}
