using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using ExtremeRoles.Compat;
using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Ability;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Helper;

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

        SetupColorHelpers();
        SetupPaletteHelpers();
        SetupMathfHelpers();
        SetupCompatModManager();
        SetupUnityObjectOperators();
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
        PlayerCache.AllPlayerControl.Clear();
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

    private static void SetupCompatModManager()
    {
        if (CompatModManager.Instance == null)
        {
            CompatModManager.Initialize();
        }
    }

    private static void SetupMathfHelpers()
    {
        var mockClamp01 = new Mock<MockMathfClamp01Helper>();
        mockClamp01.Setup(h => h.Invoke(It.IsAny<float>())).Returns((float f) => Math.Clamp(f, 0f, 1f));
        MockMathfClamp01Helper.Instance = mockClamp01.Object;

        var mockClamp = new Mock<MockMathfClampHelper>();
        mockClamp.Setup(h => h.Invoke(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>())).Returns((float v, float min, float max) => Math.Clamp(v, min, max));
        MockMathfClampHelper.Instance = mockClamp.Object;

        var mockMax = new Mock<MockMathfMaxHelper>();
        mockMax.Setup(h => h.Invoke(It.IsAny<float>(), It.IsAny<float>())).Returns((float a, float b) => Math.Max(a, b));
        MockMathfMaxHelper.Instance = mockMax.Object;

        var mockMin = new Mock<MockMathfMinHelper>();
        mockMin.Setup(h => h.Invoke(It.IsAny<float>(), It.IsAny<float>())).Returns((float a, float b) => Math.Min(a, b));
        MockMathfMinHelper.Instance = mockMin.Object;

        var mockAbs = new Mock<MockMathfAbsHelper>();
        mockAbs.Setup(h => h.Invoke(It.IsAny<float>())).Returns((float f) => Math.Abs(f));
        MockMathfAbsHelper.Instance = mockAbs.Object;
    }

    private static void SetupColorHelpers()
    {
        MockColor32op_ImplicitHelper.Instance = new Mock<MockColor32op_ImplicitHelper>().Object;
        MockColor32op_ImplicitHelper2.Instance = new Mock<MockColor32op_ImplicitHelper2>().Object;
        MockColorop_ImplicitHelper.Instance = new Mock<MockColorop_ImplicitHelper>().Object;
        MockColorop_ImplicitHelper2.Instance = new Mock<MockColorop_ImplicitHelper2>().Object;

        MockColorget_blackHelper.Instance = new Mock<MockColorget_blackHelper>().Object;
        MockColorget_blueHelper.Instance = new Mock<MockColorget_blueHelper>().Object;
        MockColorget_clearHelper.Instance = new Mock<MockColorget_clearHelper>().Object;
        MockColorget_cyanHelper.Instance = new Mock<MockColorget_cyanHelper>().Object;
        MockColorget_grayHelper.Instance = new Mock<MockColorget_grayHelper>().Object;
        MockColorget_greenHelper.Instance = new Mock<MockColorget_greenHelper>().Object;
        MockColorget_greyHelper.Instance = new Mock<MockColorget_greyHelper>().Object;
        MockColorget_magentaHelper.Instance = new Mock<MockColorget_magentaHelper>().Object;
        MockColorget_redHelper.Instance = new Mock<MockColorget_redHelper>().Object;
        MockColorget_whiteHelper.Instance = new Mock<MockColorget_whiteHelper>().Object;
        MockColorget_yellowHelper.Instance = new Mock<MockColorget_yellowHelper>().Object;
    }

    private static void SetupPaletteHelpers()
    {
        MockPaletteget_CrewmateBlueHelper.Instance = new Mock<MockPaletteget_CrewmateBlueHelper>().Object;
        MockPaletteget_ImpostorRedHelper.Instance = new Mock<MockPaletteget_ImpostorRedHelper>().Object;
        MockPaletteget_WhiteHelper.Instance = new Mock<MockPaletteget_WhiteHelper>().Object;
        MockPaletteget_ClearWhiteHelper.Instance = new Mock<MockPaletteget_ClearWhiteHelper>().Object;
        MockPaletteget_BlackHelper.Instance = new Mock<MockPaletteget_BlackHelper>().Object;
    }

    private static void SetupUnityObjectOperators()
    {
        var mockEq = new Mock<MockObjectop_EqualityHelper>();
        mockEq.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<UnityEngine.Object>()))
            .Returns((UnityEngine.Object x, UnityEngine.Object y) =>
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                {
                    return false;
                }
                return ReferenceEquals(x, y);
            });
        MockObjectop_EqualityHelper.Instance = mockEq.Object;

        var mockIneq = new Mock<MockObjectop_InequalityHelper>();
        mockIneq.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<UnityEngine.Object>()))
            .Returns((UnityEngine.Object x, UnityEngine.Object y) =>
            {
                if (ReferenceEquals(x, y))
                {
                    return false;
                }
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                {
                    return true;
                }
                return !ReferenceEquals(x, y);
            });
        MockObjectop_InequalityHelper.Instance = mockIneq.Object;

        var mockImplicit = new Mock<MockObjectop_ImplicitHelper>();
        mockImplicit.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>()))
            .Returns((UnityEngine.Object obj) => !ReferenceEquals(obj, null));
        MockObjectop_ImplicitHelper.Instance = mockImplicit.Object;
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
    public void TryGetPlayerControl_WhenPlayerExists_ReturnsTrueAndPlayer()
    {
        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)5);
        PlayerCache.AllPlayerControl.Add(mockPlayer.Object);

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
        PlayerCache.AllPlayerControl.Add(mockPlayer.Object);

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

        var targetRole = new ExtremeRoles.Roles.Solo.Neutral.Monika();
        var sourceRole = new ExtremeRoles.Roles.Solo.Neutral.Jester();

        ExtremeRoleManager.GameRole[targetId] = targetRole;

        bool result = Player.IsValidPlayer(sourceRole, mockSourcePlayer.Object, mockTargetInfo.Object);

        Assert.True(result);
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
    public void TryGetClosestPlayerInRange_WhenNoPlayerInRange_ReturnsFalse()
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
}
