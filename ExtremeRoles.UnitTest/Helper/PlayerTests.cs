using System;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API;
using Moq;
using UnityEngine;
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
    public void GetPlayerControlById_WhenMultiplePlayersInCache_ShouldReturnCorrectPlayer()
    {
        var mockPlayer1 = new Mock<PlayerControl>();
        mockPlayer1.SetupGet(p => p.PlayerId).Returns((byte)1);

        var mockPlayer2 = new Mock<PlayerControl>();
        mockPlayer2.SetupGet(p => p.PlayerId).Returns((byte)2);

        PlayerCache.AllPlayerControl.Add(mockPlayer1.Object);
        PlayerCache.AllPlayerControl.Add(mockPlayer2.Object);

        var result = Player.GetPlayerControlById(2);

        Assert.NotNull(result);
        Assert.Equal((byte)2, result.PlayerId);
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
    public void TryGetPlayerControl_InMockEnv_ThrowsNullReferenceExceptionDueToStrippedOperator()
    {
        Assert.Throws<NullReferenceException>(() => Player.TryGetPlayerControl(99, out _));
    }

    [Fact]
    public void TryGetPlayerInfo_InMockEnv_ThrowsNullReferenceExceptionDueToStrippedGameData()
    {
        Assert.Throws<NullReferenceException>(() => Player.TryGetPlayerInfo(1, out _));
    }

    [Fact]
    public void TryGetPlayerRoom_InMockEnv_ThrowsNullReferenceExceptionDueToStrippedUnityOperator()
    {
        Assert.Throws<NullReferenceException>(() => Player.TryGetPlayerRoom(null!, out _));
    }

    [Fact]
    public void TryGetPlayerColiderRoom_InMockEnv_ThrowsNullReferenceExceptionDueToStrippedUnityOperator()
    {
        Assert.Throws<NullReferenceException>(() => Player.TryGetPlayerColiderRoom(null!, out _));
    }

    [Fact]
    public void IsValidPlayer_InMockEnv_ThrowsNullReferenceExceptionDueToStrippedUnityOperator()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        var mockRole = new Mock<SingleRoleBase>();

        Assert.Throws<NullReferenceException>(() => Player.IsValidPlayer(mockRole.Object, mockSourcePlayer.Object, null!));
    }

    [Fact]
    public void GetAllPlayerInRange_InMockEnv_ThrowsNullReferenceExceptionDueToStrippedShipStatus()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        var mockRole = new Mock<SingleRoleBase>();

        Assert.Throws<NullReferenceException>(() => Player.GetAllPlayerInRange(mockSourcePlayer.Object, mockRole.Object, 5.0f));
    }
}
