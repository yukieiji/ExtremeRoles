using UnityEngine;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion
    {
        private const KeyCode OpsKey = KeyCode.LeftControl;
        private const KeyCode endGame = KeyCode.L;
        private const KeyCode hideGUI = KeyCode.F;

        private const KeyCode reset = KeyCode.R;

        private const KeyCode speedDown = KeyCode.PageDown;
        private const KeyCode speedUp = KeyCode.PageUp;

        private const KeyCode zoomDown = KeyCode.Plus;
        private const KeyCode zoomOut = KeyCode.Plus;

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

            bool pressCtrl = Input.GetKey(OpsKey);

            // ゲーム終了
            if (pressCtrl &&
                Input.GetKeyDown(endGame))
            {
                RpcForceEndGame();
            }

            // GUI非表示
            if (pressCtrl &&
                Input.GetKeyDown(hideGUI))
            {
                this.isHideGUI = !this.isHideGUI;
            }

            // 高速移動
            if (pressCtrl &&
                Input.GetKeyDown(speedUp))
            {
                this.RpcSpeedUp();
            }

            // 低速移動
            if (pressCtrl &&
                Input.GetKeyDown(speedDown))
            {
                this.RpcSpeedDown();
            }


            if (pressCtrl &&
                Input.GetKeyDown(reset))
            {
                this.RpcResetSpeed();
            }
        }

    }
}
