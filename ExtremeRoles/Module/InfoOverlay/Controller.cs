using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomMonoBehaviour.View;
using ExtremeRoles.Module.InfoOverlay.Model;
using ExtremeRoles.Resources;
using UnityEngine;
using UpdateFunc = ExtremeRoles.Module.InfoOverlay.Update;

#nullable enable

namespace ExtremeRoles.Module.InfoOverlay;


public sealed class Controller : NullableSingleton<Controller>
{
				private InfoOverlayView? view;
				private InfoOverlayModel model;

				public Controller()
				{
								this.model = new InfoOverlayModel();
				}

				public void SetLobbyView()
				{
								UpdateFunc.InitializeLobby(this.model);
				}
				public void SetGameView()
				{
								UpdateFunc.InitializeGame(this.model);
				}

				public void ToggleView(InfoOverlayModel.Type showTyep)
				{
								if (this.view == null ||
												this.view.isActiveAndEnabled)
								{
												Show(showTyep);
								}
								else
								{
												Hide();
								}
				}

				public void Show(InfoOverlayModel.Type showTyep)
				{
								UpdateFunc.SwithTo(this.model, showTyep);

								if (this.view == null)
								{
												this.setView();
								}
								if (this.view != null)
								{
												this.view.gameObject.SetActive(true);
								}
				}

				public void Hide()
				{
								if (this.view != null)
								{
												this.view.gameObject.SetActive(false);
								}
				}

				public void Update()
				{
								keyUpdate();

								if (this.model.IsDuty &&
												this.view != null)
								{
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
				}

				private void setView()
				{
								GameObject obj = Object.Instantiate(
												Loader.GetUnityObjectFromResources<GameObject>(
																Path.InfoOverlayResources,
																Path.InfoOverlayPrefab));
								this.view = obj.GetComponent<InfoOverlayView>();
				}
}
