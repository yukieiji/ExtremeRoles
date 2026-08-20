using System;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Helper;

public class PlayerTests : IDisposable
{
    public PlayerTests()
    {
        PlayerCache.AllPlayerControl.Clear();
    }

    public void Dispose()
    {
        PlayerCache.AllPlayerControl.Clear();
    }

    [Fact]
    public void GetPlayerControlById_WithMatchingPlayerInCache_ShouldReturnPlayer()
    {
        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)5);

        PlayerCache.AllPlayerControl.Add(mockPlayer.Object);

        var result = Player.GetPlayerControlById(5);

        Assert.NotNull(result);
        Assert.Equal((byte)5, result.PlayerId);
    }

    [Fact]
    public void GetPlayerControlById_WhenPlayerNotFound_ShouldReturnNull()
    {
        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)5);

        PlayerCache.AllPlayerControl.Add(mockPlayer.Object);

        var result = Player.GetPlayerControlById(99);

        Assert.Null(result);
    }

    [Fact]
    public void CreatePlayerIcon_InMockEnv_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(() => Player.CreatePlayerIcon());
    }

    [Fact]
    public void TryGetPlayerInfo_InMockEnv_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(() => Player.TryGetPlayerInfo(1, out _));
    }
}
