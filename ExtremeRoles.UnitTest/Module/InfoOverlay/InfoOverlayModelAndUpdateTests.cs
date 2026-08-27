using System;
using System.Collections.Generic;
using ExtremeRoles.Module.InfoOverlay;
using ExtremeRoles.Module.InfoOverlay.Model;
using ExtremeRoles.Module.InfoOverlay.Model.Panel;
using ExtremeRoles.Module.Interface;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.InfoOverlay;

[Collection("UnityMock")]
public class InfoOverlayModelAndUpdateTests
{
	public InfoOverlayModelAndUpdateTests()
	{
		MockSetupHelper.SetupCommonMocks();
		MockSetupHelper.SetupLogger();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
	}

	[Fact]
	public void InfoOverlayModel_DefaultsAndProperties_SetAndGetCorrectly()
	{
		var model = new InfoOverlayModel();

		Assert.False(model.IsDuty);
		Assert.Equal(InfoOverlayModel.Type.YourRolePanel, model.CurShow);
		Assert.NotNull(model.PanelModel);

		model.IsDuty = true;
		Assert.True(model.IsDuty);

		model.CurShow = InfoOverlayModel.Type.AllGhostRolePanel;
		Assert.Equal(InfoOverlayModel.Type.AllGhostRolePanel, model.CurShow);

		model.IsShowActiveOnly = true;
		Assert.True(model.IsShowActiveOnly);

		model.IsShowActiveOnly = false;
		Assert.False(model.IsShowActiveOnly);
	}

	[Fact]
	public void UpdatePanel_CallsUpdateVisualOnAllPanels_AndSetsIsDutyTrue()
	{
		var model = new InfoOverlayModel();
		var mockPanel1 = new Mock<IInfoOverlayPanelModel>();
		var mockPanel2 = new Mock<IInfoOverlayPanelModel>();

		model.PanelModel[InfoOverlayModel.Type.YourRolePanel] = mockPanel1.Object;
		model.PanelModel[InfoOverlayModel.Type.YourGhostRolePanel] = mockPanel2.Object;
		model.IsDuty = false;

		Update.UpdatePanel(model);

		mockPanel1.Verify(p => p.UpdateVisual(), Times.Once);
		mockPanel2.Verify(p => p.UpdateVisual(), Times.Once);
		Assert.True(model.IsDuty);
	}

	[Fact]
	public void UpdateActiveToggle_UpdatesModelAndPanelModel_AndCallsUpdatePanel()
	{
		var model = new InfoOverlayModel();
		var mockPanel = new Mock<IInfoOverlayPanelModel>();
		model.PanelModel[InfoOverlayModel.Type.AllRolePanel] = mockPanel.Object;

		var dummyPanelModel = new DummyRolePagePanelModel();

		Update.UpdateActiveToggle(model, dummyPanelModel, true);

		Assert.True(model.IsShowActiveOnly);
		mockPanel.Verify(p => p.UpdateVisual(), Times.Once);
		Assert.True(model.IsDuty);
	}

	[Fact]
	public void InitializeLobby_WhenPanelModelEmpty_InitializesDefaultPanels()
	{
		var model = new InfoOverlayModel();
		model.PanelModel.Clear();

		Update.InitializeLobby(model);

		Assert.Equal(4, model.PanelModel.Count);
		Assert.True(model.PanelModel.ContainsKey(InfoOverlayModel.Type.AllRolePanel));
		Assert.True(model.PanelModel.ContainsKey(InfoOverlayModel.Type.AllGhostRolePanel));
		Assert.True(model.PanelModel.ContainsKey(InfoOverlayModel.Type.GlobalSettingPanel));
		Assert.True(model.PanelModel.ContainsKey(InfoOverlayModel.Type.Liberal));
		Assert.Equal(InfoOverlayModel.Type.AllRolePanel, model.CurShow);
		Assert.True(model.IsDuty);
	}

	[Fact]
	public void InitializeLobby_WhenPanelModelHas5OrMorePanels_RemovesGameSpecificPanels()
	{
		var model = new InfoOverlayModel();
		Update.InitializeLobby(model); // Adds 4 lobby panels
		// Add 2 game panels to bring total to 6
		model.PanelModel[InfoOverlayModel.Type.YourRolePanel] = new Mock<IInfoOverlayPanelModel>().Object;
		model.PanelModel[InfoOverlayModel.Type.YourGhostRolePanel] = new Mock<IInfoOverlayPanelModel>().Object;
		Assert.Equal(6, model.PanelModel.Count);

		Update.InitializeLobby(model);

		Assert.Equal(4, model.PanelModel.Count);
		Assert.False(model.PanelModel.ContainsKey(InfoOverlayModel.Type.YourRolePanel));
		Assert.False(model.PanelModel.ContainsKey(InfoOverlayModel.Type.YourGhostRolePanel));
		Assert.Equal(InfoOverlayModel.Type.AllRolePanel, model.CurShow);
		Assert.True(model.IsDuty);
	}

	[Fact]
	public void InitializeGame_WhenPanelModelEmpty_InitializesDefaultPanels()
	{
		var model = new InfoOverlayModel();
		model.PanelModel.Clear();

		Update.InitializeGame(model);

		Assert.True(model.PanelModel.Count >= 4);
		Assert.Equal(InfoOverlayModel.Type.YourRolePanel, model.CurShow);
		Assert.True(model.IsDuty);
	}

	[Fact]
	public void InitializeGame_WhenPanelModelHasPanels_AddsOrUpdatesLocalRolePanels()
	{
		var model = new InfoOverlayModel();
		Update.InitializeLobby(model);

		Update.InitializeGame(model);

		Assert.True(model.PanelModel.ContainsKey(InfoOverlayModel.Type.YourRolePanel));
		Assert.True(model.PanelModel.ContainsKey(InfoOverlayModel.Type.YourGhostRolePanel));
		Assert.IsType<LocalRoleInfoModel>(model.PanelModel[InfoOverlayModel.Type.YourRolePanel]);
		Assert.IsType<LocalGhostRoleInfoModel>(model.PanelModel[InfoOverlayModel.Type.YourGhostRolePanel]);
		Assert.Equal(InfoOverlayModel.Type.YourRolePanel, model.CurShow);
		Assert.True(model.IsDuty);
	}

	[Fact]
	public void SwithTo_WhenTypeExists_ChangesCurShowAndSetsIsDuty()
	{
		var model = new InfoOverlayModel();
		Update.InitializeLobby(model);
		model.IsDuty = false;

		Update.SwithTo(model, InfoOverlayModel.Type.GlobalSettingPanel);

		Assert.Equal(InfoOverlayModel.Type.GlobalSettingPanel, model.CurShow);
		Assert.True(model.IsDuty);
	}

	[Fact]
	public void SwithTo_WhenTypeDoesNotExist_DoesNotChangeCurShow()
	{
		var model = new InfoOverlayModel();
		Update.InitializeLobby(model);
		model.CurShow = InfoOverlayModel.Type.AllRolePanel;
		model.IsDuty = false;

		Update.SwithTo(model, InfoOverlayModel.Type.YourRolePanel); // Not in lobby model

		Assert.Equal(InfoOverlayModel.Type.AllRolePanel, model.CurShow);
		Assert.False(model.IsDuty);
	}

	[Fact]
	public void IncreasePage_And_DecreasePage_WhenCurrentPanelIsRolePagePanel_ChangesPageAndSetsIsDuty()
	{
		var model = new InfoOverlayModel();
		var dummyPanel = new DummyRolePagePanelModel();
		model.PanelModel[InfoOverlayModel.Type.AllRolePanel] = dummyPanel;
		model.CurShow = InfoOverlayModel.Type.AllRolePanel;
		model.IsDuty = false;

		// Initial page is 0
		Update.IncreasePage(model);
		Assert.True(model.IsDuty);

		model.IsDuty = false;
		Update.DecreasePage(model);
		Assert.True(model.IsDuty);
	}

	[Fact]
	public void IncreasePage_And_DecreasePage_WhenCurrentPanelIsNotRolePagePanel_DoesNothing()
	{
		var model = new InfoOverlayModel();
		var mockPanel = new Mock<IInfoOverlayPanelModel>();
		model.PanelModel[InfoOverlayModel.Type.GlobalSettingPanel] = mockPanel.Object;
		model.CurShow = InfoOverlayModel.Type.GlobalSettingPanel;
		model.IsDuty = false;

		Update.IncreasePage(model);
		Assert.False(model.IsDuty);

		Update.DecreasePage(model);
		Assert.False(model.IsDuty);
	}

	private sealed class DummyRolePagePanelModel : RolePagePanelModelBase
	{
		protected override void CreateAllRoleText()
		{
		}
	}
}
