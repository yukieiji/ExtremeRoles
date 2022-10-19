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
            Vector3 positionOffset,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false) : base(
                buttonText, canUse,
                sprite, positionOffset,
                abilityCleanUp, abilityCheck,
                hotkey, mirror)
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

            SetActive(localPlayer.IsKillTimerEnabled);

            var hudManager = FastDestroyableSingleton<HudManager>.Instance;

            this.Button.graphic.sprite = this.ButtonSprite;
            this.Button.OverrideText(ButtonText);

            if (hudManager.UseButton != null)
            {
                Vector3 pos = hudManager.UseButton.transform.localPosition;
                if (this.Mirror)
                {
                    pos = new Vector3(-pos.x, pos.y, pos.z);
                }
                this.Button.transform.localPosition = pos + PositionOffset;
            }

            AbilityButtonUpdate();

            if (Input.GetKeyDown(this.Hotkey))
            {
                OnClickEvent();
            }

        }
    }
}
