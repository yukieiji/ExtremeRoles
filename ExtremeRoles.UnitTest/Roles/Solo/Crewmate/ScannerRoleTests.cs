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
		mockTranslation.Setup(t => t.GetString(It.IsAny<SystemTypes>()))
			.Returns((SystemTypes room) => room.ToString());
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
	public void UseAbility_WhenNoRoomSelected_ReturnsFalseAndScanningIsFalse()
	{
		// Arrange
		var scanner = new ScannerRole();
		scanner.CreateRoleAllOption();
		scanner.Initialize();

		// Act
		bool useResult = scanner.UseAbility();

		// Assert
		Assert.False(useResult);

		var isScanningField = typeof(ScannerRole).GetField("isScanning", BindingFlags.NonPublic | BindingFlags.Instance);
		bool isScanning = (bool)isScanningField!.GetValue(scanner)!;
		Assert.False(isScanning);
	}

	[Fact]
	public void UseAbility_WhenRoomSelected_ReturnsTrueAndSetsIsScanningTrue()
	{
		// Arrange
		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(1.0f, 2.0f));

		var scanner = new ScannerRole();
		scanner.CreateRoleAllOption();
		scanner.Initialize();

		var roomField = typeof(ScannerRole).GetField("selectedRoom", BindingFlags.NonPublic | BindingFlags.Instance);
		roomField!.SetValue(scanner, SystemTypes.Cafeteria);

		// Act
		bool useResult = scanner.UseAbility();

		// Assert
		Assert.True(useResult);

		var isScanningField = typeof(ScannerRole).GetField("isScanning", BindingFlags.NonPublic | BindingFlags.Instance);
		bool isScanning = (bool)isScanningField!.GetValue(scanner)!;
		Assert.True(isScanning);

		var prevPosField = typeof(ScannerRole).GetField("prevPlayerPos", BindingFlags.NonPublic | BindingFlags.Instance);
		Vector2 prevPos = (Vector2)prevPosField!.GetValue(scanner)!;
		Assert.Equal(new Vector2(1.0f, 2.0f), prevPos);
	}

	[Fact]
	public void ResetOnMeetingStart_ResetsScanningAndSelectedRoom()
	{
		// Arrange
		var scanner = new ScannerRole();
		scanner.CreateRoleAllOption();
		scanner.Initialize();

		var roomField = typeof(ScannerRole).GetField("selectedRoom", BindingFlags.NonPublic | BindingFlags.Instance);
		roomField!.SetValue(scanner, SystemTypes.Electrical);

		var isScanningField = typeof(ScannerRole).GetField("isScanning", BindingFlags.NonPublic | BindingFlags.Instance);
		isScanningField!.SetValue(scanner, true);

		// Act
		scanner.ResetOnMeetingStart();

		// Assert
		SystemTypes? selectedRoom = (SystemTypes?)roomField.GetValue(scanner);
		bool isScanning = (bool)isScanningField.GetValue(scanner)!;

		Assert.Null(selectedRoom);
		Assert.False(isScanning);
	}

	[Fact]
	public void ResetOnMeetingEnd_ResetsScanningAndSelectedRoom()
	{
		// Arrange
		var scanner = new ScannerRole();
		scanner.CreateRoleAllOption();
		scanner.Initialize();

		var roomField = typeof(ScannerRole).GetField("selectedRoom", BindingFlags.NonPublic | BindingFlags.Instance);
		roomField!.SetValue(scanner, SystemTypes.Nav);

		var isScanningField = typeof(ScannerRole).GetField("isScanning", BindingFlags.NonPublic | BindingFlags.Instance);
		isScanningField!.SetValue(scanner, true);

		// Act
		scanner.ResetOnMeetingEnd(null);

		// Assert
		SystemTypes? selectedRoom = (SystemTypes?)roomField.GetValue(scanner);
		bool isScanning = (bool)isScanningField.GetValue(scanner)!;

		Assert.Null(selectedRoom);
		Assert.False(isScanning);
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
}
