using System;
using System.Reflection;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection("UnityMock")]
public sealed class ExtremeGameEndCheckerTests
{
    public ExtremeGameEndCheckerTests()
    {
        MockSetupHelper.SetupCommonMocks();
        SetupShipStatusAndGameData();
    }

    private static void SetupShipStatusAndGameData()
    {
        var mockShipStatus = new Mock<ShipStatus>();
        var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
        mockShipHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
        MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

        var dict = new Mock<Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, ISystemType>>(IntPtr.Zero);
        mockShipStatus.SetupGet(s => s.Systems).Returns(dict.Object);

        var mockData = new Mock<GameData>();
        var mockDataHelper = new Mock<MockGameDataget_InstanceHelper>();
        mockDataHelper.Setup(h => h.Invoke()).Returns(mockData.Object);
        MockGameDataget_InstanceHelper.Instance = mockDataHelper.Object;

        var mockPlayers = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
        mockPlayers.SetupGet(p => p.Count).Returns(0);
        mockData.SetupGet(d => d.AllPlayers).Returns(mockPlayers.Object);

        if (ExtremeRoles.GameMode.ExtremeGameModeManager.Instance == null)
        {
            ExtremeRoles.GameMode.ExtremeGameModeManager.Create(AmongUs.GameOptions.GameModes.Normal);
        }

        if (ExtremeRoles.Module.SystemType.ExtremeSystemTypeManager.Instance == null)
        {
            var manager = (ExtremeRoles.Module.SystemType.ExtremeSystemTypeManager)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ExtremeRoles.Module.SystemType.ExtremeSystemTypeManager));
            FieldInfo? field = typeof(ExtremeRoles.Module.SystemType.ExtremeSystemTypeManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
            field?.SetValue(null, manager);
        }
    }

    [Fact]
    public void Check_WhenCheckerReturnsGameEnd_CallsGameIsEndAndCleanUp()
    {
        var mockChecker = new Mock<IGameEndChecker>();
        GameOverReason expectedReason = GameOverReason.CrewmatesByTask;
        mockChecker.Setup(c => c.TryCheckGameEnd(out expectedReason)).Returns(true);
        mockChecker.Setup(c => c.CleanUp()).Verifiable();

        ExtremeGameEndChecker gameEndChecker = (ExtremeGameEndChecker)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ExtremeGameEndChecker));

        FieldInfo? checkersField = typeof(ExtremeGameEndChecker).GetField("checkers", BindingFlags.NonPublic | BindingFlags.Instance);
        checkersField?.SetValue(gameEndChecker, new IGameEndChecker[] { mockChecker.Object });

        FieldInfo? statsField = typeof(ExtremeGameEndChecker).GetField("statistics", BindingFlags.NonPublic | BindingFlags.Instance);
        statsField?.SetValue(gameEndChecker, new PlayerStatistics());

        gameEndChecker.Check();

        mockChecker.Verify(c => c.CleanUp(), Times.Once());
    }
}
