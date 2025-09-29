using System.Collections.Generic;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.InfoOverlay.Model;
using ExtremeRoles.Module.InfoOverlay.Model.Panel;

namespace ExtremeRoles.Module.InfoOverlay;

#nullable enable

public static class Update
{
	public static void UpdatePanel(InfoOverlayModel model)
	{
		foreach (var panel in model.PanelModel.Values)
		{
			panel.UpdateVisual();
		}
		model.IsDuty = true;
	}

	public static void InitializeLobby(InfoOverlayModel model)
	{
		if (model.PanelModel == null ||
			model.PanelModel.Count == 0)
		{
			initializeModel(model);
		}
		else if (model.PanelModel.Count >= 4)
		{
			foreach (var value in System.Enum.GetValues<InfoOverlayModel.Type>())
			{
				switch (value)
				{
					case InfoOverlayModel.Type.AllRolePanel:
					case InfoOverlayModel.Type.AllGhostRolePanel:
					case InfoOverlayModel.Type.GlobalSettingPanel:
						continue;
					default:
						break;
				}
				model.PanelModel.Remove(value);
			}
		}
		model.CurShow = InfoOverlayModel.Type.AllRolePanel;
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
			model.PanelModel[InfoOverlayModel.Type.YourRolePanel] = new LocalRoleInfoModel();
			model.PanelModel[InfoOverlayModel.Type.YourGhostRolePanel] = new LocalGhostRoleInfoModel();
		}
		model.CurShow = InfoOverlayModel.Type.YourRolePanel;
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
			panel is not RolePagePanelModelBase pagePanel) { return; }
		pagePanel.CurPage = pagePanel.CurPage + 1;
		model.IsDuty = true;
	}

	public static void DecreasePage(InfoOverlayModel model)
	{
		if (!model.PanelModel.TryGetValue(model.CurShow, out var panel) ||
			panel is not RolePagePanelModelBase pagePanel) { return; }
		pagePanel.CurPage = pagePanel.CurPage - 1;
		model.IsDuty = true;
	}

	private static void initializeModel(InfoOverlayModel model)
	{
		model.PanelModel = new SortedDictionary<InfoOverlayModel.Type, IInfoOverlayPanelModel>()
		{
			{
				InfoOverlayModel.Type.AllRolePanel,
				new AllRoleInfoModel()
			},
			{
				InfoOverlayModel.Type.AllGhostRolePanel,
				new AllGhostRoleInfoModel()
			},
			{
				InfoOverlayModel.Type.GlobalSettingPanel,
				new GlobalSettingInfoModel()
			}
		};
	}
}
