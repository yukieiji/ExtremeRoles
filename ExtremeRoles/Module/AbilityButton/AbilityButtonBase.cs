using System;
using UnityEngine;

using ExtremeRoles.Performance;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Module.AbilityButton
{
    public abstract class AbilityButtonBase
    {
        public const string AditionalInfoName = "ExRKillButtonAditionalInfo";

        public Vector3 PositionOffset;
        protected string ButtonText = null;

        protected ActionButton Button;

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

            Transform info = this.Button.transform.FindChild(AditionalInfoName);
            if (info != null)
            {
                info.gameObject.SetActive(false);
            }

            this.CleanUp = abilityCleanUp;

            this.AbilityCheck = abilityCheck;
            if (this.AbilityCheck == null)
            {
                this.AbilityCheck = allTrue;
            }

            SetActive(false);
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
