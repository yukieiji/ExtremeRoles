using System;
using UnityEngine;

using ExtremeRoles.Performance;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Module.AbilityButton
{
    public abstract class AbilityButtonBase
    {
        public Vector3 PositionOffset;
        protected string ButtonText = null;

        protected ActionButton Button;
        protected bool IsButtonActive = true;

        protected bool IsAbilityOn = false;
        protected float Timer = 10.0f;
        protected float AbilityActiveTime = 0.0f;
        protected float CoolTime = float.MaxValue;

        protected Func<bool> CanUse;
        protected Action CleanUp = null;
        protected Func<bool> AbilityCheck = null;
        protected Sprite ButtonSprite;

        protected KeyCode Hotkey;

        protected readonly Color DisableColor = new Color(1f, 1f, 1f, 0.3f);
        protected readonly Color TimerOnColor = new Color(0F, 0.8F, 0F);

        public AbilityButtonBase(
            string buttonText,
            Func<bool> canUse,
            Sprite sprite,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F)
        {

            this.ButtonText = buttonText;

            this.CanUse = canUse;

            this.ButtonSprite = sprite;

            var killButton = FastDestroyableSingleton<HudManager>.Instance.KillButton;

            this.Button = UnityEngine.Object.Instantiate(
                killButton, killButton.transform.parent);
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

            this.IsButtonActive = true;

            ButtonObjectSetActive(false);
            GameSystem.ReGridButtons();

            this.Hotkey = hotkey;

            bool allTrue() => true;
        }

        public float GetCurTime() => this.Timer;

        public Transform GetTransform() => this.Button.transform;

        public bool IsAbilityActive() => this.IsAbilityOn;

        public void ReplaceHotKey(KeyCode newKey)
        {
            this.Hotkey = newKey;
        }

        public void SetAbilityCoolTime(float time)
        {
            this.CoolTime = time;
        }

        public void SetActive(bool isActive)
        {
            this.IsButtonActive = isActive;
        }

        public void SetButtonText(string newText)
        {
            this.ButtonText = newText;
        }

        public void SetButtonImage(Sprite newImage)
        {
            this.ButtonSprite = newImage;
        }

        public void SetLabelToCrewmate()
        {
            if (FastDestroyableSingleton<HudManager>.Instance == null) { return; }

            var useButton = FastDestroyableSingleton<HudManager>.Instance.UseButton;

            UnityEngine.Object.Destroy(
                this.Button.buttonLabelText.fontMaterial);
            this.Button.buttonLabelText.fontMaterial = UnityEngine.Object.Instantiate(
                useButton.buttonLabelText.fontMaterial, this.Button.transform);
        }

        protected bool IsHasCleanUp() => this.CleanUp != null;

        protected void ButtonObjectSetActive(bool isActive)
        {
            this.Button.gameObject.SetActive(isActive);
            this.Button.graphic.enabled = isActive;
        }

        public virtual void ForceAbilityOff()
        {
            this.IsAbilityOn = false;
            this.Button.cooldownTimerText.color = Palette.EnabledColor;
        }

        public virtual void SetAbilityActiveTime(float time)
        {
            this.AbilityActiveTime = time;
        }

        public virtual void ResetCoolTimer()
        {
            this.Timer = this.CoolTime;
        }

        public abstract void Update();

        protected abstract void OnClickEvent();
    }
}
