using System;
using System.Reflection;
using AmongUs.GameOptions;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
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
public class LoggerRoleTests
{
	public LoggerRoleTests()
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
	public void Constructor_InitializesLoggerRoleCorrectly()
	{
		// Arrange & Act
		var logger = new LoggerRole();

		// Assert
		Assert.Equal(ExtremeRoleId.Logger, logger.Core.Id);
		Assert.Equal(ExtremeRoleType.Crewmate, logger.Core.Team);
		Assert.Equal(ColorPalette.LoggerGreen, logger.Core.Color);
	}

	[Fact]
	public void RoleSpecificInit_CreatesOptionsAndInitializes()
	{
		// Arrange
		var logger = new LoggerRole();

		// Act
		logger.CreateRoleAllOption();

		// Assert
		Assert.NotNull(logger.Core);
	}

	[Fact]
	public void DetectorData_InitializationAndProperties()
	{
		// Arrange & Act
		var data = new LoggerRole.DetectorData(1);

		// Assert
		Assert.Equal(1, data.IndexNumber);
		Assert.NotNull(data.Logs);
		Assert.Empty(data.Logs);

		data.Logs.Add("Test Log");
		Assert.Single(data.Logs);
		Assert.Equal("Test Log", data.Logs[0]);
	}
}
