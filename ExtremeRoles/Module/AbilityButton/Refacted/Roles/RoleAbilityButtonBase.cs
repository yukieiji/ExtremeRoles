using System;
using UnityEngine;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.AbilityButton.Refacted.Roles
{

    public abstract class RoleAbilityButtonBase : AbilityButtonBase
    {
        protected Func<bool> UseAbility;

        public RoleAbilityButtonBase(
            string buttonText,
            Func<bool> ability,
            Func<bool> canUse,
            Sprite sprite,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F) : base(
                sprite, buttonText, abilityCleanUp,
                canUse, abilityCheck, hotkey)
        {
            this.UseAbility = ability;
        }

        protected sealed override bool GetActivate()
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            return
                (
                    localPlayer.IsKillTimerEnabled ||
                    localPlayer.ForceKillTimerContinue ||
                    FastDestroyableSingleton<HudManager>.Instance.UseButton.isActiveAndEnabled
                ) &&
                localPlayer.Data != null &&
                MeetingHud.Instance != null &&
                ExileController.Instance != null &&
                !localPlayer.Data.IsDead;
        }
    }
}
