using System;
using UnityEngine;

using ExtremeRoles.Performance;
using static Il2CppSystem.Threading.ThreadLocal<T>;

namespace ExtremeRoles.Module.AbilityButton.Refacted.Roles
{

    public abstract class RoleAbilityButtonBase : AbilityButtonBase
    {
        protected Func<bool> UseAbility;
        protected Func<bool> canUse = () => true;
        protected Func<bool> abilityCheck = () => true;

        public RoleAbilityButtonBase(
            string buttonText,
            Func<bool> ability,
            Func<bool> canUse,
            Sprite sprite,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F) : base(
                sprite, buttonText, abilityCleanUp, hotkey)
        {
            this.UseAbility = ability;

            this.canUse = canUse ?? this.canUse;
            this.abilityCheck = abilityCheck ?? this.abilityCheck;
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
