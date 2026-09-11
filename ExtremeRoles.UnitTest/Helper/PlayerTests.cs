using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ExtremeRoles.Compat;
using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Ability;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Helper;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class PlayerTests : IDisposable
{
    private static readonly IGameOptions globalGameOptions;
    private static readonly GameOptionsManager globalGameOptionsManager;
    private static readonly GameData globalGameData;
    private static readonly ShipStatus globalShipStatus;

    private static readonly Mock<MockGameDataget_InstanceHelper> globalGameDataHelper = new();
    private static readonly Mock<MockShipStatusget_InstanceHelper> globalShipHelper = new();
    private static readonly Mock<MockGameOptionsManagerget_InstanceHelper> globalOptionsHelper = new();
    private static readonly Mock<MockRoleBehaviourGetTempPlayerListHelper> globalGetTempHelper = new();

    static PlayerTests()
    {
        var mockOptions = new Mock<IGameOptions>(IntPtr.Zero);
        mockOptions.Setup(o => o.GetFloat(FloatOptionNames.KillCooldown)).Returns(25.0f);
        globalGameOptions = mockOptions.Object;

        var mockManager = new Mock<GameOptionsManager>(IntPtr.Zero);
        mockManager.SetupGet(m => m.CurrentGameOptions).Returns(globalGameOptions);
        globalGameOptionsManager = mockManager.Object;

        globalGameData = new Mock<GameData>().Object;
        globalShipStatus = new Mock<ShipStatus>().Object;

        MockSetupHelper.SetupUnityCommonMocks();
    }

    public PlayerTests()
    {
        ResetAllState();
    }

    public void Dispose()
    {
        ResetAllState();
    }

    private void ResetAllState()
    {
        PlayerCache.RemovePlayerControl(_ => true);
        ExtremeRoleManager.GameRole.Clear();

        ExtremeRoles.GameMode.ExtremeGameModeManager.Create(AmongUs.GameOptions.GameModes.Normal);

        globalGameDataHelper.Setup(h => h.Invoke()).Returns(globalGameData);
        MockGameDataget_InstanceHelper.Instance = globalGameDataHelper.Object;

        globalShipHelper.Setup(h => h.Invoke()).Returns(globalShipStatus);
        MockShipStatusget_InstanceHelper.Instance = globalShipHelper.Object;

        globalOptionsHelper.Setup(h => h.Invoke()).Returns(globalGameOptionsManager);
        MockGameOptionsManagerget_InstanceHelper.Instance = globalOptionsHelper.Object;

        var emptyList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerControl>>(IntPtr.Zero);
        emptyList.SetupGet(l => l.Count).Returns(0);
        globalGetTempHelper.Setup(h => h.Invoke()).Returns(emptyList.Object);
        MockRoleBehaviourGetTempPlayerListHelper.Instance = globalGetTempHelper.Object;
    }


    [Fact]
    public void GetPlayerControlById_WithMatchingPlayerInCache_ShouldReturnPlayer()
    {
        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)5);

        PlayerCache.AddPlayerControl(mockPlayer.Object);

        var result = Player.GetPlayerControlById(5);

        Assert.NotNull(result);
        Assert.Equal((byte)5, result.PlayerId);
    }

    [Fact]
    public void GetPlayerControlById_WhenPlayerNotFound_ShouldReturnNull()
    {
        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)5);

        PlayerCache.AddPlayerControl(mockPlayer.Object);

        var result = Player.GetPlayerControlById(99);

        Assert.Null(result);
    }

    [Fact]
    public void TryGetPlayerControl_WhenPlayerExists_ReturnsTrueAndPlayer()
    {
        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)5);
        PlayerCache.AddPlayerControl(mockPlayer.Object);

        bool result = Player.TryGetPlayerControl(5, out var foundPlayer);

        Assert.True(result);
        Assert.NotNull(foundPlayer);
        Assert.Equal((byte)5, foundPlayer.PlayerId);
    }

    [Fact]
    public void TryGetPlayerControl_WhenPlayerNotFound_ReturnsFalseAndNull()
    {
        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)5);
        PlayerCache.AddPlayerControl(mockPlayer.Object);

        bool result = Player.TryGetPlayerControl(99, out var foundPlayer);

        Assert.False(result);
        Assert.Null(foundPlayer);
    }

    [Fact]
    public void TryGetPlayerInfo_WhenGameDataNull_ReturnsFalseAndNull()
    {
        var mockHelper = new Mock<MockGameDataget_InstanceHelper>();
        mockHelper.Setup(h => h.Invoke()).Returns((GameData)null!);
        MockGameDataget_InstanceHelper.Instance = mockHelper.Object;

        bool result = Player.TryGetPlayerInfo(1, out var info);

        Assert.False(result);
        Assert.Null(info);
    }

    [Fact]
    public void TryGetPlayerInfo_WhenPlayerExists_ReturnsTrueAndPlayerInfo()
    {
        var mockGameData = new Mock<GameData>();
        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockGameData.Setup(g => g.GetPlayerById(1)).Returns(mockInfo.Object);

        var mockHelper = new Mock<MockGameDataget_InstanceHelper>();
        mockHelper.Setup(h => h.Invoke()).Returns(mockGameData.Object);
        MockGameDataget_InstanceHelper.Instance = mockHelper.Object;

        bool result = Player.TryGetPlayerInfo(1, out var info);

        Assert.True(result);
        Assert.NotNull(info);
        Assert.Same(mockInfo.Object, info);
    }

    [Fact]
    public void TryGetPlayerRoom_WhenPlayerNull_ReturnsFalseAndNull()
    {
        bool result = Player.TryGetPlayerRoom(null!, out var roomId);

        Assert.False(result);
        Assert.Null(roomId);
    }

    [Fact]
    public void TryGetPlayerColiderRoom_WhenColliderNull_ReturnsFalseAndNull()
    {
        bool result = Player.TryGetPlayerColiderRoom(null!, out var roomId);

        Assert.False(result);
        Assert.Null(roomId);
    }

    [Fact]
    public void GetClosestPlayerInKillRange_WhenNoPlayersInRange_ReturnsNull()
    {
        var emptyList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerControl>>(IntPtr.Zero);
        emptyList.SetupGet(l => l.Count).Returns(0);

        var mockRole = new Mock<RoleBehaviour>();
        mockRole.Setup(r => r.GetPlayersInAbilityRangeSorted(It.IsAny<Il2CppSystem.Collections.Generic.List<PlayerControl>>()))
            .Returns(emptyList.Object);

        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockInfo.SetupGet(i => i.Role).Returns(mockRole.Object);

        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.Data).Returns(mockInfo.Object);

        var target = Player.GetClosestPlayerInKillRange(mockPlayer.Object);

        Assert.Null(target);
    }

    [Fact]
    public void GetClosestPlayerInKillRange_WhenPlayerInRange_ReturnsClosestPlayer()
    {
        var mockTargetPlayer = new Mock<PlayerControl>();

        var mockList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerControl>>(IntPtr.Zero);
        mockList.SetupGet(l => l.Count).Returns(1);
        mockList.SetupGet(l => l[0]).Returns(mockTargetPlayer.Object);

        var mockRole = new Mock<RoleBehaviour>();
        mockRole.Setup(r => r.GetPlayersInAbilityRangeSorted(It.IsAny<Il2CppSystem.Collections.Generic.List<PlayerControl>>()))
            .Returns(mockList.Object);

        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockInfo.SetupGet(i => i.Role).Returns(mockRole.Object);

        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.Data).Returns(mockInfo.Object);

        var target = Player.GetClosestPlayerInKillRange(mockPlayer.Object);

        Assert.NotNull(target);
        Assert.Same(mockTargetPlayer.Object, target);
    }

    [Fact]
    public void GetClosestPlayerInKillRange_Parameterless_WhenPlayerInRange_ReturnsClosestPlayer()
    {
        var mockTargetPlayer = new Mock<PlayerControl>();

        var mockList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerControl>>(IntPtr.Zero);
        mockList.SetupGet(l => l.Count).Returns(1);
        mockList.SetupGet(l => l[0]).Returns(mockTargetPlayer.Object);

        var mockRole = new Mock<RoleBehaviour>();
        mockRole.Setup(r => r.GetPlayersInAbilityRangeSorted(It.IsAny<Il2CppSystem.Collections.Generic.List<PlayerControl>>()))
            .Returns(mockList.Object);

        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockInfo.SetupGet(i => i.Role).Returns(mockRole.Object);

        var mockLocalPlayer = new Mock<PlayerControl>();
        mockLocalPlayer.SetupGet(p => p.Data).Returns(mockInfo.Object);

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(h => h.Invoke()).Returns(mockLocalPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var target = Player.GetClosestPlayerInKillRange();

        Assert.NotNull(target);
        Assert.Same(mockTargetPlayer.Object, target);
    }

    [Fact]
    public void IsValidPlayer_WhenTargetPlayerNull_ReturnsFalse()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

        bool result = Player.IsValidPlayer(sourceRole, mockSourcePlayer.Object, null!);

        Assert.False(result);
    }

    [Fact]
    public void IsValidPlayer_WhenTargetIsSelf_ReturnsFalse()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        mockSourcePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

        var mockTargetInfo = new Mock<NetworkedPlayerInfo>();
        mockTargetInfo.SetupGet(t => t.PlayerId).Returns((byte)1);
        mockTargetInfo.SetupGet(t => t.IsDead).Returns(false);
        mockTargetInfo.SetupGet(t => t.Disconnected).Returns(false);

        var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

        bool result = Player.IsValidPlayer(sourceRole, mockSourcePlayer.Object, mockTargetInfo.Object);

        Assert.False(result);
    }

    [Fact]
    public void IsValidPlayer_WhenTargetIsDead_ReturnsFalse()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        mockSourcePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

        var mockTargetInfo = new Mock<NetworkedPlayerInfo>();
        mockTargetInfo.SetupGet(t => t.PlayerId).Returns((byte)2);
        mockTargetInfo.SetupGet(t => t.IsDead).Returns(true);
        mockTargetInfo.SetupGet(t => t.Disconnected).Returns(false);

        var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

        bool result = Player.IsValidPlayer(sourceRole, mockSourcePlayer.Object, mockTargetInfo.Object);

        Assert.False(result);
    }

    [Fact]
    public void IsValidPlayer_WhenTargetInVent_ReturnsFalse()
    {
        byte sourceId = 1;
        byte targetId = 2;

        var mockSourcePlayer = new Mock<PlayerControl>();
        mockSourcePlayer.SetupGet(p => p.PlayerId).Returns(sourceId);

        var mockTargetControl = new Mock<PlayerControl>();
        mockTargetControl.SetupGet(p => p.inVent).Returns(true);

        var mockTargetInfo = new Mock<NetworkedPlayerInfo>();
        mockTargetInfo.SetupGet(t => t.PlayerId).Returns(targetId);
        mockTargetInfo.SetupGet(t => t.IsDead).Returns(false);
        mockTargetInfo.SetupGet(t => t.Disconnected).Returns(false);
        mockTargetInfo.SetupGet(t => t.Object).Returns(mockTargetControl.Object);

        var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

        bool result = Player.IsValidPlayer(sourceRole, mockSourcePlayer.Object, mockTargetInfo.Object);

        Assert.False(result);
    }

    [Fact]
    public void IsValidPlayer_WhenTargetInMovingPlat_ReturnsFalse()
    {
        byte sourceId = 1;
        byte targetId = 2;

        var mockSourcePlayer = new Mock<PlayerControl>();
        mockSourcePlayer.SetupGet(p => p.PlayerId).Returns(sourceId);

        var mockTargetControl = new Mock<PlayerControl>();
        mockTargetControl.SetupGet(p => p.inVent).Returns(false);
        mockTargetControl.SetupGet(p => p.inMovingPlat).Returns(true);

        var mockTargetInfo = new Mock<NetworkedPlayerInfo>();
        mockTargetInfo.SetupGet(t => t.PlayerId).Returns(targetId);
        mockTargetInfo.SetupGet(t => t.IsDead).Returns(false);
        mockTargetInfo.SetupGet(t => t.Disconnected).Returns(false);
        mockTargetInfo.SetupGet(t => t.Object).Returns(mockTargetControl.Object);

        var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

        bool result = Player.IsValidPlayer(sourceRole, mockSourcePlayer.Object, mockTargetInfo.Object);

        Assert.False(result);
    }

    [Fact]
    public void IsValidPlayer_WhenTargetOnLadder_ReturnsFalse()
    {
        byte sourceId = 1;
        byte targetId = 2;

        var mockSourcePlayer = new Mock<PlayerControl>();
        mockSourcePlayer.SetupGet(p => p.PlayerId).Returns(sourceId);

        var mockTargetControl = new Mock<PlayerControl>();
        mockTargetControl.SetupGet(p => p.inVent).Returns(false);
        mockTargetControl.SetupGet(p => p.inMovingPlat).Returns(false);
        mockTargetControl.SetupGet(p => p.onLadder).Returns(true);

        var mockTargetInfo = new Mock<NetworkedPlayerInfo>();
        mockTargetInfo.SetupGet(t => t.PlayerId).Returns(targetId);
        mockTargetInfo.SetupGet(t => t.IsDead).Returns(false);
        mockTargetInfo.SetupGet(t => t.Disconnected).Returns(false);
        mockTargetInfo.SetupGet(t => t.Object).Returns(mockTargetControl.Object);

        var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

        bool result = Player.IsValidPlayer(sourceRole, mockSourcePlayer.Object, mockTargetInfo.Object);

        Assert.False(result);
    }

    [Fact]
    public void IsValidPlayer_WhenTargetHasSameTeam_ReturnsFalse()
    {
        byte sourceId = 1;
        byte targetId = 2;

        var mockSourcePlayer = new Mock<PlayerControl>();
        mockSourcePlayer.SetupGet(p => p.PlayerId).Returns(sourceId);

        var mockTargetControl = new Mock<PlayerControl>();
        mockTargetControl.SetupGet(p => p.inVent).Returns(false);
        mockTargetControl.SetupGet(p => p.inMovingPlat).Returns(false);
        mockTargetControl.SetupGet(p => p.onLadder).Returns(false);

        var mockTargetInfo = new Mock<NetworkedPlayerInfo>();
        mockTargetInfo.SetupGet(t => t.PlayerId).Returns(targetId);
        mockTargetInfo.SetupGet(t => t.IsDead).Returns(false);
        mockTargetInfo.SetupGet(t => t.Disconnected).Returns(false);
        mockTargetInfo.SetupGet(t => t.Object).Returns(mockTargetControl.Object);

        var targetRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();
        var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

        ExtremeRoleManager.GameRole[targetId] = targetRole;

        bool result = Player.IsValidPlayer(sourceRole, mockSourcePlayer.Object, mockTargetInfo.Object);

        Assert.False(result);
    }

    [Fact]
    public void IsValidPlayer_WhenTargetIsInvincibleToSource_ReturnsFalse()
    {
        byte sourceId = 1;
        byte targetId = 2;

        var mockSourcePlayer = new Mock<PlayerControl>();
        mockSourcePlayer.SetupGet(p => p.PlayerId).Returns(sourceId);

        var mockTargetControl = new Mock<PlayerControl>();
        mockTargetControl.SetupGet(p => p.inVent).Returns(false);
        mockTargetControl.SetupGet(p => p.inMovingPlat).Returns(false);
        mockTargetControl.SetupGet(p => p.onLadder).Returns(false);

        var mockTargetInfo = new Mock<NetworkedPlayerInfo>();
        mockTargetInfo.SetupGet(t => t.PlayerId).Returns(targetId);
        mockTargetInfo.SetupGet(t => t.IsDead).Returns(false);
        mockTargetInfo.SetupGet(t => t.Disconnected).Returns(false);
        mockTargetInfo.SetupGet(t => t.Object).Returns(mockTargetControl.Object);

        var mockInvincible = new Mock<IInvincible>();
        mockInvincible.Setup(i => i.IsValidAbilitySource(sourceId)).Returns(false);
        var mockAbility = mockInvincible.As<IAbility>();

        var targetRole = new ExtremeRoles.Roles.Solo.Neutral.Monika();
        typeof(SingleRoleBase).GetProperty(nameof(SingleRoleBase.AbilityClass))!.SetValue(targetRole, mockAbility.Object);

        var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

        ExtremeRoleManager.GameRole[targetId] = targetRole;

        bool result = Player.IsValidPlayer(sourceRole, mockSourcePlayer.Object, mockTargetInfo.Object);

        Assert.False(result);
    }

    [Fact]
    public void IsValidPlayer_WhenAllConditionsMet_ReturnsTrue()
    {
        byte sourceId = 1;
        byte targetId = 2;

        var mockSourcePlayer = new Mock<PlayerControl>();
        mockSourcePlayer.SetupGet(p => p.PlayerId).Returns(sourceId);

        var mockTargetControl = new Mock<PlayerControl>();
        mockTargetControl.SetupGet(p => p.inVent).Returns(false);
        mockTargetControl.SetupGet(p => p.inMovingPlat).Returns(false);
        mockTargetControl.SetupGet(p => p.onLadder).Returns(false);

        var mockTargetInfo = new Mock<NetworkedPlayerInfo>();
        mockTargetInfo.SetupGet(t => t.PlayerId).Returns(targetId);
        mockTargetInfo.SetupGet(t => t.IsDead).Returns(false);
        mockTargetInfo.SetupGet(t => t.Disconnected).Returns(false);
        mockTargetInfo.SetupGet(t => t.Object).Returns(mockTargetControl.Object);

        var targetRole = new ExtremeRoles.Roles.Solo.Neutral.TaskMaster();
        var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

		ExtremeRoleManager.GameRole[sourceId] = sourceRole;
		ExtremeRoleManager.GameRole[targetId] = targetRole;

        bool result = Player.IsValidPlayer(sourceRole, mockSourcePlayer.Object, mockTargetInfo.Object);

        Assert.True(result);
    }

	[Fact]
	public void IsValidPlayer_WhenAllConditionsMet_ReturnsFalse()
	{
		byte sourceId = 1;
		byte targetId = 2;

		var mockSourcePlayer = new Mock<PlayerControl>();
		mockSourcePlayer.SetupGet(p => p.PlayerId).Returns(sourceId);

		var mockTargetControl = new Mock<PlayerControl>();
		mockTargetControl.SetupGet(p => p.inVent).Returns(false);
		mockTargetControl.SetupGet(p => p.inMovingPlat).Returns(false);
		mockTargetControl.SetupGet(p => p.onLadder).Returns(false);

		var mockTargetInfo = new Mock<NetworkedPlayerInfo>();
		mockTargetInfo.SetupGet(t => t.PlayerId).Returns(targetId);
		mockTargetInfo.SetupGet(t => t.IsDead).Returns(false);
		mockTargetInfo.SetupGet(t => t.Disconnected).Returns(false);
		mockTargetInfo.SetupGet(t => t.Object).Returns(mockTargetControl.Object);

		var targetRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();
		var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

		targetRole.SetControlId(10);
		sourceRole.SetControlId(10);

		ExtremeRoleManager.GameRole[sourceId] = sourceRole;
		ExtremeRoleManager.GameRole[targetId] = targetRole;

		bool result = Player.IsValidPlayer(sourceRole, mockSourcePlayer.Object, mockTargetInfo.Object);

		Assert.False(result);
	}

	[Fact]
    public void GetAllPlayerInRange_WhenShipStatusNull_ReturnsEmptyList()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

        var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
        mockShipHelper.Setup(h => h.Invoke()).Returns((ShipStatus)null!);
        MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

        var result = Player.GetAllPlayerInRange(mockSourcePlayer.Object, sourceRole, 5.0f);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void TryGetClosestPlayerInRange_4Params_WhenNoPlayerInRange_ReturnsFalse()
    {
        var mockSourcePlayer = new Mock<PlayerControl>();
        var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

        var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
        mockShipHelper.Setup(h => h.Invoke()).Returns((ShipStatus)null!);
        MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

        bool found = Player.TryGetClosestPlayerInRange(mockSourcePlayer.Object, sourceRole, 5.0f, out var targetPlayer);

        Assert.False(found);
        Assert.Null(targetPlayer);
    }

    [Fact]
    public void TryGetClosestPlayerInRange_3Params_WhenNoPlayerInRange_ReturnsFalse()
    {
        var mockLocalPlayer = new Mock<PlayerControl>();
        var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(h => h.Invoke()).Returns(mockLocalPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
        mockShipHelper.Setup(h => h.Invoke()).Returns((ShipStatus)null!);
        MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

        bool found = Player.TryGetClosestPlayerInRange(sourceRole, 5.0f, out var targetPlayer);

        Assert.False(found);
        Assert.Null(targetPlayer);
    }
}
