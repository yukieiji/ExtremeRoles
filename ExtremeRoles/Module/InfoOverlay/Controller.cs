using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour.View;
using ExtremeRoles.Module.InfoOverlay.Model;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;

using UpdateFunc = ExtremeRoles.Module.InfoOverlay.Update;

#nullable enable

namespace ExtremeRoles.Module.InfoOverlay;


public sealed class Controller : NullableSingleton<Controller>
{
	public bool IsBlock
	{
		get => this.isBlock;
		set
		{
			if (value)
			{
				this.Hide();
			}
			this.isBlock = value;
		}

	}
	private bool isBlock = false;
	private InfoOverlayView? view;
	private InfoOverlayModel model;
	private HelpButton button;

	public Controller()
	{
		this.model = new InfoOverlayModel();
		this.button = new HelpButton();
	}

	public void Hide()
	{
		if (this.view != null)
		{
			this.view.gameObject.SetActive(false);
		}
	}

	public void InitializeToLobby()
	{
		if (!this.button.IsInitialized)
		{
			this.button.CreateInfoButton(toggleView);
		}
		else
		{
			this.button.SetInfoButtonToGameStartShipPositon();
		}
		UpdateFunc.InitializeLobby(this.model);
	}

	public void InitializeToGame()
	{
		this.button.SetInfoButtonToInGamePositon();
		if (!GameSystem.IsFreePlay)
		{
			UpdateFunc.InitializeGame(this.model);
		}
		else
		{
			UpdateFunc.InitializeLobby(this.model);
		}
	}

	public void Update()
	{
		keyUpdate();

		if (this.model.IsDuty &&
			this.view != null)
		{
			this.model.IsDuty = false;
			this.view.UpdateFromModel(this.model);
		}
	}

	private void keyUpdate()
	{
		var hud = FastDestroyableSingleton<HudManager>.Instance;

		if (hud == null ||
			hud.Chat.IsOpenOrOpening ||
			Minigame.Instance != null)
		{
			if (this.view != null && this.view.isActiveAndEnabled)
			{
				Hide();
			}
			return;
		}

		if (GameSystem.IsLobby)
		{
			if (Input.GetKeyDown(KeyCode.H))
			{
				toggleView(InfoOverlayModel.Type.AllRolePanel);
			}
			if (Input.GetKeyDown(KeyCode.G))
			{
				toggleView(InfoOverlayModel.Type.AllGhostRolePanel);
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.H))
			{
				toggleView(InfoOverlayModel.Type.YourRolePanel);
			}
			if (Input.GetKeyDown(KeyCode.G))
			{
				toggleView(InfoOverlayModel.Type.YourGhostRolePanel);
			}
			if (Input.GetKeyDown(KeyCode.I))
			{
				toggleView(InfoOverlayModel.Type.AllRolePanel);
			}
			if (Input.GetKeyDown(KeyCode.U))
			{
				toggleView(InfoOverlayModel.Type.AllGhostRolePanel);
			}
		}
		if (Input.GetKeyDown(KeyCode.O))
		{
			toggleView(InfoOverlayModel.Type.GlobalSettingPanel);
		}

		if (this.view != null &&
			this.view.isActiveAndEnabled)
		{
			if (Input.GetKeyDown(KeyCode.PageDown))
			{
				UpdateFunc.IncreasePage(this.model);
			}
			if (Input.GetKeyDown(KeyCode.PageUp))
			{
				UpdateFunc.DecreasePage(this.model);
			}
		}
	}

	private void setView()
	{
		GameObject obj = Object.Instantiate(
			Loader.GetUnityObjectFromResources<GameObject>(
				Path.InfoOverlayResources,
				Path.InfoOverlayPrefab));
		this.view = obj.GetComponent<InfoOverlayView>();
		this.view.Awake();
	}

	private void show(InfoOverlayModel.Type showTyep)
	{
		var hudManager = FastDestroyableSingleton<HudManager>.Instance;

		if (hudManager == null || this.IsBlock) { return; }
		if (MeetingHud.Instance == null)
		{
			hudManager.SetHudActive(true);
		}

		UpdateFunc.SwithTo(this.model, showTyep);

		if (this.view == null)
		{
			this.setView();
		}
		if (this.view != null)
		{
			Transform parent = MeetingHud.Instance == null ?
			hudManager.transform : MeetingHud.Instance.transform;

			this.view.transform.localPosition = new Vector3(0f, 0f, -900f);
			this.view.transform.SetParent(parent);
			this.view.gameObject.SetActive(true);
		}
	}

	private void toggleView()
	{
		toggleView(this.model.CurShow);
	}

	private void toggleView(InfoOverlayModel.Type showTyep)
	{
		if (this.view == null ||
			!this.view.isActiveAndEnabled ||
			this.model.CurShow != showTyep)
		{
			show(showTyep);
		}
		else
		{
			Hide();
		}
	}
}
