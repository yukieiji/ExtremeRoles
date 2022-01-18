using System;
using UnityEngine;

namespace ExtremeRoles.Module.RoleAbilityButton
{

    public abstract class RoleAbilityButtonBase
    {
        public Vector3 PositionOffset;
        public string ButtonText = null;

        protected ActionButton Button;

        protected bool IsAbilityOn = false;
        protected float Timer = 10.0f;
        protected float AbilityActiveTime = 0.0f;
        protected float CoolTime = float.MaxValue;

        protected Func<bool> UseAbility;
        protected Func<bool> CanUse;
        protected Action CleanUp = null;
        protected Func<bool> AbilityCheck = null;

        protected readonly Color DisableColor = new Color(1f, 1f, 1f, 0.3f);
        protected readonly Color TimerOnColor = new Color(0F, 0.8F, 0F);

        private bool mirror;
        private KeyCode hotkey;
        private Sprite buttonSprite;

        public RoleAbilityButtonBase(
            string buttonText,
            Func<bool> ability,
            Func<bool> canUse,
            Sprite sprite,
            Vector3 positionOffset,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false)
        {

            this.ButtonText = buttonText;

            this.UseAbility = ability;
            this.CanUse = canUse;

            this.mirror = mirror;

            this.buttonSprite = sprite;
            this.PositionOffset = positionOffset;

            this.Button = UnityEngine.Object.Instantiate(
                HudManager.Instance.KillButton,
                HudManager.Instance.KillButton.transform.parent);
            PassiveButton button = Button.GetComponent<PassiveButton>();
            button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            button.OnClick.AddListener(
                (UnityEngine.Events.UnityAction)OnClickEvent);

            this.CleanUp = abilityCleanUp;

            this.AbilityCheck = abilityCheck;
            if (this.AbilityCheck == null)
            {
                this.AbilityCheck = allTrue;
            }

            SetActive(false);

            this.hotkey = hotkey;

            bool allTrue() => true;
        }

        protected abstract void OnClickEvent();

        protected abstract void AbilityButtonUpdate();

        public void ForceAbilityOff()
        {
            this.IsAbilityOn = false;
            this.Button.cooldownTimerText.color = Palette.EnabledColor;
        }

        public void SetAbilityActiveTime(float time)
        {
            this.AbilityActiveTime = time;
        }

        public void SetAbilityCoolTime(float time)
        {
            this.CoolTime = time;
        }

        public void ResetCoolTimer()
        {
            this.Timer = this.CoolTime;
        }

        public void SetLabelToCrewmate()
        {
            if (HudManager.Instance == null) { return; }

            var useButton = HudManager.Instance.UseButton;
            
            UnityEngine.Object.Destroy(
                this.Button.buttonLabelText.fontMaterial);
            this.Button.buttonLabelText.fontMaterial = UnityEngine.Object.Instantiate(
                useButton.buttonLabelText.fontMaterial, this.Button.transform);
        }

        public void SetActive(bool isActive)
        {
            if (isActive)
            {
                Button.gameObject.SetActive(true);
                Button.graphic.enabled = true;
            }
            else
            {
                Button.gameObject.SetActive(false);
                Button.graphic.enabled = false;
            }
        }

        public void ReplaceHotKey(KeyCode newKey)
        {
            this.hotkey = newKey;
        }


        public void Update()
        {
            if (PlayerControl.LocalPlayer.Data == null ||
                MeetingHud.Instance ||
                ExileController.Instance ||
                PlayerControl.LocalPlayer.Data.IsDead)
            {
                SetActive(false);
                return;
            }
            SetActive(HudManager.Instance.UseButton.isActiveAndEnabled);

            this.Button.graphic.sprite = this.buttonSprite;
            this.Button.OverrideText(ButtonText);

            if (HudManager.Instance.UseButton != null)
            {
                Vector3 pos = HudManager.Instance.UseButton.transform.localPosition;
                if (this.mirror)
                {
                    pos = new Vector3(-pos.x, pos.y, pos.z);
                }
                this.Button.transform.localPosition = pos + PositionOffset;
            }

            AbilityButtonUpdate();

            if (Input.GetKeyDown(this.hotkey))
            {
                OnClickEvent();
            }

        }


        protected bool IsHasCleanUp() => this.CleanUp != null;

    }
}
