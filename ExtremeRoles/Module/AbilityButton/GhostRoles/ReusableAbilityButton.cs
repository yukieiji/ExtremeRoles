using System;
using UnityEngine;

using Hazel;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Module.AbilityButton.GhostRoles
{

    public class ReusableAbilityButton : GhostRoleAbilityButtonBase
    {

        public ReusableAbilityButton(
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
        { }

        protected override void AbilityButtonUpdate()
        {
            if (this.CanUse() && !this.IsComSabNow())
            {
                this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.EnabledColor;
                this.Button.graphic.material.SetFloat("_Desat", 0f);
            }
            else
            {
                this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.DisabledClear;
                this.Button.graphic.material.SetFloat("_Desat", 1f);
            }
            
            if (this.Timer >= 0)
            {
                bool abilityOn = this.IsHasCleanUp() && IsAbilityOn;

                PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

                if (abilityOn ||
                    localPlayer.IsKillTimerEnabled ||
                    localPlayer.ForceKillTimerContinue)
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
                this.ResetCoolTimer();
            }

            Button.SetCoolDown(
                this.Timer,
                (this.IsHasCleanUp() && this.IsAbilityOn) ? this.AbilityActiveTime : this.CoolTime);
        }

        protected override void OnClickEvent()
        {
            if (!this.IsComSabNow() &&
                this.CanUse() &&
                this.Timer < 0f &&
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
                        this.ResetCoolTimer();
                    }
                }
            }
        }
    }
}
