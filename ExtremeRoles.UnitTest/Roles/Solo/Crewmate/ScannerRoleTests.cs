using System;
using System.Reflection;
using AmongUs.GameOptions;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

using ExtremeRoles.Module;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ScannerRoleTests
{
	public ScannerRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupPaletteHelpers();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupLobbyMock();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupGameDataMock();
		MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		MockSetupHelper.SetupOptionManager();
		SetupGameOptionsManagerMock();

		var shipStateField = typeof(ExtremeRolesPlugin).GetField("<ShipState>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
		shipStateField?.SetValue(null, new ExtremeShipStatus());

		ExtremeRoleManager.GameRole.Clear();

		var clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);
	}

	private static void SetupGameOptionsManagerMock()
	{
		if (MockGameOptionsManagerget_InstanceHelper.Instance == null)
		{
			var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
			var mockGameOptionsManager = new Mock<GameOptionsManager>(IntPtr.Zero);
			mockGameOptionsManager.SetupGet(g => g.CurrentGameOptions).Returns(mockGameOptions.Object);

			var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
			mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockGameOptionsManager.Object);
			MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
		}
	}

	[Fact]
	public void Constructor_InitializesScannerRoleCorrectly()
	{
		// Arrange & Act
		var scanner = new ScannerRole();

		// Assert
		Assert.Equal(ExtremeRoleId.Scanner, scanner.Core.Id);
		Assert.Equal(ExtremeRoleType.Crewmate, scanner.Core.Team);
		Assert.Equal(ColorPalette.ScannerCyan, scanner.Core.Color);
	}

	[Fact]
	public void Initialize_CreatesOptionsAndInitsRole()
	{
		// Arrange
		var scanner = new ScannerRole();
		scanner.CreateRoleAllOption();

		// Act
		scanner.Initialize();

		// Assert
		Assert.Equal(ExtremeRoleId.Scanner, scanner.Core.Id);
	}

	[Fact]
	public void IsAbilityUse_WhenInCommonUseState_ReturnsTrue()
	{
		// Arrange
		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockLocalPlayer.Object);
		mockLocalPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);

		var scanner = new ScannerRole();
		scanner.CreateRoleAllOption();
		scanner.Initialize();

		// Act
		bool canUse = scanner.IsAbilityUse();

		// Assert
		Assert.True(canUse);
	}

	[Fact]
	public void UseAbility_WithoutSelectedRoom_ReturnsFalse()
	{
		// Arrange
		var scanner = new ScannerRole();
		scanner.CreateRoleAllOption();
		scanner.Initialize();

		// Act
		bool result = scanner.UseAbility();

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void ResetOnMeetingStart_ResetsStateWithoutErrors()
	{
		// Arrange
		var scanner = new ScannerRole();
		scanner.CreateRoleAllOption();
		scanner.Initialize();

		// Act
		scanner.ResetOnMeetingStart();

		// Assert
		Assert.False(scanner.UseAbility());
	}

	[Fact]
	public void ResetOnMeetingEnd_ResetsStateWithoutErrors()
	{
		// Arrange
		var scanner = new ScannerRole();
		scanner.CreateRoleAllOption();
		scanner.Initialize();

		// Act
		scanner.ResetOnMeetingEnd(null);

		// Assert
		Assert.False(scanner.UseAbility());
	}

	[Fact]
	public void SetSelectedRoomFieldAndUseAbility_SetsSelectedRoomAndActivatesScanning()
	{
		// Arrange
		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(Vector2.zero);

		var scanner = new ScannerRole();
		scanner.CreateRoleAllOption();
		scanner.Initialize();

		var roomField = typeof(ScannerRole).GetField("selectedRoom", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(roomField);
		roomField!.SetValue(scanner, SystemTypes.Hallway);

		// Act
		bool result = scanner.UseAbility();

		// Assert
		Assert.True(result);
	}
}
