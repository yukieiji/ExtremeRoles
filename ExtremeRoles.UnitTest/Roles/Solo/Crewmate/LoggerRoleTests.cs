using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.Interface;
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

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.PlayerId).Returns((byte)1);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockLocalPlayer.Object);
		mockLocalPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

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
	public void RoleSpecificInit_LoadsOptionValuesCorrectly()
	{
		// Arrange
		var logger = new LoggerRole();
		logger.CreateRoleAllOption();

		// Act
		logger.Initialize();

		// Assert
		var rangeField = typeof(LoggerRole).GetField("range", BindingFlags.NonPublic | BindingFlags.Instance);
		var isVisibleAllField = typeof(LoggerRole).GetField("isVisibleAll", BindingFlags.NonPublic | BindingFlags.Instance);

		float range = (float)rangeField?.GetValue(logger)!;
		bool isVisibleAll = (bool)isVisibleAllField?.GetValue(logger)!;

		Assert.Equal(2.5f, range);
		Assert.True(isVisibleAll);
	}

	[Fact]
	public void DetectorData_InitializationAndLogAddition_UpdatesLogList()
	{
		// Arrange
		var data = new LoggerRole.DetectorData(1);

		// Act
		data.Logs.Add("Player 1 passed near detector");

		// Assert
		int logCount = data.Logs.Count;
		string logMessage = data.Logs[0];

		Assert.Equal(1, data.IndexNumber);
		Assert.Equal(1, logCount);
		Assert.Equal("Player 1 passed near detector", logMessage);
	}

	[Fact]
	public void ResetOnMeetingStart_AggregatesActiveAndArchivedLogs_AndClearsDetectorState()
	{
		// Arrange
		MeetingReporter.Reset();
		var reporter = MeetingReporter.Instance;

		var logger = new LoggerRole();
		logger.CreateRoleAllOption();
		logger.Initialize();

		var archivedListField = typeof(LoggerRole).GetField("archivedDetectorLogs", BindingFlags.NonPublic | BindingFlags.Instance);
		var archivedList = (List<LoggerRole.DetectorData>)archivedListField?.GetValue(logger)!;

		var archivedData = new LoggerRole.DetectorData(1);
		archivedData.Logs.Add("Archived Log Entry");
		archivedList.Add(archivedData);

		var activeListField = typeof(LoggerRole).GetField("activeDetectors", BindingFlags.NonPublic | BindingFlags.Instance);
		var activeList = (List<LoggerRole.DetectorBehavior>)activeListField?.GetValue(logger)!;

		var consoleObj = new GameObject();
		var detector = new LoggerRole.DetectorBehavior(logger, 2, consoleObj);
		detector.Logs.Add("Active Log Entry");
		activeList.Add(detector);

		// Act
		logger.ResetOnMeetingStart();

		// Assert
		int archivedCountAfter = archivedList.Count;
		int activeLogsCountAfter = detector.Logs.Count;

		Assert.Equal(0, archivedCountAfter);
		Assert.Equal(0, activeLogsCountAfter);
		Assert.True(reporter.HasChatReport);
	}

	[Fact]
	public void DetectorBehavior_Use_ArchivesLogsAndRestoresAbilityCharge()
	{
		// Arrange
		var logger = new LoggerRole();
		logger.CreateRoleAllOption();
		logger.Initialize();

		var consoleObj = new GameObject();
		var detector = new LoggerRole.DetectorBehavior(logger, 1, consoleObj);
		detector.Logs.Add("Test Event");

		var activeList = (List<LoggerRole.DetectorBehavior>)typeof(LoggerRole).GetField("activeDetectors", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(logger)!;
		activeList.Add(detector);

		// Act
		detector.Use();

		// Assert
		Assert.DoesNotContain(detector, activeList);

		var archivedList = (List<LoggerRole.DetectorData>)typeof(LoggerRole).GetField("archivedDetectorLogs", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(logger)!;
		Assert.Single(archivedList);
		Assert.Equal(1, archivedList[0].IndexNumber);
		Assert.Single(archivedList[0].Logs);
		Assert.Equal("Test Event", archivedList[0].Logs[0]);
	}
}
