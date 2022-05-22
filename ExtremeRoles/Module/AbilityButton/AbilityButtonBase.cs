using System;
using UnityEngine;

namespace ExtremeRoles.Module.AbilityButton
{
    public abstract class AbilityButtonBase
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
        protected Sprite ButtonSprite;

        protected bool Mirror;
        protected KeyCode Hotkey;

        protected readonly Color DisableColor = new Color(1f, 1f, 1f, 0.3f);
        protected readonly Color TimerOnColor = new Color(0F, 0.8F, 0F);

        public AbilityButtonBase(
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

            this.Mirror = mirror;

            this.ButtonSprite = sprite;
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

            this.Hotkey = hotkey;

            bool allTrue() => true;
        }

        public float GetCurTime() => this.Timer;

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

        public void SetLabelToCrewmate()
        {
            if (HudManager.Instance == null) { return; }

            var useButton = HudManager.Instance.UseButton;
        public Transform GetTransform() => this.Button.transform;

            UnityEngine.Object.Destroy(
                this.Button.buttonLabelText.fontMaterial);
            this.Button.buttonLabelText.fontMaterial = UnityEngine.Object.Instantiate(
                useButton.buttonLabelText.fontMaterial, this.Button.transform);
        }

        protected bool IsHasCleanUp() => this.CleanUp != null;

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
