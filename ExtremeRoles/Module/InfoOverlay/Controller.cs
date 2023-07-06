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
				public bool IsBlock { get; private set; } = false;
				private InfoOverlayView? view;
				private InfoOverlayModel model;
				private HelpButton button;

				public Controller()
				{
								this.model = new InfoOverlayModel();
								this.button = new HelpButton();
				}

				public void BlockShow(bool isBlock)
				{
								this.IsBlock = isBlock;
								this.Hide();
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
												this.button.CreateInfoButton();
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
								UpdateFunc.InitializeGame(this.model);
				}

				public void Show(InfoOverlayModel.Type showTyep)
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

				public void ToggleView()
				{
								ToggleView(this.model.CurShow);
				}

				public void ToggleView(InfoOverlayModel.Type showTyep)
				{
								if (this.view == null ||
												!this.view.isActiveAndEnabled)
								{
												Show(showTyep);
								}
								else if (this.model.CurShow != showTyep)
								{
												Show(showTyep);
								}
								else
								{
												Hide();
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
								if (GameSystem.IsLobby)
								{
												if (Input.GetKeyDown(KeyCode.H))
												{
																ToggleView(InfoOverlayModel.Type.AllRole);
												}
												if (Input.GetKeyDown(KeyCode.G))
												{
																ToggleView(InfoOverlayModel.Type.AllGhostRole);
												}
								}
								else
								{
												if (Input.GetKeyDown(KeyCode.H))
												{
																ToggleView(InfoOverlayModel.Type.MyRole);
												}
												if (Input.GetKeyDown(KeyCode.G))
												{
																ToggleView(InfoOverlayModel.Type.MyGhostRole);
												}
												if (Input.GetKeyDown(KeyCode.I))
												{
																ToggleView(InfoOverlayModel.Type.AllRole);
												}
												if (Input.GetKeyDown(KeyCode.U))
												{
																ToggleView(InfoOverlayModel.Type.AllGhostRole);
												}
								}
								if (Input.GetKeyDown(KeyCode.O))
								{
												ToggleView(InfoOverlayModel.Type.GlobalSetting);
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
}
