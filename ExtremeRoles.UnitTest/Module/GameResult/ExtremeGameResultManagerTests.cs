using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

using Il2CppPlayerList = Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>;

namespace ExtremeRoles.UnitTest.Module.GameResult;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ExtremeGameResultManagerTests
{
    private sealed class DummySingleRole : SingleRoleBase
    {
        public DummySingleRole(RoleCore core)
        {
            var field = typeof(SingleRoleBase).GetField("<Core>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(this, core);
        }

        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) { }
        protected override void RoleSpecificInit() { }
    }

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

        if (ExtremeGameModeManager.Instance == null)
        {
            ExtremeGameModeManager.Create(AmongUs.GameOptions.GameModes.Normal);
        }
    }

    private static NetworkedPlayerInfo CreateMockPlayerInfo(byte playerId, string name)
    {
        var mockPlayer = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
        mockPlayer.SetupGet(p => p.PlayerName).Returns(name);
        return mockPlayer.Object;
    }

    private static CachedPlayerData CreateMockCachedPlayerData(string playerName)
    {
        var mock = new Mock<CachedPlayerData>(IntPtr.Zero);
        mock.SetupGet(c => c.PlayerName).Returns(playerName);
        return mock.Object;
    }

    private static void SetupMockCachedWinners(CachedPlayerData[] cachedWinnersList)
    {
        var mockWinners = new Mock<Il2CppSystem.Collections.Generic.List<CachedPlayerData>>(IntPtr.Zero);
        mockWinners.SetupGet(w => w.Count).Returns(cachedWinnersList.Length);
        mockWinners.Setup(w => w[It.IsAny<int>()]).Returns((int i) => cachedWinnersList[i]);
        mockWinners.Setup(w => w.ToArray()).Returns(new Il2CppReferenceArray<CachedPlayerData>(cachedWinnersList));

        var mockEnum = new Mock<Il2CppSystem.Collections.Generic.List<CachedPlayerData>.Enumerator>(IntPtr.Zero);
        mockEnum.Setup(e => e.MoveNext()).Returns(false);
        mockWinners.Setup(w => w.GetEnumerator()).Returns(mockEnum.Object);

        var mockWinnersHelper = new Mock<MockEndGameResultget_CachedWinnersHelper>();
        mockWinnersHelper.Setup(h => h.Invoke()).Returns(mockWinners.Object);
        MockEndGameResultget_CachedWinnersHelper.Instance = mockWinnersHelper.Object;
    }

    [Fact]
    public void CreateTaskInfo_SinglePlayer_PopulatesTaskInfoAndWinnerPoolWithCorrectValues()
    {
        var mockGameData = MockSetupHelper.SetupGameDataMock();

        var player1 = CreateMockPlayerInfo(1, "Alice");

        var mockList = new Mock<Il2CppPlayerList>(IntPtr.Zero);
        var players = new[] { player1 };

        mockList.SetupGet(l => l.Count).Returns(players.Length);
        mockList.Setup(l => l[It.IsAny<int>()]).Returns((int i) => players[i]);

        mockGameData.SetupGet(g => g.AllPlayers).Returns(mockList.Object);

        var manager = new ExtremeGameResultManager();

        manager.CreateTaskInfo();

        var taskInfoField = typeof(ExtremeGameResultManager).GetField("playerTaskInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        var playerTaskInfo = (Dictionary<byte, ExtremeGameResultManager.TaskInfo>)taskInfoField!.GetValue(manager)!;

        Assert.Single(playerTaskInfo);
        Assert.True(playerTaskInfo.TryGetValue(1, out var info1));
        Assert.Equal(0, info1.CompletedTask);
        Assert.Equal(0, info1.TotalTask);

        var winnerField = typeof(ExtremeGameResultManager).GetField("winner", BindingFlags.NonPublic | BindingFlags.Instance);
        var winner = (WinnerContainer)winnerField!.GetValue(manager)!;
        var poolField = typeof(WinnerContainer).GetField("allWinnerPool", BindingFlags.NonPublic | BindingFlags.Instance);
        var pool = (Dictionary<byte, CachedPlayerData>)poolField!.GetValue(winner)!;

        Assert.Single(pool);
        Assert.True(pool.ContainsKey(1));
    }

    [Fact]
    public void CreateTaskInfo_MultiplePlayers_PopulatesTaskInfoAndWinnerPoolForMultiplePlayers()
    {
        var mockGameData = MockSetupHelper.SetupGameDataMock();

        var player1 = CreateMockPlayerInfo(1, "Alice");
        var player2 = CreateMockPlayerInfo(2, "Bob");
        var player3 = CreateMockPlayerInfo(3, "Charlie");

        var mockList = new Mock<Il2CppPlayerList>(IntPtr.Zero);
        var players = new[] { player1, player2, player3 };

        mockList.SetupGet(l => l.Count).Returns(players.Length);
        mockList.Setup(l => l[It.IsAny<int>()]).Returns((int i) => players[i]);

        mockGameData.SetupGet(g => g.AllPlayers).Returns(mockList.Object);

        var manager = new ExtremeGameResultManager();

        manager.CreateTaskInfo();

        var taskInfoField = typeof(ExtremeGameResultManager).GetField("playerTaskInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        var playerTaskInfo = (Dictionary<byte, ExtremeGameResultManager.TaskInfo>)taskInfoField!.GetValue(manager)!;

        Assert.Equal(3, playerTaskInfo.Count);
        Assert.True(playerTaskInfo.ContainsKey(1));
        Assert.True(playerTaskInfo.ContainsKey(2));
        Assert.True(playerTaskInfo.ContainsKey(3));

        var winnerField = typeof(ExtremeGameResultManager).GetField("winner", BindingFlags.NonPublic | BindingFlags.Instance);
        var winner = (WinnerContainer)winnerField!.GetValue(manager)!;
        var poolField = typeof(WinnerContainer).GetField("allWinnerPool", BindingFlags.NonPublic | BindingFlags.Instance);
        var pool = (Dictionary<byte, CachedPlayerData>)poolField!.GetValue(winner)!;

        Assert.Equal(3, pool.Count);
        Assert.True(pool.ContainsKey(1));
        Assert.True(pool.ContainsKey(2));
        Assert.True(pool.ContainsKey(3));
    }

    [Fact]
    public void CreateEndGameManagerResult_MultiplePlayers_BuildsExpectedPlayerSummariesWithDetailedValues()
    {
        var mockGameData = MockSetupHelper.SetupGameDataMock();

        var player1 = CreateMockPlayerInfo(1, "Alice");
        var player2 = CreateMockPlayerInfo(2, "Bob");
        var player3 = CreateMockPlayerInfo(3, "Charlie");

        var mockList = new Mock<Il2CppPlayerList>(IntPtr.Zero);
        var players = new[] { player1, player2, player3 };

        mockList.SetupGet(l => l.Count).Returns(players.Length);
        mockList.Setup(l => l[It.IsAny<int>()]).Returns((int i) => players[i]);

        mockGameData.SetupGet(g => g.AllPlayers).Returns(mockList.Object);

        var role1 = new DummySingleRole(new RoleCore(ExtremeRoleId.Sheriff, ExtremeRoleType.Crewmate, Color.white));
        var role2 = new DummySingleRole(new RoleCore(ExtremeRoleId.Bait, ExtremeRoleType.Crewmate, Color.yellow));
        var role3 = new DummySingleRole(new RoleCore(ExtremeRoleId.Inspector, ExtremeRoleType.Crewmate, Color.blue));

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.GameRole[1] = role1;
        ExtremeRoleManager.GameRole[2] = role2;
        ExtremeRoleManager.GameRole[3] = role3;

        var cachedWinner1 = CreateMockCachedPlayerData("Alice");
        var cachedWinner2 = CreateMockCachedPlayerData("Bob");
        SetupMockCachedWinners(new[] { cachedWinner1, cachedWinner2 });

        var manager = new ExtremeGameResultManager();
        manager.CreateTaskInfo();

        manager.CreateEndGameManagerResult();

        Assert.NotNull(manager.PlayerSummaries);
        Assert.Equal(3, manager.PlayerSummaries.Count);

        var summary1 = manager.PlayerSummaries.First(s => s.PlayerId == 1);
        Assert.Equal("Alice", summary1.PlayerName);
        Assert.Equal(role1, summary1.Role);
        Assert.Equal(0, summary1.CompletedTask);
        Assert.Equal(0, summary1.TotalTask);

        var summary2 = manager.PlayerSummaries.First(s => s.PlayerId == 2);
        Assert.Equal("Bob", summary2.PlayerName);
        Assert.Equal(role2, summary2.Role);
        Assert.Equal(0, summary2.CompletedTask);
        Assert.Equal(0, summary2.TotalTask);

        var summary3 = manager.PlayerSummaries.First(s => s.PlayerId == 3);
        Assert.Equal("Charlie", summary3.PlayerName);
        Assert.Equal(role3, summary3.Role);
        Assert.Equal(0, summary3.CompletedTask);
        Assert.Equal(0, summary3.TotalTask);
    }
}
