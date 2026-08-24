using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AmongUs.GameOptions;
using ExtremeRoles.Compat;
using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Neutral;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Helper;

public class GameSystemTests : IDisposable
{
    private static readonly IGameOptions globalGameOptions;
    private static readonly GameOptionsManager globalGameOptionsManager;
    private static readonly GameData globalGameData;
    private static readonly ShipStatus globalShipStatus;

    private static readonly Mock<MockGameDataget_InstanceHelper> globalGameDataHelper = new();
    private static readonly Mock<MockShipStatusget_InstanceHelper> globalShipHelper = new();
    private static readonly Mock<MockGameOptionsManagerget_InstanceHelper> globalOptionsHelper = new();
    private static readonly Mock<MockAmongUsClientget_InstanceHelper> globalAmongUsClientHelper = new();
    private static readonly Mock<MockGameManagerget_InstanceHelper> globalGameManagerHelper = new();

    static GameSystemTests()
    {
        var mockOptions = new Mock<IGameOptions>(IntPtr.Zero);
        globalGameOptions = mockOptions.Object;

        var mockManager = new Mock<GameOptionsManager>(IntPtr.Zero);
        mockManager.SetupGet(m => m.CurrentGameOptions).Returns(globalGameOptions);
        globalGameOptionsManager = mockManager.Object;

        globalGameData = new Mock<GameData>().Object;
        globalShipStatus = new Mock<ShipStatus>().Object;

        MockSetupHelper.SetupCommonMocks();
    }

    public GameSystemTests()
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

        globalGameDataHelper.Setup(h => h.Invoke()).Returns(globalGameData);
        MockGameDataget_InstanceHelper.Instance = globalGameDataHelper.Object;

        globalShipHelper.Setup(h => h.Invoke()).Returns(globalShipStatus);
        MockShipStatusget_InstanceHelper.Instance = globalShipHelper.Object;

        globalOptionsHelper.Setup(h => h.Invoke()).Returns(globalGameOptionsManager);
        MockGameOptionsManagerget_InstanceHelper.Instance = globalOptionsHelper.Object;

        var mockAmongUsClient = new Mock<AmongUsClient>();
        globalAmongUsClientHelper.Setup(h => h.Invoke()).Returns(mockAmongUsClient.Object);
        MockAmongUsClientget_InstanceHelper.Instance = globalAmongUsClientHelper.Object;

        var mockGameManager = new Mock<GameManager>();
        globalGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
        MockGameManagerget_InstanceHelper.Instance = globalGameManagerHelper.Object;
    }

    [Theory]
    [InlineData(InnerNet.InnerNetClient.GameStates.NotJoined, true)]
    [InlineData(InnerNet.InnerNetClient.GameStates.Joined, true)]
    [InlineData(InnerNet.InnerNetClient.GameStates.Ended, true)]
    [InlineData(InnerNet.InnerNetClient.GameStates.Started, false)]
    public void IsLobby_ReturnsExpected(InnerNet.InnerNetClient.GameStates gameState, bool expected)
    {
        var mockClient = new Mock<AmongUsClient>();
        mockClient.SetupGet(c => c.GameState).Returns(gameState);

        globalAmongUsClientHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);

        Assert.Equal(expected, GameSystem.IsLobby);
    }

    [Theory]
    [InlineData(NetworkModes.FreePlay, true)]
    [InlineData(NetworkModes.OnlineGame, false)]
    [InlineData(NetworkModes.LocalGame, false)]
    public void IsFreePlay_ReturnsExpected(NetworkModes networkMode, bool expected)
    {
        var mockClient = new Mock<AmongUsClient>();
        mockClient.SetupGet(c => c.NetworkMode).Returns(networkMode);

        globalAmongUsClientHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);

        Assert.Equal(expected, GameSystem.IsFreePlay);
    }


    [Fact]
    public void TryGetTaskDoRole_WhenDisconnected_ReturnsFalse()
    {
        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockInfo.SetupGet(p => p.Disconnected).Returns(true);

        bool result = GameSystem.TryGetTaskDoRole(mockInfo.Object, out var role);

        Assert.False(result);
        Assert.Null(role);
    }

    [Fact]
    public void TryGetTaskDoRole_WhenTasksNull_ReturnsFalse()
    {
        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockInfo.SetupGet(p => p.Disconnected).Returns(false);
        mockInfo.SetupGet(p => p.Tasks).Returns((Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>)null!);

        bool result = GameSystem.TryGetTaskDoRole(mockInfo.Object, out var role);

        Assert.False(result);
        Assert.Null(role);
    }

    [Fact]
    public void TryGetTaskDoRole_WhenObjectNull_ReturnsFalse()
    {
        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockInfo.SetupGet(p => p.Disconnected).Returns(false);
        var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
        mockInfo.SetupGet(p => p.Tasks).Returns(mockList.Object);
        mockInfo.SetupGet(p => p.Object).Returns((PlayerControl)null!);

        bool result = GameSystem.TryGetTaskDoRole(mockInfo.Object, out var role);

        Assert.False(result);
        Assert.Null(role);
    }

    [Fact]
    public void TryGetTaskDoRole_WhenDeadAndGhostsDoNotDoTasks_ReturnsFalse()
    {
        byte playerId = 1;

        var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
        mockLogicOptions.Setup(l => l.GetGhostsDoTasks()).Returns(false);

        var mockGameManager = new Mock<GameManager>();
        mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);
        globalGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);

        var mockPlayerControl = new Mock<PlayerControl>();
        mockPlayerControl.SetupGet(p => p.PlayerId).Returns(playerId);

        var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockInfo.SetupGet(p => p.PlayerId).Returns(playerId);
        mockInfo.SetupGet(p => p.Disconnected).Returns(false);
        mockInfo.SetupGet(p => p.Tasks).Returns(mockList.Object);
        mockInfo.SetupGet(p => p.Object).Returns(mockPlayerControl.Object);
        mockInfo.SetupGet(p => p.IsDead).Returns(true);

        bool result = GameSystem.TryGetTaskDoRole(mockInfo.Object, out var role);

        Assert.False(result);
        Assert.Null(role);
    }

    [Fact]
    public void TryGetTaskDoRole_WhenDeadAndGhostsDoTasks_AndRoleValid_ReturnsTrue()
    {
        byte playerId = 1;

        var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
        mockLogicOptions.Setup(l => l.GetGhostsDoTasks()).Returns(true);

        var mockGameManager = new Mock<GameManager>();
        mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);
        globalGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);

        var mockPlayerControl = new Mock<PlayerControl>();
        mockPlayerControl.SetupGet(p => p.PlayerId).Returns(playerId);

        var mockRoleBehaviour = new Mock<RoleBehaviour>();
        mockRoleBehaviour.SetupGet(r => r.TasksCountTowardProgress).Returns(true);

        var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockInfo.SetupGet(p => p.PlayerId).Returns(playerId);
        mockInfo.SetupGet(p => p.Disconnected).Returns(false);
        mockInfo.SetupGet(p => p.Tasks).Returns(mockList.Object);
        mockInfo.SetupGet(p => p.Object).Returns(mockPlayerControl.Object);
        mockInfo.SetupGet(p => p.IsDead).Returns(true);
        mockInfo.SetupGet(p => p.Role).Returns(mockRoleBehaviour.Object);

        var expectedRole = new Jester();
        ExtremeRoleManager.GameRole[playerId] = expectedRole;

        bool result = GameSystem.TryGetTaskDoRole(mockInfo.Object, out var role);

        Assert.True(result);
        Assert.Same(expectedRole, role);
    }

    [Fact]
    public void TryGetTaskDoRole_WhenRoleTasksDoNotProgress_ReturnsFalse()
    {
        byte playerId = 1;

        var mockPlayerControl = new Mock<PlayerControl>();
        mockPlayerControl.SetupGet(p => p.PlayerId).Returns(playerId);

        var mockRoleBehaviour = new Mock<RoleBehaviour>();
        mockRoleBehaviour.SetupGet(r => r.TasksCountTowardProgress).Returns(false);

        var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockInfo.SetupGet(p => p.PlayerId).Returns(playerId);
        mockInfo.SetupGet(p => p.Disconnected).Returns(false);
        mockInfo.SetupGet(p => p.Tasks).Returns(mockList.Object);
        mockInfo.SetupGet(p => p.Object).Returns(mockPlayerControl.Object);
        mockInfo.SetupGet(p => p.IsDead).Returns(false);
        mockInfo.SetupGet(p => p.Role).Returns(mockRoleBehaviour.Object);

        bool result = GameSystem.TryGetTaskDoRole(mockInfo.Object, out var role);

        Assert.False(result);
        Assert.Null(role);
    }

    [Fact]
    public void TryGetTaskDoRole_WhenRoleNotFoundInManager_ReturnsFalse()
    {
        byte playerId = 1;

        var mockPlayerControl = new Mock<PlayerControl>();
        mockPlayerControl.SetupGet(p => p.PlayerId).Returns(playerId);

        var mockRoleBehaviour = new Mock<RoleBehaviour>();
        mockRoleBehaviour.SetupGet(r => r.TasksCountTowardProgress).Returns(true);

        var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockInfo.SetupGet(p => p.PlayerId).Returns(playerId);
        mockInfo.SetupGet(p => p.Disconnected).Returns(false);
        mockInfo.SetupGet(p => p.Tasks).Returns(mockList.Object);
        mockInfo.SetupGet(p => p.Object).Returns(mockPlayerControl.Object);
        mockInfo.SetupGet(p => p.IsDead).Returns(false);
        mockInfo.SetupGet(p => p.Role).Returns(mockRoleBehaviour.Object);

        bool result = GameSystem.TryGetTaskDoRole(mockInfo.Object, out var role);

        Assert.False(result);
        Assert.Null(role);
    }

    [Fact]
    public void TryGetTaskDoRole_WhenAllValid_ReturnsTrueAndRole()
    {
        byte playerId = 1;

        var mockPlayerControl = new Mock<PlayerControl>();
        mockPlayerControl.SetupGet(p => p.PlayerId).Returns(playerId);

        var mockRoleBehaviour = new Mock<RoleBehaviour>();
        mockRoleBehaviour.SetupGet(r => r.TasksCountTowardProgress).Returns(true);

        var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockInfo.SetupGet(p => p.PlayerId).Returns(playerId);
        mockInfo.SetupGet(p => p.Disconnected).Returns(false);
        mockInfo.SetupGet(p => p.Tasks).Returns(mockList.Object);
        mockInfo.SetupGet(p => p.Object).Returns(mockPlayerControl.Object);
        mockInfo.SetupGet(p => p.IsDead).Returns(false);
        mockInfo.SetupGet(p => p.Role).Returns(mockRoleBehaviour.Object);

        var expectedRole = new Jester();
        ExtremeRoleManager.GameRole[playerId] = expectedRole;

        bool result = GameSystem.TryGetTaskDoRole(mockInfo.Object, out var role);

        Assert.True(result);
        Assert.Same(expectedRole, role);
    }

    [Fact]
    public void GetTaskInfo_WhenTryGetTaskDoRoleFails_ReturnsZeroZero()
    {
        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockInfo.SetupGet(p => p.Disconnected).Returns(true);

        var (completed, total) = GameSystem.GetTaskInfo(mockInfo.Object);

        Assert.Equal(0, completed);
        Assert.Equal(0, total);
    }

    [Fact]
    public void GetTaskInfo_WhenPlayerHasTasks_ReturnsCorrectCounts()
    {
        byte playerId = 1;

        var mockPlayerControl = new Mock<PlayerControl>();
        mockPlayerControl.SetupGet(p => p.PlayerId).Returns(playerId);

        var mockRoleBehaviour = new Mock<RoleBehaviour>();
        mockRoleBehaviour.SetupGet(r => r.TasksCountTowardProgress).Returns(true);

        var task1 = new Mock<NetworkedPlayerInfo.TaskInfo>();
        task1.SetupGet(t => t.Complete).Returns(true);

        var task2 = new Mock<NetworkedPlayerInfo.TaskInfo>();
        task2.SetupGet(t => t.Complete).Returns(false);

        var task3 = new Mock<NetworkedPlayerInfo.TaskInfo>();
        task3.SetupGet(t => t.Complete).Returns(true);

        var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
        mockList.SetupGet(l => l.Count).Returns(3);
        mockList.SetupGet(l => l[0]).Returns(task1.Object);
        mockList.SetupGet(l => l[1]).Returns(task2.Object);
        mockList.SetupGet(l => l[2]).Returns(task3.Object);

        var mockInfo = new Mock<NetworkedPlayerInfo>();
        mockInfo.SetupGet(p => p.PlayerId).Returns(playerId);
        mockInfo.SetupGet(p => p.Disconnected).Returns(false);
        mockInfo.SetupGet(p => p.Tasks).Returns(mockList.Object);
        mockInfo.SetupGet(p => p.Object).Returns(mockPlayerControl.Object);
        mockInfo.SetupGet(p => p.IsDead).Returns(false);
        mockInfo.SetupGet(p => p.Role).Returns(mockRoleBehaviour.Object);

        var expectedRole = new Jester();
        expectedRole.HasTask = true;
        ExtremeRoleManager.GameRole[playerId] = expectedRole;

        var (completed, total) = GameSystem.GetTaskInfo(mockInfo.Object);

        Assert.Equal(2, completed);
        Assert.Equal(3, total);
    }

    [Fact]
    public void GetRandomCommonTaskId_WhenShipStatusNull_ReturnsByteMax()
    {
        globalShipHelper.Setup(h => h.Invoke()).Returns((ShipStatus)null!);

        int result = GameSystem.GetRandomCommonTaskId();

        Assert.Equal(byte.MaxValue, result);
    }

    [Fact]
    public void GetRandomShortTaskId_WhenShortTasksNull_ReturnsByteMax()
    {
        var mockShip = new Mock<ShipStatus>();
        mockShip.SetupGet(s => s.ShortTasks).Returns((NormalPlayerTask[])null!);
        globalShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);

        int result = GameSystem.GetRandomShortTaskId();

        Assert.Equal(byte.MaxValue, result);
    }


    [Fact]
    public void GetRandomLongTask_WhenShipStatusNull_ReturnsByteMax()
    {
        globalShipHelper.Setup(h => h.Invoke()).Returns((ShipStatus)null!);

        int result = GameSystem.GetRandomLongTask();

        Assert.Equal(byte.MaxValue, result);
    }

    [Fact]
    public void GetRandomShortTaskId_WhenShipStatusNull_ReturnsByteMax()
    {
        globalShipHelper.Setup(h => h.Invoke()).Returns((ShipStatus)null!);

        int result = GameSystem.GetRandomShortTaskId();

        Assert.Equal(byte.MaxValue, result);
    }

    [Fact]
    public void IsValidConsole_WhenPlayerOrConsoleNull_ReturnsFalse()
    {
        Assert.False(GameSystem.IsValidConsole(null!, null!));

        var mockPlayer = new Mock<PlayerControl>();
        Assert.False(GameSystem.IsValidConsole(mockPlayer.Object, null!));
    }

    [Fact]
    public void TryGetKillDistance_WhenGameManagerNull_ReturnsFalse()
    {
        globalGameManagerHelper.Setup(h => h.Invoke()).Returns((GameManager)null!);

        bool result = GameSystem.TryGetKillDistance(out var arr);

        Assert.False(result);
        Assert.Null(arr);
    }

    [Fact]
    public void TryGetKillDistance_WhenLogicOptionsNull_ReturnsFalse()
    {
        var mockGameManager = new Mock<GameManager>();
        mockGameManager.SetupGet(g => g.LogicOptions).Returns((LogicOptions)null!);
        globalGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);

        bool result = GameSystem.TryGetKillDistance(out var arr);

        Assert.False(result);
        Assert.Null(arr);
    }

    [Fact]
    public void TryGetKillDistance_WhenCurrentGameOptionsNull_ReturnsFalse()
    {
        var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
        mockLogicOptions.SetupGet(l => l.currentGameOptions).Returns((IGameOptions)null!);

        var mockGameManager = new Mock<GameManager>();
        mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);
        globalGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);

        bool result = GameSystem.TryGetKillDistance(out var arr);

        Assert.False(result);
        Assert.Null(arr);
    }

    [Fact]
    public void TryGetKillDistance_WhenKillDistancesPresent_ReturnsTrueAndArray()
    {
        var mockArray = new Mock<Il2CppStructArray<float>>(IntPtr.Zero).Object;

        var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
        mockGameOptions.Setup(g => g.GetFloatArray(FloatArrayOptionNames.KillDistances)).Returns(mockArray);

        var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
        mockLogicOptions.SetupGet(l => l.currentGameOptions).Returns(mockGameOptions.Object);

        var mockGameManager = new Mock<GameManager>();
        mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);
        globalGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);

        bool result = GameSystem.TryGetKillDistance(out var arr);

        Assert.True(result);
        Assert.Same(mockArray, arr);
    }

    [Fact]
    public void GetDeadBody_WhenNoDeadBodiesExist_ReturnsNull()
    {
        byte targetPlayerId = 5;

		var mockFindObjects3 = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects3.Setup(x => x.Invoke<DeadBody>()).Returns(new Il2CppReferenceArray<DeadBody>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects3.Object;

		DeadBody? body = GameSystem.GetDeadBody(targetPlayerId);

        Assert.Null(body);
    }

	[Fact]
	public void GetDeadBody_WhenDeadBodiesExist_ReturnsBody()
	{
		byte targetPlayerId = 5;


		var mockDeadBody1 = new Mock<DeadBody>(IntPtr.Zero);
		mockDeadBody1.SetupGet(d => d.ParentId).Returns(targetPlayerId);
		var mockDeadBody2 = new Mock<DeadBody>(IntPtr.Zero);
		mockDeadBody2.SetupGet(d => d.ParentId).Returns((byte)10);

		var mockFindObjects3 = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects3.Setup(x => x.Invoke<DeadBody>()).Returns(new Il2CppReferenceArray<DeadBody>([mockDeadBody1.Object, mockDeadBody2.Object]));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects3.Object;

		DeadBody? body = GameSystem.GetDeadBody(targetPlayerId);

		Assert.NotNull(body);
		Assert.Same(mockDeadBody1.Object, body);
	}

	[Fact]
    public void GetShipObj_WhenMapIdExceeds5_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => GameSystem.GetShipObj(6));
    }
}
