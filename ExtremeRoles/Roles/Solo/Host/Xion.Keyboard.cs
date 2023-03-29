using UnityEngine;
using System.Linq;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Roles.Solo.Host;

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

    private const KeyCode forceMeetingEnd = KeyCode.C;
    private const KeyCode spawnDeadBody = KeyCode.B;
    private const KeyCode functionCall = KeyCode.F;

    private bool isHideGUI = false;

    public static void SpecialKeyShortCut()
    {
        if (!AmongUsClient.Instance.AmHost) { return; }
        // HotFix : BlackOut AmongUs
        if (Input.GetKeyDown(functionCall) &&
            isLocalGame() &&
            GameSystem.IsLobby &&
            IsAllPlyerDummy())
        {
            GameSystem.SpawnDummyPlayer($"XionDummy_{randomString(10)}");
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

        if (MeetingHud.Instance)
        {
            // 会議強制終了
            if (Input.GetKey(ops) && Input.GetKeyDown(forceMeetingEnd))
            {
                MeetingHud.Instance.ForceSkipAll();
            }
            return;
        }

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

        // カメラリセット
        if (Input.GetKey(ops) &&
            Input.GetKeyDown(spawnDeadBody) &&
            isLocalGame())
        {
            this.SpawnDummyDeadBody();
        }

# if DEBUG
        // テスト用能力
        if (Input.GetKey(ops) &&
            Input.GetKeyDown(functionCall))
        {
            this.RpcTestAbilityCall();
        }
#endif
    }

    public static bool IsAllPlyerDummy()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) { continue; }

            if (!player.GetComponent<DummyBehaviour>().enabled)
            {
                return false;
            }
        }
        return true;
    }

    private static string randomString(int length)
    {
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[RandomGenerator.Instance.Next(s.Length)]).ToArray());
    }
}
