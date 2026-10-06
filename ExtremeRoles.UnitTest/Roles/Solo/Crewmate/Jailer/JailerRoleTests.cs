using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using InnerNet;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.JailerTests;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class JailerRoleTests
{
	private readonly Mock<AmongUsClient> clientMock;

	public JailerRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		clientMock = MockSetupHelper.SetupAmongUsClientMock();
		SetLobbyMode(false);
	}

	private void SetLobbyMode(bool isLobby)
	{
		if (isLobby)
		{
			clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.NotJoined);
			var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
		else
		{
			clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns((LobbyBehaviour)null!);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
	}

	[Fact]
	public void Constructor_InitializesCorrectly()
	{
		// Act
		var jailer = new Jailer();

		// Assert
		Assert.NotNull(jailer);
		Assert.Equal(ExtremeRoleId.Jailer, jailer.Core.Id);
		Assert.Equal(RoleTypes.Crewmate, jailer.NoneAwakeRole);
	}

	[Fact]
	public void CreateRoleAllOption_CreatesExpectedOptions()
	{
		// Arrange
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Jailer);
		var jailer = new Jailer();

		// Act
		jailer.CreateRoleAllOption();

		// Assert
		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);

		Assert.True(jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var awakeTaskGageOpt));
		Assert.NotNull(awakeTaskGageOpt);

		Assert.True(jailer.Loader.TryGet(Jailer.Option.TargetMode, out var targetModeOpt));
		Assert.NotNull(targetModeOpt);
	}

	[Theory]
	[InlineData(true, false, true)]
	[InlineData(false, true, true)]
	[InlineData(false, false, false)]
	public void IsAwake_ReturnsExpectedValue(bool isLobby, bool awakeRoleState, bool expectedIsAwake)
	{
		// Arrange
		SetLobbyMode(isLobby);
		var jailer = new Jailer();
		jailer.CreateRoleAllOption();

		if (jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var gageOpt) && gageOpt != null)
		{
			gageOpt.Selection = awakeRoleState ? 0 : 7; // 0% -> awakeRole = true
		}
		if (jailer.Loader.TryGet(Jailer.Option.AwakeDeadPlayerNum, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = awakeRoleState ? 0 : 5;
		}

		jailer.Initialize();

		// Act
		bool isAwake = jailer.IsAwake;

		// Assert
		Assert.Equal(expectedIsAwake, isAwake);
	}

	[Fact]
	public void GetFakeOptionString_ReturnsEmptyString()
	{
		// Arrange
		var jailer = new Jailer();

		// Act
		string result = jailer.GetFakeOptionString();

		// Assert
		Assert.Equal("", result);
	}

	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	public void GetColoredRoleName_WhenAwakeOrTruthColor_ReturnsColoredRoleName(bool isTruthColor, bool isAwake)
	{
		// Arrange
		SetLobbyMode(isAwake);
		var jailer = new Jailer();

		// Act
		string roleName = jailer.GetColoredRoleName(isTruthColor);

		// Assert
		Assert.NotNull(roleName);
		Assert.Contains("Jailer", roleName);
	}

	[Fact]
	public void GetColoredRoleName_WhenNotAwakeAndNotTruthColor_ReturnsWhiteCrewmateName()
	{
		// Arrange
		SetLobbyMode(false);
		var jailer = new Jailer();
		jailer.CreateRoleAllOption();

		if (jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var gageOpt) && gageOpt != null)
		{
			gageOpt.Selection = 7; // 70%
		}
		if (jailer.Loader.TryGet(Jailer.Option.AwakeDeadPlayerNum, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = 5;
		}
		jailer.Initialize();

		// Act
		string roleName = jailer.GetColoredRoleName(false);

		// Assert
		Assert.NotNull(roleName);
		Assert.Contains("Crewmate", roleName);
	}

	[Fact]
	public void GetFullDescription_WhenAwake_ReturnsJailerFullDescription()
	{
		// Arrange
		SetLobbyMode(true);
		var jailer = new Jailer();

		// Act
		string description = jailer.GetFullDescription();

		// Assert
		Assert.NotNull(description);
		Assert.Equal("JailerFullDescription", description);
	}

	[Fact]
	public void GetFullDescription_WhenNotAwake_ReturnsCrewmateFullDescription()
	{
		// Arrange
		SetLobbyMode(false);
		var jailer = new Jailer();
		jailer.CreateRoleAllOption();

		if (jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var gageOpt) && gageOpt != null)
		{
			gageOpt.Selection = 7;
		}
		if (jailer.Loader.TryGet(Jailer.Option.AwakeDeadPlayerNum, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = 5;
		}
		jailer.Initialize();

		// Act
		string description = jailer.GetFullDescription();

		// Assert
		Assert.NotNull(description);
		Assert.Equal("CrewmateFullDescription", description);
	}

	[Fact]
	public void GetImportantText_WhenAwake_ReturnsJailerImportantText()
	{
		// Arrange
		SetLobbyMode(true);
		var jailer = new Jailer();

		// Act
		string text = jailer.GetImportantText(true);

		// Assert
		Assert.NotNull(text);
		Assert.Contains("Jailer", text);
	}

	[Fact]
	public void GetImportantText_WhenNotAwake_ReturnsCrewmateImportantText()
	{
		// Arrange
		SetLobbyMode(false);
		var jailer = new Jailer();
		jailer.CreateRoleAllOption();

		if (jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var gageOpt) && gageOpt != null)
		{
			gageOpt.Selection = 7;
		}
		if (jailer.Loader.TryGet(Jailer.Option.AwakeDeadPlayerNum, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = 5;
		}
		jailer.Initialize();

		// Act
		string text = jailer.GetImportantText(true);

		// Assert
		Assert.NotNull(text);
		Assert.Contains("crewImportantText", text);
	}

	[Fact]
	public void GetIntroDescription_WhenAwake_ReturnsBaseIntroDescription()
	{
		// Arrange
		SetLobbyMode(true);
		var jailer = new Jailer();

		// Act
		string desc = jailer.GetIntroDescription();

		// Assert
		Assert.NotNull(desc);
		Assert.Contains("JailerIntroDescription", desc);
	}

	[Fact]
	public void GetIntroDescription_WhenNotAwake_ReturnsCrewmateIntroDescription()
	{
		// Arrange
		SetLobbyMode(false);
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();

		var mockRole = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRole.SetupGet(r => r.Blurb).Returns("Crewmate Blurb");

		var mockInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockInfo.SetupGet(i => i.Role).Returns(mockRole.Object);
		localPlayerMock.SetupGet(p => p.Data).Returns(mockInfo.Object);

		var jailer = new Jailer();
		jailer.CreateRoleAllOption();

		if (jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var gageOpt) && gageOpt != null)
		{
			gageOpt.Selection = 7;
		}
		if (jailer.Loader.TryGet(Jailer.Option.AwakeDeadPlayerNum, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = 5;
		}
		jailer.Initialize();

		// Act
		string desc = jailer.GetIntroDescription();

		// Assert
		Assert.NotNull(desc);
		Assert.Contains("Crewmate Blurb", desc);
	}

	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	public void GetNameColor_WhenAwakeOrTruthColor_ReturnsRoleColor(bool isTruthColor, bool isAwake)
	{
		// Arrange
		SetLobbyMode(isAwake);
		var jailer = new Jailer();

		// Act
		Color color = jailer.GetNameColor(isTruthColor);

		// Assert
		Assert.Equal(ColorPalette.JailerSapin, color);
	}

	[Fact]
	public void GetNameColor_WhenNotAwakeAndNotTruthColor_ReturnsWhite()
	{
		// Arrange
		SetLobbyMode(false);
		var jailer = new Jailer();
		jailer.CreateRoleAllOption();

		if (jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var gageOpt) && gageOpt != null)
		{
			gageOpt.Selection = 7;
		}
		if (jailer.Loader.TryGet(Jailer.Option.AwakeDeadPlayerNum, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = 5;
		}
		jailer.Initialize();

		// Act
		Color color = jailer.GetNameColor(false);

		// Assert
		Assert.Equal(Palette.White, color);
	}

	[Fact]
	public void ResetOnMeetingStartAndEnd_ExecutesWithoutError()
	{
		// Arrange
		var jailer = new Jailer();

		// Act & Assert
		jailer.ResetOnMeetingStart();
		jailer.ResetOnMeetingEnd(null);
	}
}
