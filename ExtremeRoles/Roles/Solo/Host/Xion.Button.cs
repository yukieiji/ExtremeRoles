using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.Solo.Host.Button;

using ExtremeRoles.Extension.Manager;
using ExtremeRoles.GameMode;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Host;

public sealed partial class Xion
{
    private List<XionActionButton> funcButton = new List<XionActionButton>();
    private XionActionToPlayerButton? playerActionButton = null;

    private const float zoomOutFactor = 1.25f;
    private const float zoomInFactor = 0.8f;
    private const float maxZoomIn = 0.0001f;
    private const float maxZoomOut = 50.0f;

    private enum PlayerState : byte
    {
        Alive,
        Dead
    }

    public void CreateButton()
    {
		// デフォルトで必要なボタン
        this.funcButton = new List<XionActionButton>(6)
        {
            new XionActionButton(
				GetSprite("ZoomIn"),
				this.cameraZoomIn,
                Tr.GetString("zoomIn")),
            new XionActionButton(
				GetSprite("SpeedDown"),
				this.RpcSpeedDown,
                Tr.GetString("speedDown")),
        };

		bool enableMeeting = !ExtremeGameModeManager.Instance.ShipOption.IsBreakEmergencyButton;
		if (enableMeeting)
		{
			this.funcButton.Add(
				new XionActionButton(
					UnityObjectLoader.LoadFromResources<Sprite>(ObjectPath.Meeting),
					this.RpcCallMeeting,
					Tr.GetString("emergencyMeeting")));
		}

		// 残りのボタン
		this.funcButton.Add(
			new XionActionButton(
				UnityObjectLoader.LoadSpriteFromResources(
					ObjectPath.MaintainerRepair),
				this.RpcRepairSabotage,
				Tr.GetString("maintenance")));
		this.funcButton.Add(
			new XionActionButton(
				GetSprite("ZoomOut"),
				this.cameraZoomOut,
				Tr.GetString("zoomOut")));
		this.funcButton.Add(
			new XionActionButton(
				GetSprite("SpeedUp"),
				this.RpcSpeedUp,
				Tr.GetString("speedUp")));


		var hud = FastDestroyableSingleton<HudManager>.Instance;
        GridArrange grid = hud.UseButton.transform.parent.gameObject.GetComponent<GridArrange>();
        grid.MaxColumns = enableMeeting ? 4 : 3;

		hud.ReGridButtons();

        // プレイヤーに関する能力周り
        this.playerActionButton = new XionActionToPlayerButton(PlayerId);
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
        foreach (var button in this.funcButton)
        {
            button.Update();
        }

        this.playerActionButton?.Update(this.isHideGUI);
    }

    private void resetCoolTime()
    {
        foreach (var button in this.funcButton)
        {
            button.ResetCoolTimer();
        }
        this.playerActionButton?.ResetCoolTime();
    }

    private void setButtonActive(bool active)
    {
        // GUI非表示中はボタン全部消す
        if (this.isHideGUI)
        {
            active = false;
        }
        foreach (var button in this.funcButton)
        {
            button.SetActive(active);
        }
        this.playerActionButton?.SetActive(active);
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
        foreach (var cam in Camera.allCameras)
        {
            if (cam != null && cam.gameObject.name == "UI Camera")
            {
                cam.orthographicSize *= zoomFactor;
                // The UI is scaled too, else we cant click the buttons. Downside: }map is super small.
            }
        }

		int width = Screen.width;
		int height = Screen.height;

		ResolutionManager.SetResolution(width, height, Screen.fullScreen);
        // This will move button positions to the correct position.
    }

    public static Action GetPlayerButtonAction(byte playerId)
    {
        return () =>
        {
            NetworkedPlayerInfo player = GameData.Instance.GetPlayerById(playerId);
            if (player == null || player.Disconnected) { return; }

            if (Key.IsAltDown())
            {
                if (player.IsDead)
                {
                    Player.RpcUncheckRevive(playerId);
                }
                else
                {
                    Player.RpcUncheckMurderPlayer(
                        playerId, playerId, byte.MaxValue);
                }
            }
            else
            {
                rpcTeleport(player.Object);
            }
        };
    }

	public static Sprite GetSprite(string name)
		=> UnityObjectLoader.LoadFromResources(ExtremeRoleId.Xion, name);
}
