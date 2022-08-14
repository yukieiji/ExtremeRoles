using UnityEngine;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion
    {
        private const KeyCode endGame = KeyCode.L;
        private const KeyCode hideGUI = KeyCode.F;

        private const KeyCode reset = KeyCode.R;

        private const KeyCode ops = KeyCode.LeftAlt;
        private const KeyCode changeSpeed = KeyCode.LeftShift;
        private const KeyCode cameraMod = KeyCode.LeftControl;

        private const KeyCode down = KeyCode.PageDown;
        private const KeyCode up = KeyCode.PageUp;

        private const KeyCode SpawnDummy = KeyCode.F;

        private bool isHideGUI = false;

        public static void SpecialKeyShortCut()
        {
            if (!AmongUsClient.Instance.AmHost) { return; }
            if (Input.GetKeyDown(SpawnDummy) &&
                AmongUsClient.Instance.GameMode == GameModes.LocalGame &&
                Helper.GameSystem.IsLobby)
            {
                spawnDummy();
            }
        }

        private void keyBind()
        {

            // ゲーム終了
            if (Input.GetKey(ops) &&
                Input.GetKeyDown(endGame))
            {
                RpcForceEndGame();
            }

            if (MeetingHud.Instance) { return; }

            // GUI非表示
            if (Input.GetKey(ops) &&
                Input.GetKeyDown(hideGUI))
            {
                this.isHideGUI = !this.isHideGUI;
            }

            // 高速移動
            if (Input.GetKey(changeSpeed) &&
                Input.GetKeyDown(up))
            {
                this.RpcSpeedUp();
            }

            // 低速移動
            if (Input.GetKey(changeSpeed) &&
                Input.GetKeyDown(down))
            {
                this.RpcSpeedDown();
            }

            // 移動速度リセット
            if (Input.GetKey(changeSpeed) &&
                Input.GetKeyDown(reset))
            {
                this.RpcResetSpeed();
            }

            // ズームイン
            if (Input.GetKey(cameraMod) &&
                Input.GetKeyDown(up))
            {
                this.cameraZoomIn();
            }

            // ズームアウト
            if (Input.GetKey(cameraMod) &&
                Input.GetKeyDown(down))
            {
                this.cameraZoomOut();
            }

            // カメラリセット
            if (Input.GetKey(cameraMod) &&
                Input.GetKeyDown(reset))
            {
                this.resetCamera();
            }
        }

    }
}
