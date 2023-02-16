using System;

using UnityEngine;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.AbilityButton
{
    public abstract class AbilityButtonBase
    {
        public enum AbilityState : byte
        {
            None = 0,
            CoolDown,
            Ready,
            Activating,
        }

        public const string AditionalInfoName = "ExRKillButtonAditionalInfo";

        public float Timer { get; private set; }
        public AbilityState State { get; private set; } = AbilityState.CoolDown;
        public Transform Transform => this.button.transform;

        public float CoolTime { get; private set; } = 10.0f;
        public float ActiveTime { get; private set; } = 0.0f;

        private bool isShow = true;

        protected Func<bool> AbilityCheck = () => true;
        protected Func<bool> CanUse = () => true;
        protected Action AbilityCleanUp = null;
        private ActionButton button;

        private KeyCode hotKey = KeyCode.F;

        private Sprite buttonImg;
        private string buttonText;

        private readonly Color TimerOnColor = new Color(0F, 0.8F, 0F);

        private static GridArrange cachedArrange = null;

        public AbilityButtonBase(
            Sprite img,
            string buttonText,
            Action cleanUp,
            Func<bool> canUse,
            Func<bool> abilityCheck,
            KeyCode hotKey)
        {
            this.State = AbilityState.CoolDown;

            var killButton = FastDestroyableSingleton<HudManager>.Instance.KillButton;

            this.button = UnityEngine.Object.Instantiate(
                killButton, killButton.transform.parent);
            PassiveButton passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener(
                (UnityEngine.Events.UnityAction)DoClick);

            this.AbilityCleanUp = cleanUp;
            this.buttonText = buttonText;
            this.buttonImg = img;
            this.CanUse = canUse ?? this.CanUse;
            this.AbilityCheck = abilityCheck ?? this.AbilityCheck;
            this.hotKey = hotKey;
            this.button.graphic.sprite = this.buttonImg;

            Transform info = this.button.transform.FindChild(AditionalInfoName);
            if (info != null)
            {
                info.gameObject.SetActive(false);
            }

            SetButtonShow(false);
            ReGridButtons();
        }

        public static void ReGridButtons()
        {
            if (!FastDestroyableSingleton<HudManager>.Instance) { return; }

            if (cachedArrange == null)
            {
                var useButton = FastDestroyableSingleton<HudManager>.Instance.UseButton;
                cachedArrange = useButton.transform.parent.gameObject.GetComponent<GridArrange>();
            }
            cachedArrange.ArrangeChilds();
        }

        public bool IsAbilityActive() =>
            this.State == AbilityState.Activating;

        public void SetButtonShow(bool isShow)
        {
            this.isShow = isShow;
            setActive(isShow);
        }

        public void SetButtonImg(Sprite newImg)
        {
            this.buttonImg = newImg;
        }

        public void SetButtonText(string newText)
        {
            this.buttonText = newText;
        }

        public void SetCoolTime(float time)
        {
            this.CoolTime = time;
        }

        public void SetHotKey(KeyCode newKey)
        {
            this.hotKey = newKey;
        }

        public void SetLabelToCrewmate()
        {
            if (FastDestroyableSingleton<HudManager>.Instance == null) { return; }

            var useButton = FastDestroyableSingleton<HudManager>.Instance.UseButton;

            UnityEngine.Object.Destroy(
                this.button.buttonLabelText.fontMaterial);
            this.button.buttonLabelText.fontMaterial = UnityEngine.Object.Instantiate(
                useButton.buttonLabelText.fontMaterial, this.button.transform);
        }

        public void Update()
        {
            if (!this.isShow || this.button == null) { return; }
            
            setActive(GetActivate());
            if (!this.button.isActiveAndEnabled) { return; }

            UpdateAbility();

            this.button.graphic.sprite = this.buttonImg;
            this.button.OverrideText(this.buttonText);

            if (this.IsEnable())
            {
                this.button.graphic.color = this.button.buttonLabelText.color = Palette.EnabledColor;
                this.button.graphic.material.SetFloat("_Desat", 0f);
            }
            else
            {
                this.button.graphic.color = this.button.buttonLabelText.color = Palette.DisabledClear;
                this.button.graphic.material.SetFloat("_Desat", 1f);
            }

            switch (this.State)
            {
                case AbilityState.None:
                    this.button.SetCoolDown(0, this.CoolTime);
                    return;
                case AbilityState.CoolDown:
                    // 白色でタイマーをすすめる
                    this.Timer -= Time.deltaTime;
                    this.button.cooldownTimerText.color = Palette.EnabledColor;
                    
                    // クールダウンが明けた
                    if (this.Timer <= 0.0f)
                    {
                        this.SetStatus(AbilityState.Ready);
                    }
                    break;
                case AbilityState.Activating:
                    // 緑色でタイマーをすすめる
                    this.Timer -= Time.deltaTime;
                    this.button.cooldownTimerText.color = TimerOnColor;
                    
                    if (!this.AbilityCheck.Invoke())
                    {
                        this.ForceAbilityOff();
                        return;
                    }
                    // 能力がアクティブが時間切れなので能力のリセット等を行う
                    if (this.Timer <= 0.0f)
                    {
                        this.AbilityCleanUp?.Invoke();
                        this.ResetCoolTimer();
                    }
                    break;
                case AbilityState.Ready:
                    this.Timer = 0.0f;
                    if (Input.GetKeyDown(this.hotKey))
                    {
                        DoClick();
                    }
                    break;
                default:
                    break;
            }

            this.button.SetCoolDown(
                this.Timer,
                this.State != AbilityState.Activating ? 
                this.CoolTime : this.ActiveTime);
        }

        public virtual void ResetCoolTimer()
        {
            this.SetStatus(AbilityState.CoolDown);
        }

        public virtual void SetAbilityActiveTime(float time)
        {
            this.ActiveTime = time;
        }

        public virtual void ForceAbilityOff()
        {
            this.SetStatus(AbilityState.Ready);
            this.AbilityCleanUp?.Invoke();
        }

        protected void SetStatus(AbilityState newState)
        {
            this.State = newState;
            switch (newState)
            {
                case AbilityState.None:
                case AbilityState.Ready:
                    this.Timer = 0;
                    break;
                case AbilityState.CoolDown:
                    this.Timer = this.CoolTime;
                    break;
                case AbilityState.Activating:
                    this.Timer = this.ActiveTime;
                    break;
                default:
                    break;
            }
        }

        private void setActive(bool active)
        {
            this.button.gameObject.SetActive(active);
            this.button.graphic.enabled = active;
        }

        protected bool HasCleanUp() => this.AbilityCleanUp != null;
        protected TMPro.TextMeshPro GetCoolDownText() => this.button.cooldownTimerText;

        protected abstract bool GetActivate();
        protected abstract void UpdateAbility();
        protected abstract bool IsEnable();

        protected abstract void DoClick();
    }
}

