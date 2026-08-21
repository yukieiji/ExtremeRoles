using System;
using AmongUs.GameOptions;
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
    public void TryGetPlayerControl_WithMatchingPlayerInCache_ReturnsTrueAndSetsResult()
    {
        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)3);

        PlayerCache.AllPlayerControl.Add(mockPlayer.Object);

        bool success = Player.TryGetPlayerControl(3, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal((byte)3, result.PlayerId);
    }

    [Fact]
    public void TryGetPlayerControl_WhenPlayerNotFound_ReturnsFalseAndSetsResultNull()
    {
        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)3);

        PlayerCache.AllPlayerControl.Add(mockPlayer.Object);

        bool success = Player.TryGetPlayerControl(99, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryGetPlayerInfo_WhenGameDataUninitialized_ReturnsFalseAndSetsNull()
    {
        bool success = Player.TryGetPlayerInfo(1, out var player);

        Assert.False(success);
        Assert.Null(player);
    }

    [Fact]
    public void TryGetPlayerRoom_WhenPlayerIsNull_ReturnsFalseAndSetsRoomIdNull()
    {
        bool success = Player.TryGetPlayerRoom(null!, out var roomId);

        Assert.False(success);
        Assert.Null(roomId);
    }

    [Fact]
    public void TryGetPlayerColiderRoom_WhenColliderIsNull_ReturnsFalseAndSetsRoomIdNull()
    {
        bool success = Player.TryGetPlayerColiderRoom(null!, out var roomId);

        Assert.False(success);
        Assert.Null(roomId);
    }

    [Fact]
    public void TryGetPlayerColiderRoom_WhenShipStatusUninitialized_ReturnsFalseAndSetsRoomIdNull()
    {
        var mockCollider = new Mock<Collider2D>();

        bool success = Player.TryGetPlayerColiderRoom(mockCollider.Object, out var roomId);

        Assert.False(success);
        Assert.Null(roomId);
    }

    [Fact]
    public void IsValidPlayer_WhenTargetPlayerIsNull_ReturnsFalse()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        var mockRole = new Mock<SingleRoleBase>();

        bool isValid = Player.IsValidPlayer(mockRole.Object, mockSourcePlayer.Object, null!);

        Assert.False(isValid);
    }

    [Fact]
    public void IsValidPlayer_WhenSourcePlayerIsNull_ReturnsFalse()
    {
        var mockTargetPlayer = new Mock<NetworkedPlayerInfo>();
        var mockRole = new Mock<SingleRoleBase>();

        bool isValid = Player.IsValidPlayer(mockRole.Object, null!, mockTargetPlayer.Object);

        Assert.False(isValid);
    }

    [Fact]
    public void IsValidPlayer_WhenRoleIsNull_ReturnsFalse()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        var mockTargetPlayer = new Mock<NetworkedPlayerInfo>();

        bool isValid = Player.IsValidPlayer(null!, mockSourcePlayer.Object, mockTargetPlayer.Object);

        Assert.False(isValid);
    }

    [Fact]
    public void IsValidPlayer_WhenTargetPlayerIsSameAsSourcePlayer_ReturnsFalse()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        mockSourcePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

        var mockTargetPlayer = new Mock<NetworkedPlayerInfo>();
        mockTargetPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

        var mockRole = new Mock<SingleRoleBase>();

        bool isValid = Player.IsValidPlayer(mockRole.Object, mockSourcePlayer.Object, mockTargetPlayer.Object);

        Assert.False(isValid);
    }

    [Fact]
    public void GetAllPlayerInRange_WhenSourcePlayerIsNull_ReturnsEmptyList()
    {
        var mockRole = new Mock<SingleRoleBase>();

        var result = Player.GetAllPlayerInRange(null!, mockRole.Object, 5.0f);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllPlayerInRange_WhenRoleIsNull_ReturnsEmptyList()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();

        var result = Player.GetAllPlayerInRange(mockSourcePlayer.Object, null!, 5.0f);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllPlayerInRange_WhenShipStatusUninitialized_ReturnsEmptyList()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        var mockRole = new Mock<SingleRoleBase>();

        var result = Player.GetAllPlayerInRange(mockSourcePlayer.Object, mockRole.Object, 5.0f);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetClosestPlayerInRange_WhenShipStatusUninitialized_ReturnsNull()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        var mockRole = new Mock<SingleRoleBase>();

        var result = Player.GetClosestPlayerInRange(mockSourcePlayer.Object, mockRole.Object, 5.0f);

        Assert.Null(result);
    }

    [Fact]
    public void TryGetClosestPlayerInRange_WhenShipStatusUninitialized_ReturnsFalseAndSetsNull()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        var mockRole = new Mock<SingleRoleBase>();

        bool success = Player.TryGetClosestPlayerInRange(mockSourcePlayer.Object, mockRole.Object, 5.0f, out var targetPlayer);

        Assert.False(success);
        Assert.Null(targetPlayer);
    }

    [Fact]
    public void IsPlayerInRangeAndDrawOutLine_WhenShipStatusUninitialized_ReturnsFalse()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        var mockTargetPlayer = new Mock<PlayerControl>();
        var mockRole = new Mock<SingleRoleBase>();

        bool inRange = Player.IsPlayerInRangeAndDrawOutLine(mockSourcePlayer.Object, mockTargetPlayer.Object, mockRole.Object, 5.0f);

        Assert.False(inRange);
    }

    [Fact]
    public void TryGetTaskType_WhenPlayerIsNull_ReturnsFalseAndSetsTaskNull()
    {
        bool success = Player.TryGetTaskType(null!, TaskTypes.SubmitScan, out var task);

        Assert.False(success);
        Assert.Null(task);
    }

    [Fact]
    public void GetPlayerTaskGage_WhenPlayerControlIsNull_ReturnsZero()
    {
        float gage = Player.GetPlayerTaskGage((PlayerControl)null!);

        Assert.Equal(0f, gage);
    }

    [Fact]
    public void GetPlayerTaskGage_WhenPlayerDataIsNull_ReturnsZero()
    {
        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.Data).Returns((NetworkedPlayerInfo)null!);

        float gage = Player.GetPlayerTaskGage(mockPlayer.Object);

        Assert.Equal(0f, gage);
    }

    [Fact]
    public void GetPlayerTaskGage_WhenNetworkedPlayerInfoIsNull_ReturnsZero()
    {
        float gage = Player.GetPlayerTaskGage((NetworkedPlayerInfo)null!);

        Assert.Equal(0f, gage);
    }

    [Fact]
    public void GetPlayerTaskGage_WhenPlayerTasksIsNull_ReturnsZero()
    {
        var mockPlayerInfo = new Mock<NetworkedPlayerInfo>();
        mockPlayerInfo.SetupGet(p => p.Tasks).Returns((Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>)null!);

        float gage = Player.GetPlayerTaskGage(mockPlayerInfo.Object);

        Assert.Equal(0f, gage);
    }

    [Fact]
    public void GetDeadBodyInfo_WhenLocalPlayerUninitialized_ReturnsNull()
    {
        var result = Player.GetDeadBodyInfo(5.0f);

        Assert.Null(result);
    }
}
