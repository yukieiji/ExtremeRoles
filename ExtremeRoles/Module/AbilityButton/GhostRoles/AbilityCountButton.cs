using System;
using UnityEngine;

using Hazel;


namespace ExtremeRoles.Module.AbilityButton.GhostRoles
{

    public class AbilityCountButton : GhostRoleAbilityButtonBase
    {
        public int CurAbilityNum
        {
            get => this.abilityNum;
        }

        private int abilityNum = 0;
        private TMPro.TextMeshPro abilityCountText = null;

        public AbilityCountButton(
            GhostRoleAbilityManager.AbilityType abilityType,
            Action<MessageWriter> ability,
            Func<bool> abilityPreCheck,
            Func<bool> canUse,
            Sprite sprite,
            Vector3 positionOffset,
            Action rpcHostCallAbility = null,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false) : base(
                abilityType,
                ability, abilityPreCheck,
                canUse, sprite, positionOffset,
                rpcHostCallAbility, abilityCleanUp,
                abilityCheck, hotkey, mirror)
        {
            this.abilityCountText = GameObject.Instantiate(
                this.Button.cooldownTimerText,
                this.Button.cooldownTimerText.transform.parent);
            updateAbilityCountText();
            this.abilityCountText.enableWordWrapping = false;
            this.abilityCountText.transform.localScale = Vector3.one * 0.5f;
            this.abilityCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);
        }

        public void UpdateAbilityCount(int newCount)
        {
            this.abilityNum = newCount;
            this.updateAbilityCountText();
        }


        private void updateAbilityCountText()
        {
            this.abilityCountText.text = Helper.Translation.GetString("buttonCountText") + string.Format(
                Helper.Translation.GetString(OptionUnit.Shot.ToString()), this.abilityNum);
        }
        private void reduceAbilityCount()
        {
            --this.abilityNum;
            if (this.abilityCountText != null)
            {
                updateAbilityCountText();
            }
        }

        protected override void AbilityButtonUpdate()
        {
            if (this.CanUse() && this.abilityNum > 0 && !this.IsComSabNow())
            {
                this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.EnabledColor;
                this.Button.graphic.material.SetFloat("_Desat", 0f);
            }
            else
            {
                this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.DisabledClear;
                this.Button.graphic.material.SetFloat("_Desat", 1f);
            }
            if (this.abilityNum == 0)
            {
                Button.SetCoolDown(0, this.CoolTime);
                return;
            }
            if (this.Timer >= 0)
            {
                bool abilityOn = this.IsHasCleanUp() && IsAbilityOn;

                if (abilityOn || (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable))
                {
                    this.Timer -= Time.deltaTime;
                }
                if (abilityOn)
                {
                    if (!this.AbilityCheck())
                    {
                        this.Timer = 0;
                        this.IsAbilityOn = false;
                    }
                }
            }

            if (this.Timer <= 0 && this.IsHasCleanUp() && IsAbilityOn)
            {
                this.IsAbilityOn = false;
                this.Button.cooldownTimerText.color = Palette.EnabledColor;
                this.CleanUp();
                this.reduceAbilityCount();
                this.ResetCoolTimer();
            }

            if (this.abilityNum > 0)
            {
                Button.SetCoolDown(
                    this.Timer,
                    (this.IsHasCleanUp() && this.IsAbilityOn) ? this.AbilityActiveTime : this.CoolTime);
            }
        }

        protected override void OnClickEvent()
        {
            if (!this.IsComSabNow() &&
                this.CanUse() &&
                this.Timer < 0f &&
                this.abilityNum > 0 &&
                !this.IsAbilityOn)
            {
                Button.graphic.color = this.DisableColor;

                if (this.UseAbility())
                {
                    if (this.IsHasCleanUp())
                    {
                        this.Timer = this.AbilityActiveTime;
                        Button.cooldownTimerText.color = this.TimerOnColor;
                        this.IsAbilityOn = true;
                    }
                    else
                    {
                        this.reduceAbilityCount();
                        this.ResetCoolTimer();
                    }
                }
            }
        }
    }
}
