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
    public void TryGetPlayerControl_InMockEnv_ThrowsNotImplementedExceptionDueToStrippedOperator()
    {
        Assert.Throws<NotImplementedException>(() => Player.TryGetPlayerControl(99, out _));
    }

    [Fact]
    public void TryGetPlayerInfo_InMockEnv_ThrowsNotImplementedExceptionDueToStrippedGameData()
    {
        Assert.Throws<NotImplementedException>(() => Player.TryGetPlayerInfo(1, out _));
    }

    [Fact]
    public void TryGetPlayerRoom_InMockEnv_ThrowsNotImplementedExceptionDueToStrippedUnityOperator()
    {
        Assert.Throws<NotImplementedException>(() => Player.TryGetPlayerRoom(null!, out _));
    }

    [Fact]
    public void TryGetPlayerColiderRoom_InMockEnv_ThrowsNotImplementedExceptionDueToStrippedUnityOperator()
    {
        Assert.Throws<NotImplementedException>(() => Player.TryGetPlayerColiderRoom(null!, out _));
    }

    [Fact]
    public void IsValidPlayer_InMockEnv_ThrowsNotImplementedExceptionDueToStrippedUnityOperator()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();

        Assert.Throws<NotImplementedException>(() => Player.IsValidPlayer(null!, mockSourcePlayer.Object, null!));
    }

    [Fact]
    public void IsValidPlayer_WhenTargetPlayerIsProvided_InMockEnv_ThrowsNotImplementedExceptionDueToStrippedUnityOperator()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        var mockTargetInfo = new Mock<NetworkedPlayerInfo>();

        Assert.Throws<NotImplementedException>(() => Player.IsValidPlayer(null!, mockSourcePlayer.Object, mockTargetInfo.Object));
    }

    [Fact]
    public void GetAllPlayerInRange_InMockEnv_ThrowsNotImplementedExceptionDueToStrippedShipStatus()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();

        Assert.Throws<NotImplementedException>(() => Player.GetAllPlayerInRange(mockSourcePlayer.Object, null!, 5.0f));
    }

    [Fact]
    public void IsPlayerInRangeAndDrawOutLine_InMockEnv_ThrowsNotImplementedExceptionDueToStrippedShipStatus()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        var mockTargetPlayer = new Mock<PlayerControl>();

        Assert.Throws<NotImplementedException>(() => Player.IsPlayerInRangeAndDrawOutLine(mockSourcePlayer.Object, mockTargetPlayer.Object, null!, 5.0f));
    }

    [Fact]
    public void GetClosestPlayerInRange_InMockEnv_ThrowsNotImplementedExceptionDueToStrippedShipStatus()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();

        Assert.Throws<NotImplementedException>(() => Player.GetClosestPlayerInRange(mockSourcePlayer.Object, null!, 5.0f));
    }

    [Fact]
    public void TryGetClosestPlayerInRange_InMockEnv_ThrowsNotImplementedExceptionDueToStrippedShipStatus()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();

        Assert.Throws<NotImplementedException>(() => Player.TryGetClosestPlayerInRange(mockSourcePlayer.Object, null!, 5.0f, out _));
    }
}
