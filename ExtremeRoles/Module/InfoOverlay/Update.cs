using System.Collections.Generic;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.InfoOverlay.Model;
using ExtremeRoles.Module.InfoOverlay.Model.Panel;

namespace ExtremeRoles.Module.InfoOverlay;

#nullable enable

public static class Update
{
				public static void InitializeLobby(InfoOverlayModel model)
				{
								if (model.PanelModel == null ||
												model.PanelModel.Count == 0)
								{
												initializeModel(model);
								}
								else if (model.PanelModel.Count >= 4)
								{
												foreach (InfoOverlayModel.Type value in System.Enum.GetValues(typeof(InfoOverlayModel.Type)))
												{
																switch (value)
																{
																				case InfoOverlayModel.Type.AllRole:
																				case InfoOverlayModel.Type.AllGhostRole:
																				case InfoOverlayModel.Type.GlobalSetting:
																								continue;
																				default:
																								break;
																}
																model.PanelModel.Remove(value);
												}
								}
								model.CurShow = InfoOverlayModel.Type.AllRole;
								model.IsDuty = true;
				}

				public static void InitializeGame(InfoOverlayModel model)
				{
								if (model.PanelModel == null ||
												model.PanelModel.Count == 0)
								{
												initializeModel(model);
								}
								else
								{
												model.PanelModel[InfoOverlayModel.Type.MyRole] = new LocalRoleInfoModel();
												model.PanelModel[InfoOverlayModel.Type.MyGhostRole] = new LocalGhostRoleInfoModel();
								}
								model.CurShow = InfoOverlayModel.Type.MyRole;
								model.IsDuty = true;
				}

				public static void SwithTo(InfoOverlayModel model, InfoOverlayModel.Type newType)
				{
								if (!model.PanelModel.ContainsKey(newType)) { return; }

								model.CurShow = newType;
								model.IsDuty = true;
				}

				public static void IncreasePage(InfoOverlayModel model)
				{
								if (!model.PanelModel.TryGetValue(model.CurShow, out var panel) ||
												panel is not PanelPageModelBase pagePanel) { return; }
								pagePanel.CurPage = pagePanel.CurPage + 1;
								model.IsDuty = true;
				}

				public static void DecreasePage(InfoOverlayModel model)
				{
								if (!model.PanelModel.TryGetValue(model.CurShow, out var panel) ||
												panel is not PanelPageModelBase pagePanel) { return; }
								pagePanel.CurPage = pagePanel.CurPage - 1;
								model.IsDuty = true;
				}

				private static void initializeModel(InfoOverlayModel model)
				{
								model.PanelModel = new SortedDictionary<InfoOverlayModel.Type, IInfoOverlayPanelModel>()
								{
												{
																InfoOverlayModel.Type.AllRole,
																new AllRoleInfoModel()
												},
												{
																InfoOverlayModel.Type.AllGhostRole,
																new AllGhostRoleInfoModel()
												},
												{
																InfoOverlayModel.Type.GlobalSetting,
																new GlobalSettingInfoModel()
												}
								};
				}
}
