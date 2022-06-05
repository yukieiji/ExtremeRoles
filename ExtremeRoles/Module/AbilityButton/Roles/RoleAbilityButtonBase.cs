using System;
using UnityEngine;


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
            if (PlayerControl.LocalPlayer.Data == null ||
                MeetingHud.Instance ||
                ExileController.Instance ||
                PlayerControl.LocalPlayer.Data.IsDead)
            {
                SetActive(false);
                return;
            }
            SetActive(HudManager.Instance.UseButton.isActiveAndEnabled);

            this.Button.graphic.sprite = this.ButtonSprite;
            this.Button.OverrideText(ButtonText);

            if (HudManager.Instance.UseButton != null)
            {
                Vector3 pos = HudManager.Instance.UseButton.transform.localPosition;
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
