using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.InfoOverlay.Model.Panel;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.InfoOverlay;

[Collection("UnityMock")]
public class InfoOverlayPanelModelTests
{
	public InfoOverlayPanelModelTests()
	{
		MockSetupHelper.SetupCommonMocks();
		MockSetupHelper.SetupLogger();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		var mockToHtmlStringRGBA = new Mock<MockColorUtilityToHtmlStringRGBAHelper>();
		mockToHtmlStringRGBA.Setup(x => x.Invoke(It.IsAny<Color>())).Returns("FFFFFF");
		MockColorUtilityToHtmlStringRGBAHelper.Instance = mockToHtmlStringRGBA.Object;

		var mockOpt = new Mock<IOption>();
		mockOpt.SetupGet(o => o.IsViewActive).Returns(true);
		mockOpt.SetupGet(o => o.TransedTitle).Returns("OptTitle");
		mockOpt.SetupGet(o => o.TransedValue).Returns("OptValue");
		var mockOptInfo = new Mock<IOptionInfo>();
		mockOptInfo.SetupGet(i => i.CodeRemovedName).Returns("CodeName");
		mockOpt.SetupGet(o => o.Info).Returns(mockOptInfo.Object);

		var services = new ServiceCollection();
		var mockLiberalLoader = (LiberalDefaultOptionLoader)RuntimeHelpers.GetUninitializedObject(typeof(LiberalDefaultOptionLoader));
		var dummyList = new List<IOption> { mockOpt.Object };
		typeof(LiberalDefaultOptionLoader).GetField("<GlobalOption>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(mockLiberalLoader, dummyList);
		typeof(LiberalDefaultOptionLoader).GetField("<LeaderOption>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(mockLiberalLoader, dummyList);
		typeof(LiberalDefaultOptionLoader).GetField("<MilitantOption>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(mockLiberalLoader, dummyList);
		services.AddSingleton(mockLiberalLoader);

		var serviceProvider = services.BuildServiceProvider();

		var backingField = typeof(ExtremeRolesPlugin).GetField("<Provider>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
		backingField?.SetValue(plugin, serviceProvider);

		SetupOptionCategories(mockOpt.Object);

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);

		var mockRoleOptions = new Mock<IRoleOptionsCollection>(IntPtr.Zero);
		var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
		mockGameOptions.SetupGet(g => g.RoleOptions).Returns(mockRoleOptions.Object);
		mockGameOptions.SetupGet(g => g.MapId).Returns(0);
		mockGameOptions.SetupGet(g => g.MaxPlayers).Returns(10);

		var mockOptionsMgr = new Mock<GameOptionsManager>(IntPtr.Zero);
		mockOptionsMgr.SetupGet(m => m.currentGameOptions).Returns(mockGameOptions.Object);
		var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
		mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockOptionsMgr.Object);
		MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;

		var mockAllControls = new Mock<Il2CppSystem.Collections.Generic.List<PlayerControl>>(IntPtr.Zero);
		mockAllControls.SetupGet(l => l.Count).Returns(0);
		var mockAllControlsHelper = new Mock<MockPlayerControlget_AllPlayerControlsHelper>();
		mockAllControlsHelper.Setup(h => h.Invoke()).Returns(mockAllControls.Object);
		MockPlayerControlget_AllPlayerControlsHelper.Instance = mockAllControlsHelper.Object;
	}

	private static void SetupOptionCategories(IOption defaultOpt)
	{
		foreach (var (combId, _) in ExtremeRoleManager.CombRole)
		{
			int catId = ExtremeRoleManager.GetCombRoleGroupId((CombinationRoleType)combId);
			EnsureCategory(catId, defaultOpt);
		}
		foreach (var role in ExtremeRoleManager.NormalRole.Values)
		{
			int catId = ExtremeRoleManager.GetRoleGroupId(role.Core.Id);
			EnsureCategory(catId, defaultOpt);
		}
		foreach (var role in ExtremeGhostRoleManager.AllGhostRole.Values)
		{
			int catId = ExtremeGhostRoleManager.GetRoleGroupId(role.Id);
			EnsureCategory(catId, defaultOpt);
		}
	}

	private static void EnsureCategory(int categoryId, IOption defaultOpt)
	{
		foreach (OptionTab tab in Enum.GetValues<OptionTab>())
		{
			if (!OptionManager.Instance.TryGetCategory(tab, categoryId, out _))
			{
				try
				{
					var pack = new OptionPack();
					pack.AddOption((int)RoleCommonOption.SpawnRate, defaultOpt);
					var cat = new OptionCategory(tab, categoryId, "TestCat", pack, null);
					OptionManager.Instance.RegisterOptionGroup(tab, cat);
				}
				catch
				{
				}
			}
		}
	}

	[Fact]
	public void PanelPageModelBase_CurPage_BoundsAndWrapAround_WorkCorrectly()
	{
		var model = new TestPagePanelModel();

		// Initially 0 pages -> CurPage returns 0
		Assert.Equal(0, model.PageNum);
		model.CurPage = 5;
		Assert.Equal(0, model.CurPage);

		// Add 3 pages
		model.AddDummyPages(3);
		Assert.Equal(3, model.PageNum);

		model.CurPage = 1;
		Assert.Equal(1, model.CurPage);

		// Wrap negative
		model.CurPage = -1;
		Assert.Equal(2, model.CurPage);

		// Wrap overflow
		model.CurPage = 4; // 4 % 3 = 1
		Assert.Equal(1, model.CurPage);
	}

	[Fact]
	public void PanelPageModelBase_ShowActiveOnly_And_GetInfoText_WorkCorrectly()
	{
		var model = new TestPagePanelModel();
		model.AddDummyPages(2);

		var (title, desc) = model.GetInfoText();
		Assert.Contains("Role0", title);

		// Change to ShowActiveOnly = true;
		model.ShowActiveOnly = true;
		model.UpdateVisual();

		// Call GetInfoText when active only is set
		var (activeTitle, activeDesc) = model.GetInfoText();
		Assert.NotNull(activeTitle);
	}

	[Fact]
	public void PanelPageModelBase_DefaultOptionToString_PropertiesAndToString_WorkCorrectly()
	{
		var mockInfo = new Mock<IOptionInfo>();
		mockInfo.SetupGet(i => i.IsHidden).Returns(false);
		mockInfo.SetupGet(i => i.CodeRemovedName).Returns("OptCodeName");

		var mockOption = new Mock<IOption>();
		mockOption.SetupGet(o => o.Info).Returns(mockInfo.Object);
		mockOption.SetupGet(o => o.IsChangeDefault).Returns(true);
		mockOption.SetupGet(o => o.IsViewActive).Returns(true);
		mockOption.SetupGet(o => o.TransedTitle).Returns("OptTitle");
		mockOption.SetupGet(o => o.TransedValue).Returns("OptValue");

		var helper = new RolePagePanelModelBase.DefaultOptionToString(mockOption.Object);

		Assert.True(helper.IsActive);
		string str = helper.ToString();
		Assert.Contains("OptTitle", str);
		Assert.Contains("OptValue", str);
	}

	[Fact]
	public void GlobalSettingInfoModel_UpdateVisual_DoesNotThrow()
	{
		var model = new GlobalSettingInfoModel();
		model.UpdateVisual();
	}

	[Fact]
	public void AllRoleInfoModel_GetInfoText_And_LiberalOptionToString_WorkCorrectly()
	{
		var model = new AllRoleInfoModel();
		model.UpdateVisual();
		var (info, desc) = model.GetInfoText();
		Assert.NotNull(info);

		var mockInfo = new Mock<IOptionInfo>();
		mockInfo.SetupGet(i => i.CodeRemovedName).Returns("LiberalOptCode");

		var mockOption = new Mock<IOption>();
		mockOption.SetupGet(o => o.Info).Returns(mockInfo.Object);
		mockOption.SetupGet(o => o.IsViewActive).Returns(true);
		mockOption.SetupGet(o => o.TransedTitle).Returns("LiberalOpt");
		mockOption.SetupGet(o => o.TransedValue).Returns("On");

		var globalList = new List<IOption> { mockOption.Object };
		var specificList = new List<IOption> { mockOption.Object };

		var helper = new AllRoleInfoModel.LiberalOptionToString(() => "SpawnSettingStr", globalList, specificList);

		Assert.True(helper.IsActive);
		string str = helper.ToString();
		Assert.Contains("SpawnSettingStr", str);
		Assert.Contains("LiberalOpt", str);
	}

	[Fact]
	public void AllGhostRoleInfoModel_GetInfoText_WorksCorrectly()
	{
		var model = new AllGhostRoleInfoModel();
		model.UpdateVisual();
		var (info, desc) = model.GetInfoText();
		Assert.NotNull(info);
	}

	[Fact]
	public void LocalGhostRoleInfoModel_GetInfoText_WhenPlayerAlive_ReturnsAliveText()
	{
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
		mockLocalHelper.Setup(x => x.Invoke()).Returns(mockPlayer.Object);
		MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

		var model = new LocalGhostRoleInfoModel();
		model.UpdateVisual();

		var (info, option) = model.GetInfoText();
		Assert.Contains("yourAliveNow", info);
		Assert.Equal(string.Empty, option);
	}

	[Fact]
	public void LocalGhostRoleInfoModel_GetInfoText_WhenPlayerDeadNoRole_ReturnsNoAssignText()
	{
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
		mockLocalHelper.Setup(x => x.Invoke()).Returns(mockPlayer.Object);
		MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

		var model = new LocalGhostRoleInfoModel();
		var (info, option) = model.GetInfoText();

		Assert.Contains("yourNoAssignGhostRole", info);
		Assert.Equal("", option);
	}

	[Fact]
	public void LocalRoleInfoModel_UpdateVisual_DoesNotThrow()
	{
		var model = new LocalRoleInfoModel();
		model.UpdateVisual();
	}

	[Fact]
	public void LiberalInfoModel_GetInfoText_And_UpdateVisual_WorkCorrectly()
	{
		var model = new LiberalInfoModel();
		model.UpdateVisual();
		var (info, option) = model.GetInfoText();
		Assert.NotNull(info);
		Assert.NotNull(option);
	}

	[Fact]
	public void IInfoOverlayPanelModel_ToHudStringWithChildren_FormatsCorrectly()
	{
		var mockInfo = new Mock<IOptionInfo>();
		mockInfo.SetupGet(i => i.CodeRemovedName).Returns("TestOptCode");

		var mockOption = new Mock<IOption>();
		mockOption.SetupGet(o => o.Info).Returns(mockInfo.Object);
		mockOption.SetupGet(o => o.IsViewActive).Returns(true);
		mockOption.SetupGet(o => o.TransedTitle).Returns("TestOpt");
		mockOption.SetupGet(o => o.TransedValue).Returns("TestVal");

		string hudStr = TestPagePanelModel.TestToHudString(mockOption.Object);
		Assert.Contains("TestOpt", hudStr);
		Assert.Contains("TestVal", hudStr);
	}

	private sealed class TestPagePanelModel : RolePagePanelModelBase
	{
		public void AddDummyPages(int count)
		{
			for (int i = 0; i < count; i++)
			{
				var mockOpt = new Mock<IOptionToStringHelper>();
				mockOpt.SetupGet(o => o.IsActive).Returns(i % 2 == 0);
				mockOpt.Setup(o => o.ToString()).Returns($"OptString{i}");

				AddPage(new RoleInfo($"Role{i}", $"Desc{i}", mockOpt.Object));
			}
		}

		protected override void CreateAllRoleText()
		{
		}

		public static string TestToHudString(IOption option)
		{
			return IInfoOverlayPanelModel.ToHudStringWithChildren(option);
		}
	}
}
