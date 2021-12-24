using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ExtremeRoles.Module
{
    public class RoleAbilityButton
    {
        public ActionButton Button;
        public Vector3 PositionOffset;
        public bool HasEffect;
        public Sprite ButtonSprite;

        public float Timer = 0f;
        public float MaxTimer = float.MaxValue;
        public float AbilityTime = 0.0f;
        public Vector3 LocalScale = Vector3.one;
        public bool IsAbilityOn = false;
        public bool ShowButtonText = true;
        public string ButtonText = null;

        private bool Mirror;
        private Action UseAbility;
        private Func<bool> CanUse;
        private KeyCode Hotkey;
        private Action CleanUp = null;

        RoleAbilityButton(
            Action ability,
            Func<bool> canUse,
            Sprite sprite,
            Vector3 positionOffset,
            float abilityTime=0.0f,
            Action abilityCleanUp=null,
            KeyCode hotkey=KeyCode.F,
            bool mirror = false)
        {
            this.UseAbility = ability;
            this.CanUse = canUse;
            this.PositionOffset = positionOffset;
            this.AbilityTime = abilityTime;
            this.CleanUp = abilityCleanUp;
            this.ButtonSprite = sprite;
            this.Mirror = mirror;
            this.Hotkey = hotkey;

            Timer = 10.0f;
            
            Button = UnityEngine.Object.Instantiate(
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
            if (this.Timer < 0f && CanUse())
            {
                Button.graphic.color = new Color(1f, 1f, 1f, 0.3f);
                this.UseAbility();

                if (IsHasCleanUp() && !this.IsAbilityOn)
                {
                    this.Timer = this.AbilityTime;
                    Button.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    this.IsAbilityOn = true;
                }
            }
        }

        public void Update()
        {
            if (PlayerControl.LocalPlayer.Data == null || 
                MeetingHud.Instance || 
                ExileController.Instance)
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
                if (this.Mirror)
                {
                    pos = new Vector3(-pos.x, pos.y, pos.z);
                }
                this.Button.transform.localPosition = pos + PositionOffset;
            }
            if (this.CanUse())
            {
                this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.EnabledColor;
                this.Button.graphic.material.SetFloat("_Desat", 0f);
            }
            else
            {
                this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.DisabledClear;
                this.Button.graphic.material.SetFloat("_Desat", 1f);
            }

            if (this.Timer >= 0)
            {
                if ((this.IsHasCleanUp() && IsAbilityOn) || 
                    (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable))
                {
                    this.Timer -= Time.deltaTime;
                }
            }

            if (this.Timer <= 0 && IsHasCleanUp() && IsAbilityOn)
            {
                IsAbilityOn = false;
                Button.cooldownTimerText.color = Palette.EnabledColor;
                CleanUp();
            }

            Button.SetCoolDown(
                this.Timer,
                (this.IsHasCleanUp() && this.IsAbilityOn) ? this.AbilityTime : this.MaxTimer);

            // Trigger OnClickEvent if the hotkey is being pressed down
            if (Input.GetKeyDown(Hotkey))
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

        private bool IsHasCleanUp() => CleanUp != null;

    }
}
