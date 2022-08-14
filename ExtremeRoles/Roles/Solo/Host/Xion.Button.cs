using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Resources;

using ExtremeRoles.Performance;


namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion
    {
        private sealed class NoneCoolTimeButton
        {
            private ActionButton body;
            private KeyCode hotkey;

            private float timer;
            private Action buttonAction;

            private const float coolTime = 0.0001f;

            public NoneCoolTimeButton(
                Sprite sprite,
                Action buttonAction,
                Vector3 positionOffset,
                string buttonText = "",
                KeyCode hotkey = KeyCode.F)
            {

                var hudManager = FastDestroyableSingleton<HudManager>.Instance;

                this.body = UnityEngine.Object.Instantiate(
                    hudManager.KillButton, hudManager.KillButton.transform.parent);
                PassiveButton button = body.GetComponent<PassiveButton>();
                button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                button.OnClick.AddListener((UnityEngine.Events.UnityAction)onClickEvent);

                this.buttonAction = buttonAction;

                SetActive(false);

                this.hotkey = hotkey;

                var useButton = hudManager.UseButton;

                UnityEngine.Object.Destroy(
                   this.body.buttonLabelText.fontMaterial);
                this.body.buttonLabelText.fontMaterial = UnityEngine.Object.Instantiate(
                    useButton.buttonLabelText.fontMaterial, this.body.transform);

                this.body.graphic.sprite = sprite;
                this.body.transform.localPosition = useButton.transform.localPosition + positionOffset;

                this.body.graphic.sprite = sprite;
                this.body.OverrideText(buttonText);
            }

            public void SetActive(bool isActive)
            {
                if (isActive)
                {
                    this.body.gameObject.SetActive(true);
                    this.body.graphic.enabled = true;
                }
                else
                {
                    this.body.gameObject.SetActive(false);
                    this.body.graphic.enabled = false;
                }
            }

            public void ResetCoolTimer()
            {
                this.timer = coolTime;
            }

            public void Update()
            {
                if (this.body == null) { return; }
                if (CachedPlayerControl.LocalPlayer.Data == null ||
                    MeetingHud.Instance ||
                    ExileController.Instance ||
                    CachedPlayerControl.LocalPlayer.Data.IsDead)
                {
                    SetActive(false);
                    return;
                }

                SetActive(FastDestroyableSingleton<HudManager>.Instance.UseButton.isActiveAndEnabled);

                this.body.graphic.color = this.body.buttonLabelText.color = Palette.EnabledColor;
                this.body.graphic.material.SetFloat("_Desat", 0f);

                if (this.timer >= 0)
                {

                    if (!CachedPlayerControl.LocalPlayer.PlayerControl.inVent &&
                        CachedPlayerControl.LocalPlayer.PlayerControl.moveable)
                    {
                        this.timer -= Time.deltaTime;
                    }
                }

               this.body.SetCoolDown(this.timer, coolTime);

                if (Input.GetKeyDown(this.hotkey))
                {
                    onClickEvent();
                }
            }

            private void onClickEvent()
            {
                if (this.timer < 0f)
                {
                    this.body.graphic.color = new Color(1f, 1f, 1f, 0.3f);

                    this.buttonAction();
                    this.ResetCoolTimer();
                }
            }
        }

        private HashSet<NoneCoolTimeButton> buttons = new HashSet<NoneCoolTimeButton>();

        public void CreateButton()
        {
            // メンテナンスボタン
            var maintenanceButton = new NoneCoolTimeButton(
                Loader.CreateSpriteFromResources(
                    Path.MaintainerRepair),
                RpcRepairSabotage,
                new Vector3(),
                Helper.Translation.GetString("maintenance"));
            buttons.Add(maintenanceButton);

            var meetingButton = new NoneCoolTimeButton(
                Loader.CreateSpriteFromResources(
                    Path.DetectiveApprenticeEmergencyMeeting),
                RpcCallMeeting,
                new Vector3(),
                Helper.Translation.GetString("emergencyMeeting"));
            buttons.Add(maintenanceButton);

        }

        private void disableButton()
        {
            var hudManager = FastDestroyableSingleton<HudManager>.Instance;

            // 基本的にレポートボタンを無効化
            hudManager.ReportButton.SetDisabled();
            if (this.isHideGUI)
            {
                hudManager.KillButton.SetDisabled();
                hudManager.UseButton.SetDisabled();
                setButtonActive(false);
            }
        }

        private void buttonUpdate()
        {
            foreach(NoneCoolTimeButton button in this.buttons)
            {
                button.Update();
            }
        }

        private void resetCoolTime()
        {
            foreach (NoneCoolTimeButton button in this.buttons)
            {
                button.ResetCoolTimer();
            }
        }

        private void setButtonActive(bool active)
        {
            // GUI非表示中はボタン全部消す
            if (this.isHideGUI)
            {
                active = false;
            }
            foreach (NoneCoolTimeButton button in this.buttons)
            {
                button.SetActive(active);
            }
        }

    }
}
