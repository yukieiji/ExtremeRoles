using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.Solo.Crewmate.Loner;
using MonoMod.RuntimeDetour;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Loner;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class LonerAbilityHandlerTests : IDisposable
{
	private delegate float Vector2MagOrig(ref Vector2 self);
	private delegate float Vector2MagHook(Vector2MagOrig orig, ref Vector2 self);

	private delegate void Vector2CtorOrig(ref Vector2 self, float x, float y);
	private delegate void Vector2CtorHook(Vector2CtorOrig orig, ref Vector2 self, float x, float y);

	private delegate void UncheckedMurderPlayerOrig(byte sourceId, byte targetId, byte useAnimation);
	private delegate void UncheckedMurderPlayerHook(UncheckedMurderPlayerOrig orig, byte sourceId, byte targetId, byte useAnimation);

	private delegate RPCOperator.RpcCaller CreateCallerOrig(uint netId, RPCOperator.Command ops);
	private delegate RPCOperator.RpcCaller CreateCallerHook(CreateCallerOrig orig, uint netId, RPCOperator.Command ops);

	private delegate void RpcWriteByteOrig(RPCOperator.RpcCaller self, byte value);
	private delegate void RpcWriteByteHook(RpcWriteByteOrig orig, RPCOperator.RpcCaller self, byte value);

	private delegate void RpcDisposeOrig(RPCOperator.RpcCaller self);
	private delegate void RpcDisposeHook(RpcDisposeOrig orig, RPCOperator.RpcCaller self);

	public static bool ReplaceDeadReasonCalled = false;
	private delegate void ReplaceDeadReasonOrig(ExtremeShipStatus self, byte playerId, ExtremeShipStatus.PlayerStatus newReason);
	private delegate void ReplaceDeadReasonHook(ReplaceDeadReasonOrig orig, ExtremeShipStatus self, byte playerId, ExtremeShipStatus.PlayerStatus newReason);

	private readonly Hook magHook;
	private readonly Hook ctorHook;
	private readonly Hook murderHook;
	private readonly Hook createCallerHook;
	private readonly Hook rpcWriteByteHook;
	private readonly Hook rpcDisposeHook;
	private readonly Hook replaceDeadReasonHook;

	public LonerAbilityHandlerTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		var mockExileControllerHelper = new Mock<MockExileControllerget_InstanceHelper>();
		mockExileControllerHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockExileControllerget_InstanceHelper.Instance = mockExileControllerHelper.Object;

		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);

		var mockMinigameHelper = new Mock<MockMinigameget_InstanceHelper>();
		mockMinigameHelper.Setup(h => h.Invoke()).Returns((Minigame)null!);
		MockMinigameget_InstanceHelper.Instance = mockMinigameHelper.Object;

		var mockSub = new Mock<MockVector2op_SubtractionHelper>();
		mockSub.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>()))
			.Returns((Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y));
		MockVector2op_SubtractionHelper.Instance = mockSub.Object;

		var mockTimeHelper = new Mock<MockTimeget_deltaTimeHelper>();
		mockTimeHelper.Setup(h => h.Invoke()).Returns(1.0f);
		MockTimeget_deltaTimeHelper.Instance = mockTimeHelper.Object;

		var magTarget = typeof(Vector2).GetProperty("magnitude")!.GetGetMethod()!;
		var magHookDelegate = new Vector2MagHook(getMagnitudeHook);
		this.magHook = new Hook(magTarget, magHookDelegate);

		var ctorTarget = typeof(Vector2).GetConstructor(new[] { typeof(float), typeof(float) })!;
		var ctorHookDelegate = new Vector2CtorHook(getCtorHook);
		this.ctorHook = new Hook(ctorTarget, ctorHookDelegate);

		var murderTarget = typeof(RPCOperator).GetMethod(nameof(RPCOperator.UncheckedMurderPlayer), BindingFlags.Public | BindingFlags.Static)!;
		var murderHookDelegate = new UncheckedMurderPlayerHook(getUncheckedMurderPlayerHook);
		this.murderHook = new Hook(murderTarget, murderHookDelegate);

		var createCallerTarget = typeof(RPCOperator).GetMethod(nameof(RPCOperator.CreateCaller), new[] { typeof(uint), typeof(RPCOperator.Command) })!;
		var createCallerHookDelegate = new CreateCallerHook(getCreateCallerHook);
		this.createCallerHook = new Hook(createCallerTarget, createCallerHookDelegate);

		var rpcWriteByteTarget = typeof(RPCOperator.RpcCaller).GetMethod(nameof(RPCOperator.RpcCaller.WriteByte))!;
		var rpcWriteByteHookDelegate = new RpcWriteByteHook(getRpcWriteByteHook);
		this.rpcWriteByteHook = new Hook(rpcWriteByteTarget, rpcWriteByteHookDelegate);

		var rpcDisposeTarget = typeof(RPCOperator.RpcCaller).GetMethod(nameof(RPCOperator.RpcCaller.Dispose))!;
		var rpcDisposeHookDelegate = new RpcDisposeHook(getRpcDisposeHook);
		this.rpcDisposeHook = new Hook(rpcDisposeTarget, rpcDisposeHookDelegate);

		var replaceTarget = typeof(ExtremeShipStatus).GetMethod(nameof(ExtremeShipStatus.ReplaceDeadReason), BindingFlags.Public | BindingFlags.Instance)!;
		var replaceHookDelegate = new ReplaceDeadReasonHook(getReplaceDeadReasonHook);
		this.replaceDeadReasonHook = new Hook(replaceTarget, replaceHookDelegate);
	}

	public void Dispose()
	{
		this.magHook.Dispose();
		this.ctorHook.Dispose();
		this.murderHook.Dispose();
		this.createCallerHook.Dispose();
		this.rpcWriteByteHook.Dispose();
		this.rpcDisposeHook.Dispose();
		this.replaceDeadReasonHook.Dispose();
	}

	private static float getMagnitudeHook(Vector2MagOrig orig, ref Vector2 self)
	{
		return (float)Math.Sqrt(self.x * self.x + self.y * self.y);
	}

	private static void getCtorHook(Vector2CtorOrig orig, ref Vector2 self, float x, float y)
	{
		self.x = x;
		self.y = y;
	}

	private static void getUncheckedMurderPlayerHook(UncheckedMurderPlayerOrig orig, byte sourceId, byte targetId, byte useAnimation)
	{
		// No-op to avoid calling PlayerControl.MurderPlayer native code
	}

	private static RPCOperator.RpcCaller getCreateCallerHook(CreateCallerOrig orig, uint netId, RPCOperator.Command ops)
	{
		return (RPCOperator.RpcCaller)RuntimeHelpers.GetUninitializedObject(typeof(RPCOperator.RpcCaller));
	}

	private static void getRpcWriteByteHook(RpcWriteByteOrig orig, RPCOperator.RpcCaller self, byte value)
	{
	}

	private static void getRpcDisposeHook(RpcDisposeOrig orig, RPCOperator.RpcCaller self)
	{
	}

	private static void getReplaceDeadReasonHook(ReplaceDeadReasonOrig orig, ExtremeShipStatus self, byte playerId, ExtremeShipStatus.PlayerStatus newReason)
	{
		ReplaceDeadReasonCalled = true;
		orig(self, playerId, newReason);
	}

	private static Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>> CreateMockPlayerList(List<NetworkedPlayerInfo?> players)
	{
		var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockList.SetupGet(l => l.Count).Returns(players.Count);
		mockList.Setup(l => l[It.IsAny<int>()]).Returns((int i) => players[i]!);
		return mockList;
	}

	[Fact]
	public void Constructor_InitializesMaxStressGage()
	{
		// Arrange
		var option = new StressProgress.Option(false, true, false);
		var status = new LonerStatusModel(2.5f, 10f, option);

		// Act
		var handler = new LonerAbilityHandler(100f, 1, true, status);

		// Assert
		var maxStressGage = handler.MaxStressGage;
		Assert.Equal(100f, maxStressGage);
	}

	[Fact]
	public void Update_WhenNotInTaskPhase_ResetsStress()
	{
		// Arrange
		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		GameProgressSystem.Current = GameProgressSystem.Progress.Meeting;

		var mockRolePlayer = MockSetupHelper.SetupPlayerControlMocks();
		var option = new StressProgress.Option(false, true, false);
		var status = new LonerStatusModel(2.5f, -10f, option);

		var handler = new LonerAbilityHandler(100f, 0, true, status);

		// Act
		handler.Update(mockRolePlayer.Object);

		// Assert
		var gage = status.StressGage;
		Assert.Equal(0f, gage);

		// Reset
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;
	}

	[Fact]
	public void Update_WhenRolePlayerInvalid_HidesArrowAndReturnsEarly()
	{
		// Arrange
		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(true);

		var mockRolePlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockRolePlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var option = new StressProgress.Option(false, true, false);
		var status = new LonerStatusModel(2.5f, 0f, option);
		var handler = new LonerAbilityHandler(100f, 0, true, status);

		// Act
		handler.Update(mockRolePlayer.Object);

		// Assert
		var gage = status.StressGage;
		Assert.Equal(0f, gage);
	}

	[Fact]
	public void Update_WhenStressBelowMax_DoesNotTriggerDespair()
	{
		// Arrange
		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;

		var mockRolePlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)0);
		mockRolePlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockRolePlayer.Object);
		mockData.SetupGet(d => d.PlayerId).Returns((byte)0);
		mockRolePlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var playersList = new List<NetworkedPlayerInfo?> { mockData.Object };
		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.SetupGet(g => g.AllPlayers).Returns(CreateMockPlayerList(playersList).Object);

		var option = new StressProgress.Option(true, true, true);
		var status = new LonerStatusModel(2.5f, -1.0f, option);
		var handler = new LonerAbilityHandler(100f, 0, true, status);

		// Act
		handler.Update(mockRolePlayer.Object);

		// Assert
		var gage = status.StressGage;
		Assert.True(gage < handler.MaxStressGage);
	}

	[Fact]
	public void Update_WhenStressReachesMax_TriggersDespair()
	{
		// Arrange
		ReplaceDeadReasonCalled = false;
		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.GameState).Returns(InnerNet.InnerNetClient.GameStates.Started);

		var cachedPtrField = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);

		var mockRolePlayer = MockSetupHelper.SetupPlayerControlMocks();
		cachedPtrField?.SetValue(mockRolePlayer.Object, (IntPtr)1);
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)0);
		mockRolePlayer.SetupGet(p => p.NetId).Returns((uint)1);
		mockRolePlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));
		ExtremeRoles.Performance.PlayerCache.AddPlayerControl(mockRolePlayer.Object);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockRolePlayer.Object);
		mockData.SetupGet(d => d.PlayerId).Returns((byte)0);
		mockRolePlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var mockOtherPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockOtherPlayer.Object, (IntPtr)1);
		mockOtherPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockOtherPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(1f, 0f));

		var mockOtherInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockOtherInfo.SetupGet(i => i.PlayerId).Returns((byte)1);
		mockOtherInfo.SetupGet(i => i.IsDead).Returns(false);
		mockOtherInfo.SetupGet(i => i.Disconnected).Returns(false);
		mockOtherInfo.SetupGet(i => i.Object).Returns(mockOtherPlayer.Object);
		mockOtherPlayer.SetupGet(p => p.Data).Returns(mockOtherInfo.Object);

		var playersList = new List<NetworkedPlayerInfo?> { mockData.Object, mockOtherInfo.Object };
		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.SetupGet(g => g.AllPlayers).Returns(CreateMockPlayerList(playersList).Object);

		var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty("ShipState", BindingFlags.Public | BindingFlags.Static)!;
		var shipState = new ExtremeShipStatus();
		shipState.AddDeadInfo(mockRolePlayer.Object, DeathReason.Kill, mockOtherPlayer.Object);
		shipStateProp.GetSetMethod(true)!.Invoke(null, new object[] { shipState });

		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;

		var option = new StressProgress.Option(true, true, true);
		var status = new LonerStatusModel(2.5f, -1.0f, option);
		var handler = new LonerAbilityHandler(0.05f, 0, true, status);

		// Act
		handler.Update(mockRolePlayer.Object);

		// Assert
		Assert.True(ReplaceDeadReasonCalled, "ReplaceDeadReason was NOT called!");
		Assert.True(shipState.DeadPlayerInfo.ContainsKey(0));
		Assert.Equal(ExtremeShipStatus.PlayerStatus.Despair, shipState.DeadPlayerInfo[0].Reason);
	}

	[Fact]
	public void Reset_DoesNotThrow()
	{
		// Arrange
		var option = new StressProgress.Option(false, true, false);
		var status = new LonerStatusModel(2.5f, 10f, option);
		var handler = new LonerAbilityHandler(100f, 0, true, status);

		// Act & Assert
		handler.Reset();
	}
}
