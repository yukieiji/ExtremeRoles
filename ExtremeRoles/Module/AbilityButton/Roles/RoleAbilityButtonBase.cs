using System;
using UnityEngine;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.AbilityButton.Roles
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
                buttonText, canUse,
                sprite, abilityCleanUp,
                abilityCheck,
                hotkey)
        {
            this.UseAbility = ability;
        }

        protected abstract void AbilityButtonUpdate();


        public sealed override void Update()
        {
            if (this.Button == null) { return; }

            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            if (localPlayer.Data == null ||
                MeetingHud.Instance ||
                ExileController.Instance ||
                localPlayer.Data.IsDead)
            {
                SetActive(false);
                return;
            }

            var hudManager = FastDestroyableSingleton<HudManager>.Instance;

            SetActive(
                localPlayer.IsKillTimerEnabled || 
                localPlayer.ForceKillTimerContinue ||
                hudManager.UseButton.isActiveAndEnabled);

            this.Button.graphic.sprite = this.ButtonSprite;
            this.Button.OverrideText(ButtonText);

            AbilityButtonUpdate();

            if (Input.GetKeyDown(this.Hotkey))
            {
                OnClickEvent();
            }

        }
    }
}
