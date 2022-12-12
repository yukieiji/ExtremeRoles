using ExtremeRoles.Performance;
using System;
using UnityEngine;

namespace ExtremeRoles.Roles.Solo.Host.Button
{
    internal abstract class NoneCoolButtonBase
    {
        protected ActionButton Body;
        protected Action ButtonAction;

        protected float Timer;

        private const float coolTime = 0.0001f;

        public void SetActive(bool isActive)
        {
            if (isActive)
            {
                this.Body.gameObject.SetActive(true);
                this.Body.graphic.enabled = true;
            }
            else
            {
                this.Body.gameObject.SetActive(false);
                this.Body.graphic.enabled = false;
            }
        }

        public void ResetCoolTimer()
        {
            this.Timer = coolTime;
        }

        public virtual void Update()
        {
            if (this.Body == null) { return; }
            if (CachedPlayerControl.LocalPlayer.Data == null ||
                MeetingHud.Instance ||
                ExileController.Instance)
            {
                SetActive(false);
                return;
            }

            var hudManager = FastDestroyableSingleton<HudManager>.Instance;

            SetActive(hudManager.UseButton.isActiveAndEnabled);

            this.Body.graphic.color = this.Body.buttonLabelText.color = Palette.EnabledColor;
            this.Body.graphic.material.SetFloat("_Desat", 0f);

            if (this.Timer >= 0)
            {

                if (!CachedPlayerControl.LocalPlayer.PlayerControl.inVent &&
                    CachedPlayerControl.LocalPlayer.PlayerControl.moveable)
                {
                    this.Timer -= Time.deltaTime;
                }
            }

            this.Body.SetCoolDown(this.Timer, coolTime);
        }

        protected void OnClickEvent()
        {
            if (this.Timer <= 0f)
            {
                this.Body.graphic.color = new Color(1f, 1f, 1f, 0.3f);

                this.ButtonAction.Invoke();
                this.ResetCoolTimer();
            }
        }
    }
}
