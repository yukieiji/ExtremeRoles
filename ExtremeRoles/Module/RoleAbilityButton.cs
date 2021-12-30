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
        public string ButtonText = null;

        private int abilityNum = int.MaxValue;
        private bool mirror;
        private Func<bool> useAbility;
        private Func<bool> canUse;
        private KeyCode hotkey;

        private Action cleanUp = null;
        private Func<bool> abilityCheck = null;

        private bool isAbilityOn = false;

        private float timer = 10.0f;
        private float coolTime = float.MaxValue;
        private float abilityActiveTime = 0.0f;

        private TMPro.TextMeshPro abilityCountText = null;

        public RoleAbilityButton(
            string buttonName,
            Func<bool> ability,
            Func<bool> canUse,
            Sprite sprite,
            Vector3 positionOffset,
            int abilityMaxNum = int.MaxValue,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey=KeyCode.F,
            bool mirror = false)
        {

            this.ButtonText = buttonName;

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
            
            this.abilityNum = abilityMaxNum;
            if (this.abilityNum != int.MaxValue)
            {
                this.abilityCountText = GameObject.Instantiate(
                    this.Button.cooldownTimerText,
                    this.Button.cooldownTimerText.transform.parent);
                updateAbilityCountText();
                this.abilityCountText.enableWordWrapping = false;
                this.abilityCountText.transform.localScale = Vector3.one * 0.65f;
                this.abilityCountText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
            }

            LocalScale = Button.transform.localScale;

            this.abilityCheck = abilityCheck;
            if (this.abilityCheck == null)
            {
                this.abilityCheck = allTrue;
            }

            SetActive(false);

            bool allTrue() => true;

        }
        public void ResetCoolTimer()
        {
            this.timer = this.coolTime;
        }

        public void OnClickEvent()
        {
            if (this.timer < 0f && canUse() && !this.isAbilityOn)
            {
                Button.graphic.color = new Color(1f, 1f, 1f, 0.3f);

                if (this.useAbility())
                {
                    this.ResetCoolTimer();

                    if (this.isHasCleanUp())
                    {
                        this.timer = this.abilityActiveTime;
                        Button.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                        this.isAbilityOn = true;
                    }
                    else
                    {
                        this.reduceAbilityCount();
                    }
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

        public void UpdateAbility(Func<bool> newAbility)
        {
            this.useAbility = newAbility;
        }

        public void ReplaceHotKey(KeyCode newKey)
        {
            this.hotkey = newKey;
        }

        public void UpdateAbilityCount(int newCount)
        {
            this.abilityNum = newCount;
            this.updateAbilityCountText();
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
                bool abilityOn = this.isHasCleanUp() && isAbilityOn;

                if (abilityOn || (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable))
                {
                    this.timer -= Time.deltaTime;
                }
                if (abilityOn)
                {
                    if(!this.abilityCheck())
                    {
                        this.timer = 0;
                        this.isAbilityOn = false;
                    }
                }
            }

            if (this.timer <= 0 && this.isHasCleanUp() && isAbilityOn)
            {
                this.isAbilityOn = false;
                this.Button.cooldownTimerText.color = Palette.EnabledColor;
                this.cleanUp();
                this.reduceAbilityCount();
            }

            if (this.abilityNum > 0)
            {
                Button.SetCoolDown(
                    this.timer,
                    (this.isHasCleanUp() && this.isAbilityOn) ? this.abilityActiveTime : this.coolTime);
            }

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

        private void reduceAbilityCount()
        {
            if (this.abilityCountText == null) { return; }
            --this.abilityNum;
            updateAbilityCountText();

        }
        public void updateAbilityCountText()
        {
            this.abilityCountText.text = Helper.Translation.GetString("buttonCountText") + string.Format(
                Helper.Translation.GetString("unitShots"), this.abilityNum);
        }

    }
}
