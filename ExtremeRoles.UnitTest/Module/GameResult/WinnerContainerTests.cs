using System;
using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameResult;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameResult;


[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class WinnerContainerTests
{
    public WinnerContainerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupLogger("WinnerContainerTests");
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

    private static CachedPlayerData CreateMockCachedPlayerData(string playerName)
    {
        var mock = new Mock<CachedPlayerData>(IntPtr.Zero);
        mock.SetupGet(c => c.PlayerName).Returns(playerName);
        return mock.Object;
    }

    private static void AddMockPlayerToPool(WinnerContainer container, NetworkedPlayerInfo player)
    {
        var poolField = typeof(WinnerContainer).GetField("allWinnerPool", BindingFlags.NonPublic | BindingFlags.Instance);
        var pool = (Dictionary<byte, CachedPlayerData>)poolField!.GetValue(container)!;
        pool[player.PlayerId] = CreateMockCachedPlayerData(player.PlayerName);
    }

    [Fact]
    public void AddPlusWinner_And_Convert_ReturnsExpectedResult()
    {
        var container = new WinnerContainer();
        var player = CreateMockPlayerInfo(1, "Test");

        container.AddPlusWinner(player);

        var result = container.Convert();
        Assert.Contains(player, result.PlusedWinner);
    }

    [Fact]
    public void Add_And_Remove_And_Clear_WorksCorrectly()
    {
        var container = new WinnerContainer();
        var player = CreateMockPlayerInfo(1, "Test");
        AddMockPlayerToPool(container, player);

        container.Add(player);
        var finalField = typeof(WinnerContainer).GetField("finalWinPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
        var final = (List<CachedPlayerData>)finalField!.GetValue(container)!;
        Assert.NotEmpty(final);

        container.Remove(player);
        Assert.Empty(final);

        container.Add(player);
        container.Clear();
        Assert.Empty(final);
    }

    [Fact]
    public void AllClear_ClearsBothFinalAndPlusWinners()
    {
        var container = new WinnerContainer();
        var player = CreateMockPlayerInfo(1, "Test");
        AddMockPlayerToPool(container, player);

        container.Add(player);
        container.AddPlusWinner(player);

        container.AllClear();

        var result = container.Convert();
        Assert.Empty(result.Winner);
        Assert.Empty(result.PlusedWinner);
    }

    [Fact]
    public void RemoveAll_RemovesFromBothPlusAndFinalWinners()
    {
        var container = new WinnerContainer();
        var player = CreateMockPlayerInfo(1, "Test");
        AddMockPlayerToPool(container, player);

        container.Add(player);
        container.AddPlusWinner(player);

        container.RemoveAll(player);

        Assert.DoesNotContain(player, container.PlusedWinner);
    }
}
