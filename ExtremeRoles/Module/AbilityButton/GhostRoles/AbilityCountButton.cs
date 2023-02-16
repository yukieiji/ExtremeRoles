using System;
using UnityEngine;

using ExtremeRoles.GhostRoles;

namespace ExtremeRoles.Module.AbilityButton.GhostRoles
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

            abilityCountText = UnityEngine.Object.Instantiate(
                cooldownText, cooldownText.transform.parent);
            updateAbilityCountText();
            abilityCountText.enableWordWrapping = false;
            abilityCountText.transform.localScale = Vector3.one * 0.5f;
            abilityCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);

            reduceCountAction = reduceAbilityCountAction();

            if (HasCleanUp())
            {
                baseCleanUp = new Action(AbilityCleanUp);
                AbilityCleanUp += reduceCountAction;
            }
            else
            {
                baseCleanUp = null;
                AbilityCleanUp = reduceCountAction;
            }
        }

        public void UpdateAbilityCount(int newCount)
        {
            abilityNum = newCount;
            updateAbilityCountText();
            if (State == AbilityState.None)
            {
                SetStatus(AbilityState.CoolDown);
            }
        }

        private void updateAbilityCountText()
        {
            abilityCountText.text = Helper.Translation.GetString("buttonCountText") + string.Format(
                Helper.Translation.GetString(OptionUnit.Shot.ToString()), abilityNum);
        }

        private Action reduceAbilityCountAction()
        {
            return () =>
            {
                --abilityNum;
                if (abilityCountText != null)
                {
                    updateAbilityCountText();
                }
            };
        }

        public override void ForceAbilityOff()
        {
            SetStatus(AbilityState.Ready);
            baseCleanUp?.Invoke();
        }

        protected override void DoClick()
        {
            if (IsEnable() &&
                Timer <= 0f &&
                abilityNum > 0 &&
                State == AbilityState.Ready &&
                UseAbility())
            {
                if (HasCleanUp())
                {
                    SetStatus(AbilityState.Activating);
                }
                else
                {
                    reduceCountAction.Invoke();
                    ResetCoolTimer();
                }
            }
        }

        protected override bool IsEnable() =>
            CanUse.Invoke() && abilityNum > 0 && !IsComSabNow();

        protected override void UpdateAbility()
        {
            if (abilityNum <= 0)
            {
                SetStatus(AbilityState.None);
            }
        }
    }
}
