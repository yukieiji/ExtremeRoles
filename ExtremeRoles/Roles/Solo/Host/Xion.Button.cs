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

            private float timer;
            private Action buttonAction;

            private string buttonText;
            private Vector3 offset;
            private bool mirror;

            private const float coolTime = 0.0001f;

            public NoneCoolTimeButton(
                Sprite sprite,
                Action buttonAction,
                Vector3 positionOffset,
                string buttonText = "",
                bool mirror = false)
            {

                var hudManager = FastDestroyableSingleton<HudManager>.Instance;

                this.body = UnityEngine.Object.Instantiate(
                    hudManager.KillButton, hudManager.KillButton.transform.parent);
                PassiveButton button = body.GetComponent<PassiveButton>();
                button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                button.OnClick.AddListener((UnityEngine.Events.UnityAction)onClickEvent);

                this.buttonAction = buttonAction;

                SetActive(false);

                this.mirror = mirror;

                var useButton = hudManager.UseButton;

                UnityEngine.Object.Destroy(
                   this.body.buttonLabelText.fontMaterial);
                this.body.buttonLabelText.fontMaterial = UnityEngine.Object.Instantiate(
                    useButton.buttonLabelText.fontMaterial, this.body.transform);

                this.body.graphic.sprite = sprite;
                this.body.transform.localPosition = useButton.transform.localPosition + positionOffset;

                this.body.graphic.sprite = sprite;
                this.body.OverrideText(buttonText);

                this.buttonText = buttonText;
                this.offset = positionOffset;

                ResetCoolTimer();
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
                    ExileController.Instance)
                {
                    SetActive(false);
                    return;
                }

                var hudManager = FastDestroyableSingleton<HudManager>.Instance;

                SetActive(hudManager.UseButton.isActiveAndEnabled);

                this.body.graphic.color = this.body.buttonLabelText.color = Palette.EnabledColor;
                this.body.graphic.material.SetFloat("_Desat", 0f);

                this.body.OverrideText(this.buttonText);

                if (hudManager.UseButton != null)
                {
                    Vector3 pos = hudManager.UseButton.transform.localPosition;
                    if (this.mirror)
                    {
                        pos = new Vector3(-pos.x, pos.y, pos.z);
                    }
                    this.body.transform.localPosition = pos + this.offset;
                }

                if (this.timer >= 0)
                {

                    if (!CachedPlayerControl.LocalPlayer.PlayerControl.inVent &&
                        CachedPlayerControl.LocalPlayer.PlayerControl.moveable)
                    {
                        this.timer -= Time.deltaTime;
                    }
                }

                this.body.SetCoolDown(this.timer, coolTime);
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

        private const float zoomOutFactor = 1.25f;
        private const float zoomInFactor = 0.8f;
        private const float maxZoomIn = 0.0001f;
        private const float maxZoomOut = 50.0f;

        public void CreateButton()
        {
            // メンテナンスボタン
            this.buttons.Add(
                new NoneCoolTimeButton(
                    Loader.CreateSpriteFromResources(
                        Path.MaintainerRepair),
                    this.RpcRepairSabotage,
                    new Vector3(-0.9f, -0.06f, 0),
                    Helper.Translation.GetString("maintenance")));

            // 会議招集ボタン
            this.buttons.Add(
                new NoneCoolTimeButton(
                    Loader.CreateSpriteFromResources(
                        Path.DetectiveApprenticeEmergencyMeeting),
                    this.RpcCallMeeting,
                    new Vector3(0, 1.0f, 0),
                    Helper.Translation.GetString("emergencyMeeting")));

            // ズームインアウト
            this.buttons.Add(
                new NoneCoolTimeButton(
                    Loader.CreateSpriteFromResources(
                        Path.TestButton),
                    this.cameraZoomOut,
                    new Vector3(-1.8f, 1.0f, 0),
                    Helper.Translation.GetString("zoomOut")));

            this.buttons.Add(
                new NoneCoolTimeButton(
                    Loader.CreateSpriteFromResources(
                        Path.TestButton),
                    this.cameraZoomIn,
                    new Vector3(-1.8f, -0.06f, 0),
                    Helper.Translation.GetString("zoomIn")));

            // スピード変更
            this.buttons.Add(
                new NoneCoolTimeButton(
                    Loader.CreateSpriteFromResources(
                        Path.TestButton),
                    this.RpcSpeedUp,
                    new Vector3(-2.7f, 1.0f, 0),
                    Helper.Translation.GetString("speedUp")));
            this.buttons.Add(
                new NoneCoolTimeButton(
                    Loader.CreateSpriteFromResources(
                        Path.TestButton),
                    this.RpcSpeedDown,
                    new Vector3(-2.7f, -0.06f, 0),
                    Helper.Translation.GetString("speedDown")));
        }

        private void disableButton()
        {
            var hudManager = FastDestroyableSingleton<HudManager>.Instance;

            // 基本的にレポートボタンを無効化
            hudManager.ReportButton.Hide();
            if (this.isHideGUI)
            {
                hudManager.UseButton.Hide();
                hudManager.SabotageButton.Hide();
                setButtonActive(false);
            }
            else
            {
                if (MeetingHud.Instance) { return; }
                hudManager.UseButton.Show();
                hudManager.SabotageButton.Show();
                setButtonActive(true);
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

        private void cameraZoomOut()
        {
            if (Camera.main.orthographicSize > maxZoomOut) { return; }
            modCamera(zoomOutFactor);
        }
        private void cameraZoomIn()
        {
            if (Camera.main.orthographicSize < maxZoomIn) { return; }
            modCamera(zoomInFactor);
        }

        private void resetCamera()
        {
            modCamera(this.defaultCameraZoom / Camera.main.orthographicSize);
        }

        private void modCamera(float zoomFactor)
        {
            Camera.main.orthographicSize *= zoomFactor;

            var hudManager = FastDestroyableSingleton<HudManager>.Instance;
            foreach (var cam in Camera.allCameras)
            {
                if (cam != null && cam.gameObject.name == "UI Camera")
                {
                    cam.orthographicSize *= zoomFactor;
                    // The UI is scaled too, else we cant click the buttons. Downside: }map is super small.
                }
            }
            
            if (Camera.main.orthographicSize == this.defaultCameraZoom)
            {
                hudManager.ShadowQuad.gameObject.SetActive(true);
            }
            else
            {
                // 見た目が悪くなるので無効化しとく
                hudManager.ShadowQuad.gameObject.SetActive(false);
            }

            ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height);
            // This will move button positions to the correct position.
        }

    }
}
