using System;
using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameResult;
using Moq;
using Xunit;

using Il2CppPlayerList = Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>;

namespace ExtremeRoles.UnitTest.Module.GameResult;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ExtremeGameResultManagerTests
{
    public ExtremeGameResultManagerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupLogger("ExtremeGameResultManagerTests");
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        if (ExtremeRolesPlugin.ShipState == null)
        {
            var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty("ShipState", BindingFlags.Public | BindingFlags.Static);
            shipStateProp?.SetValue(null, new ExtremeShipStatus());
        }
    }

    private static NetworkedPlayerInfo CreateMockPlayerInfo(byte playerId, string name = "Player")
    {
        var mockPlayer = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
        mockPlayer.SetupGet(p => p.PlayerName).Returns($"{name}{playerId}");
        return mockPlayer.Object;
    }

    [Fact]
    public void CreateTaskInfo_PopulatesPlayerTaskInfoAndWinnerPool()
    {
        var mockGameData = MockSetupHelper.SetupGameDataMock();

        var player1 = CreateMockPlayerInfo(1, "Player1");
        var player2 = CreateMockPlayerInfo(2, "Player2");

        var mockList = new Mock<Il2CppPlayerList>(IntPtr.Zero);
        var players = new[] { player1, player2 };

        mockList.SetupGet(l => l.Count).Returns(players.Length);
        mockList.Setup(l => l[It.IsAny<int>()]).Returns((int i) => players[i]);

        mockGameData.SetupGet(g => g.AllPlayers).Returns(mockList.Object);

        var manager = new ExtremeGameResultManager();

        manager.CreateTaskInfo();

        var taskInfoField = typeof(ExtremeGameResultManager).GetField("playerTaskInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        var playerTaskInfo = (Dictionary<byte, ExtremeGameResultManager.TaskInfo>)taskInfoField!.GetValue(manager)!;

        Assert.Equal(2, playerTaskInfo.Count);
        Assert.True(playerTaskInfo.ContainsKey(1));
        Assert.True(playerTaskInfo.ContainsKey(2));

        var winnerField = typeof(ExtremeGameResultManager).GetField("winner", BindingFlags.NonPublic | BindingFlags.Instance);
        var winner = (WinnerContainer)winnerField!.GetValue(manager)!;
        var poolField = typeof(WinnerContainer).GetField("allWinnerPool", BindingFlags.NonPublic | BindingFlags.Instance);
        var pool = (Dictionary<byte, CachedPlayerData>)poolField!.GetValue(winner)!;

        Assert.Equal(2, pool.Count);
        Assert.True(pool.ContainsKey(1));
        Assert.True(pool.ContainsKey(2));

        var winnerResult = manager.Winner;
        Assert.NotNull(winnerResult.Winner);
        Assert.NotNull(winnerResult.PlusedWinner);
    }
}
