using System;
using System.Collections.Generic;
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

    private static NetworkedPlayerInfo CreateMockPlayerInfo(byte playerId, string name = "Player")
    {
        var mockPlayer = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
        mockPlayer.SetupGet(p => p.PlayerName).Returns($"{name}{playerId}");
        return mockPlayer.Object;
    }

    private static CachedPlayerData CreateMockCachedPlayerData(string playerName)
    {
        var mock = new Mock<CachedPlayerData>(IntPtr.Zero);
        mock.SetupGet(c => c.PlayerName).Returns(playerName);
        return mock.Object;
    }

    [Fact]
    public void CreateTaskInfo_PopulatesPlayerTaskInfoAndWinnerPool()
    {
        var mockGameData = MockSetupHelper.SetupGameDataMock();

        var player1 = CreateMockPlayerInfo(1, "Player");
        var player2 = CreateMockPlayerInfo(2, "Player");

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

    [Fact]
    public void CreateEndGameManagerResult_SetsWinnerAndBuildsPlayerSummaries()
    {
        var mockGameData = MockSetupHelper.SetupGameDataMock();

        var player1 = CreateMockPlayerInfo(1, "Player");
        var mockList = new Mock<Il2CppPlayerList>(IntPtr.Zero);
        var players = new[] { player1 };

        mockList.SetupGet(l => l.Count).Returns(players.Length);
        mockList.Setup(l => l[It.IsAny<int>()]).Returns((int i) => players[i]);

        mockGameData.SetupGet(g => g.AllPlayers).Returns(mockList.Object);

        var core = new RoleCore(ExtremeRoleId.Sheriff, ExtremeRoleType.Crewmate, Color.white);
        var role = new DummySingleRole(core);
        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.GameRole[1] = role;

        var mockWinners = new Mock<Il2CppSystem.Collections.Generic.List<CachedPlayerData>>(IntPtr.Zero);
        var cachedWinnersList = new[] { CreateMockCachedPlayerData("Player1") };
        mockWinners.SetupGet(w => w.Count).Returns(cachedWinnersList.Length);
        mockWinners.Setup(w => w[It.IsAny<int>()]).Returns((int i) => cachedWinnersList[i]);
        mockWinners.Setup(w => w.ToArray()).Returns(new Il2CppReferenceArray<CachedPlayerData>(cachedWinnersList));

        var mockEnum = new Mock<Il2CppSystem.Collections.Generic.List<CachedPlayerData>.Enumerator>(IntPtr.Zero);
        mockEnum.Setup(e => e.MoveNext()).Returns(false);
        mockWinners.Setup(w => w.GetEnumerator()).Returns(mockEnum.Object);

        var mockWinnersHelper = new Mock<MockEndGameResultget_CachedWinnersHelper>();
        mockWinnersHelper.Setup(h => h.Invoke()).Returns(mockWinners.Object);
        MockEndGameResultget_CachedWinnersHelper.Instance = mockWinnersHelper.Object;

        var manager = new ExtremeGameResultManager();
        manager.CreateTaskInfo();

        manager.CreateEndGameManagerResult();

        Assert.NotNull(manager.PlayerSummaries);
        Assert.NotEmpty(manager.PlayerSummaries);
    }
}
