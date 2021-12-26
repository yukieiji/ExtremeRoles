using System;
using UnityEngine;

namespace ExtremeRoles.Module
{
    public class RoleAbilityButton
    {
        public ActionButton Button;
        public Vector3 PositionOffset;
        public Sprite ButtonSprite;

        public Vector3 LocalScale = Vector3.one;
        public bool ShowButtonText = true;
        public string ButtonText = null;

        private bool mirror;
        private Action useAbility;
        private Func<bool> canUse;
        private KeyCode hotkey;

        private Action cleanUp = null;

        private bool isAbilityOn = false;

        private float timer = 10.0f;
        private float coolTime = float.MaxValue;
        private float abilityActiveTime = 0.0f;

        public RoleAbilityButton(
            Action ability,
            Func<bool> canUse,
            Sprite sprite,
            Vector3 positionOffset,
            Action abilityCleanUp=null,
            KeyCode hotkey=KeyCode.F,
            bool mirror = false)
        {
            
            this.PositionOffset = positionOffset;
            this.ButtonSprite = sprite;

            this.useAbility = ability;
            this.cleanUp = abilityCleanUp;
            this.canUse = canUse;
            this.mirror = mirror;
            this.hotkey = hotkey;
            
            this.Button = UnityEngine.Object.Instantiate(
                HudManager.Instance.KillButton,
                HudManager.Instance.KillButton.transform.parent);
            PassiveButton button = Button.GetComponent<PassiveButton>();
            button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            button.OnClick.AddListener(
                (UnityEngine.Events.UnityAction)OnClickEvent);

            LocalScale = Button.transform.localScale;

            SetActive(false);

        }

        public void OnClickEvent()
        {
            if (this.timer < 0f && canUse())
            {
                Button.graphic.color = new Color(1f, 1f, 1f, 0.3f);
                this.useAbility();
                ResetCoolTimer();

                if (this.isHasCleanUp() && !this.isAbilityOn)
                {
                    this.timer = this.abilityActiveTime;
                    Button.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    this.isAbilityOn = true;
                }
            }
        }
        public void SetAbilityCoolTime(float time)
        {
            this.coolTime = time;
        }
        public void SetAbilityActiveTime(float time)
        {
            this.abilityActiveTime = time;
        }

        public void ResetCoolTimer()
        {
            this.timer = this.coolTime;
        }

        public void UpdateAbility(Action newAbility)
        {
            this.useAbility = newAbility;
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

            this.Button.graphic.sprite = this.ButtonSprite;
            if (this.ShowButtonText && this.ButtonText != "")
            {
                this.Button.OverrideText(ButtonText);
            }

            this.Button.buttonLabelText.enabled = ShowButtonText; // Only show the text if it's a kill button
            
            if (HudManager.Instance.UseButton != null)
            {
                Vector3 pos = HudManager.Instance.UseButton.transform.localPosition;
                if (this.mirror)
                {
                    pos = new Vector3(-pos.x, pos.y, pos.z);
                }
                this.Button.transform.localPosition = pos + PositionOffset;
            }
            if (this.canUse())
            {
                this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.EnabledColor;
                this.Button.graphic.material.SetFloat("_Desat", 0f);
            }
            else
            {
                this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.DisabledClear;
                this.Button.graphic.material.SetFloat("_Desat", 1f);
            }

            if (this.timer >= 0)
            {
                if ((this.isHasCleanUp() && isAbilityOn) || 
                    (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable))
                {
                    this.timer -= Time.deltaTime;
                }
            }

            if (this.timer <= 0 && this.isHasCleanUp() && isAbilityOn)
            {
                isAbilityOn = false;
                Button.cooldownTimerText.color = Palette.EnabledColor;
                cleanUp();
            }

            Button.SetCoolDown(
                this.timer,
                (this.isHasCleanUp() && this.isAbilityOn) ? this.abilityActiveTime : this.coolTime);

            // Trigger OnClickEvent if the hotkey is being pressed down
            if (Input.GetKeyDown(hotkey))
            {
                OnClickEvent();
            }
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

        private bool isHasCleanUp() => cleanUp != null;
    }
}
