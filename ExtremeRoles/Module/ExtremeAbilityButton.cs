using UnityEngine;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module
{
    public enum AbilityState : byte
    {
        None = 0,
        Stop,
        CoolDown,
        Ready,
        Activating,
    }

    public sealed class ExtremeAbilityButton
    {

        public const string AditionalInfoName = "ExRKillButtonAditionalInfo";

        public IAbilityBehavior Behavior { get; private set; }

        public AbilityState State { get; private set; }

        public float Timer { get; private set; } = 10.0f;

        public Transform Transform => this.button.transform;

        private ActionButton button;

        private KeyCode hotKey = KeyCode.F;

        private bool isShow = true;

        private IButtonAutoActivator activator;

        private readonly Color TimerOnColor = new Color(0F, 0.8F, 0F);

        private static GridArrange cachedArrange = null;

        public ExtremeAbilityButton(
            IAbilityBehavior behavior,
            IButtonAutoActivator activator,
            KeyCode hotKey)
        {
            this.State = AbilityState.CoolDown;
            this.activator = activator;
            this.Behavior = behavior;
            this.hotKey = hotKey;

            var killButton = FastDestroyableSingleton<HudManager>.Instance.KillButton;

            this.button = Object.Instantiate(
                killButton, killButton.transform.parent);
            PassiveButton passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener(
                (UnityEngine.Events.UnityAction)onClick);

            Transform info = this.button.transform.FindChild(AditionalInfoName);
            if (info != null)
            {
                info.gameObject.SetActive(false);
            }

            this.SetButtonShow(true);
            this.Behavior.Initialize(this.button);
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
        public bool IsAbilityReady() =>
            this.State == AbilityState.Ready;

        public void OnMeetingStart()
        {
            this.Behavior.ForceAbilityOff();
            this.SetButtonShow(false);
        }

        public void OnMeetingEnd()
        {
            this.setStatus(AbilityState.CoolDown);
            this.SetButtonShow(true);
        }

        public void SetButtonShow(bool isShow)
        {
            this.isShow = isShow;
            setActive(isShow);
        }

        public void SetHotKey(KeyCode newKey)
        {
            this.hotKey = newKey;
        }

        public void SetLabelToCrewmate()
        {
            if (FastDestroyableSingleton<HudManager>.Instance == null) { return; }

            var useButton = FastDestroyableSingleton<HudManager>.Instance.UseButton;

            Object.Destroy(
                this.button.buttonLabelText.fontMaterial);
            this.button.buttonLabelText.fontMaterial = UnityEngine.Object.Instantiate(
                useButton.buttonLabelText.fontMaterial, this.button.transform);
        }

        public void Update()
        {
            if (!this.isShow || this.button == null) { return; }

            setActive(this.activator.IsActive());
            if (!this.button.isActiveAndEnabled) { return; }

            AbilityState newState = this.Behavior.Update(this.State);
            if (newState != this.State)
            {
                setStatus(newState);
            }

            this.button.graphic.sprite = this.Behavior.AbilityImg;
            this.button.OverrideText(this.Behavior.AbilityText);

            if (this.Behavior.IsUse())
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
                    this.button.cooldownTimerText.color = Palette.EnabledColor;
                    this.button.SetCoolDown(0, this.Behavior.CoolTime);
                    return;
                case AbilityState.CoolDown:
                    // 白色でタイマーをすすめる
                    this.Timer -= Time.deltaTime;
                    this.button.cooldownTimerText.color = Palette.EnabledColor;

                    // クールダウンが明けた
                    if (this.Timer <= 0.0f)
                    {
                        this.setStatus(AbilityState.Ready);
                    }
                    break;
                case AbilityState.Activating:
                    // 緑色でタイマーをすすめる
                    this.Timer -= Time.deltaTime;
                    this.button.cooldownTimerText.color = TimerOnColor;

                    if (!this.Behavior.IsCanAbilityActiving())
                    {
                        this.Behavior.ForceAbilityOff();
                        return;
                    }
                    // 能力がアクティブが時間切れなので能力のリセット等を行う
                    if (this.Timer <= 0.0f)
                    {
                        this.Behavior.AbilityOff();
                        this.setStatus(AbilityState.CoolDown);
                    }
                    break;
                case AbilityState.Ready:
                    this.Timer = 0.0f;
                    if (Input.GetKeyDown(this.hotKey))
                    {
                        onClick();
                    }
                    break;
                default:
                    break;
            }

            this.button.SetCoolDown(
                this.Timer,
                this.State != AbilityState.Activating ?
                this.Behavior.CoolTime : this.Behavior.ActiveTime);
        }

        private void onClick()
        {
            if (this.Behavior.IsUse() &&
                this.Behavior.TryUseAbility(this.Timer, this.State, out AbilityState newState))
            {
                if (newState == AbilityState.CoolDown)
                {
                    this.Behavior.AbilityOff();
                }
                this.setStatus(newState);
            }
        }

        private void setActive(bool active)
        {
            this.button.gameObject.SetActive(active);
            this.button.graphic.enabled = active;
        }

        private void setStatus(AbilityState newState)
        {
            switch (newState)
            {
                case AbilityState.None:
                case AbilityState.Ready:
                    this.Timer = 0;
                    break;
                case AbilityState.CoolDown:
                    if (this.State != AbilityState.Stop)
                    {
                        this.Timer = this.Behavior.CoolTime;
                    }
                    break;
                case AbilityState.Activating:
                    this.Timer = this.Behavior.ActiveTime;
                    break;
                default:
                    break;
            }
            this.State = newState;
        }
    }
}
