using System;
using UnityEngine;

using ExtremeRoles.GhostRoles;

namespace ExtremeRoles.Module.AbilityButton.Refacted.GhostRoles
{

    public sealed class AbilityCountButton : GhostRoleAbilityButtonBase
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
            AbilityType abilityType,
            Action<RPCOperator.RpcCaller> ability,
            Func<bool> abilityPreCheck,
            Func<bool> canUse,
            Sprite sprite,
            Action rpcHostCallAbility = null,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F) : base(
                abilityType,
                ability, abilityPreCheck,
                canUse, sprite,
                rpcHostCallAbility, abilityCleanUp,
                abilityCheck, hotkey)
        {

            var cooldownText = GetCoolDownText();

            this.abilityCountText = GameObject.Instantiate(
                cooldownText, cooldownText.transform.parent);
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

        private void updateAbilityCountText()
        {
            this.abilityCountText.text = Helper.Translation.GetString("buttonCountText") + string.Format(
                Helper.Translation.GetString(OptionUnit.Shot.ToString()), this.abilityNum);
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

        public override void ForceAbilityOff()
        {
            this.SetStatus(AbilityState.Ready);
            this.baseCleanUp?.Invoke();
        }

        protected override void DoClick()
        {
            if (!this.IsComSabNow() &&
                this.CanUse() &&
                this.Timer < 0f &&
                this.abilityNum > 0 &&
                this.State == AbilityState.Ready &&
                this.UseAbility())
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

        protected override bool IsEnable() =>
            this.CanUse.Invoke() && this.abilityNum > 0 && !this.IsComSabNow();

        protected override void UpdateAbility()
        {
            if (this.abilityNum <= 0)
            {
                this.SetStatus(AbilityState.None);
            }
        }
    }
}
