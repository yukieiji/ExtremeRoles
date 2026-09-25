using System;
using System.Collections.Generic;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate.Loner;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Loner;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class LonerRoleTests
{
	public LonerRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		SetupTranslationControllerMock();

		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		var mockExileControllerHelper = new Mock<MockExileControllerget_InstanceHelper>();
		mockExileControllerHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockExileControllerget_InstanceHelper.Instance = mockExileControllerHelper.Object;

		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockList.SetupGet(l => l.Count).Returns(0);
		mockGameData.SetupGet(g => g.AllPlayers).Returns(mockList.Object);

		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();
	}

	private static void SetupTranslationControllerMock()
	{
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
	}

	[Fact]
	public void Constructor_SetsRoleProperties()
	{
		// Act
		var loner = new LonerRole();

		// Assert
		Assert.Equal(ExtremeRoleId.Loner, loner.Core.Id);
		Assert.Equal(ColorPalette.LonerMidnightblue, loner.Core.Color);
	}

	[Fact]
	public void CreateRoleAllOption_CreatesOptions()
	{
		// Arrange
		var loner = new LonerRole();

		// Act
		loner.CreateRoleAllOption();

		// Assert
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Loner);
		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);
	}

	[Fact]
	public void Initialize_ConfiguresStatusAndAbilityClass()
	{
		// Arrange
		var loner = new LonerRole();
		loner.CreateRoleAllOption();

		// Act
		loner.Initialize();

		// Assert
		Assert.NotNull(loner.Status);
		Assert.IsType<LonerStatusModel>(loner.Status);
		Assert.NotNull(loner.AbilityClass);
		Assert.IsType<LonerAbilityHandler>(loner.AbilityClass);
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenTargetIsLocalPlayer_ReturnsStressInfo()
	{
		// Arrange
		var loner = new LonerRole();
		loner.CreateRoleAllOption();
		loner.Initialize();

		// Act
		string nameTag = loner.GetRolePlayerNameTag(loner, 1);

		// Assert
		Assert.Equal("(0/10)", nameTag);
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenTargetIsNotLocalPlayer_ReturnsBaseTag()
	{
		// Arrange
		var loner = new LonerRole();
		loner.CreateRoleAllOption();
		loner.Initialize();

		// Act
		string nameTag = loner.GetRolePlayerNameTag(loner, 2);

		// Assert
		Assert.Equal("", nameTag);
	}

	[Fact]
	public void GetFullDescription_ReturnsFormattedDescription()
	{
		// Arrange
		var loner = new LonerRole();
		loner.CreateRoleAllOption();
		loner.Initialize();

		// Act
		string description = loner.GetFullDescription();

		// Assert
		Assert.NotNull(description);
	}

	[Fact]
	public void ResetOnMeetingStart_ExecutesWithoutError()
	{
		// Arrange
		var loner = new LonerRole();
		loner.CreateRoleAllOption();
		loner.Initialize();

		// Act & Assert
		loner.ResetOnMeetingStart();
	}

	[Fact]
	public void ResetOnMeetingEnd_ExecutesWithoutError()
	{
		// Arrange
		var loner = new LonerRole();

		// Act & Assert
		loner.ResetOnMeetingEnd(null);
	}

	[Fact]
	public void Update_ExecutesWithoutError()
	{
		// Arrange
		var loner = new LonerRole();
		loner.CreateRoleAllOption();
		loner.Initialize();

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();

		// Act & Assert
		loner.Update(mockPlayer.Object);
	}
}
