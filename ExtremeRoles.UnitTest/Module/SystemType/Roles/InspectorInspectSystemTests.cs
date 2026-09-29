using System;
using System.Collections;
using System.Reflection;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using MonoMod.RuntimeDetour;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class InspectorInspectSystemTests : IDisposable
{
	private delegate void ArrowCtorOrig(Arrow self, Color color);
	private delegate void ArrowCtorHook(ArrowCtorOrig orig, Arrow self, Color color);

	private delegate void ArrowUpdateTargetOrig(Arrow self, Vector3? target);
	private delegate void ArrowUpdateTargetHook(ArrowUpdateTargetOrig orig, Arrow self, Vector3? target);

	private delegate void ArrowUpdateOrig(Arrow self);
	private delegate void ArrowUpdateHook(ArrowUpdateOrig orig, Arrow self);

	private delegate void ArrowClearOrig(Arrow self);
	private delegate void ArrowClearHook(ArrowClearOrig orig, Arrow self);

	private readonly Hook arrowCtorHook;
	private readonly Hook arrowUpdateTargetHook;
	private readonly Hook? arrowUpdateHook;
	private readonly Hook? arrowClearHook;

	private readonly Mock<AmongUsClient> clientMock;

	public InspectorInspectSystemTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupTimeHelpers();

		SetupShipStatusMock();

		clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		if (Hazel.MockMessageWriterGetHelper.Instance == null)
		{
			var mockGet = new Mock<Hazel.MockMessageWriterGetHelper>();
			mockGet.Setup(g => g.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
			Hazel.MockMessageWriterGetHelper.Instance = mockGet.Object;
		}

		if (Hazel.MockMessageReaderGetHelper.Instance == null)
		{
			var mockReaderHelper = new Mock<Hazel.MockMessageReaderGetHelper>();
			var mockReader = new Mock<MessageReader>();
			mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppStructArray<byte>>()))
				.Returns(mockReader.Object);
			Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;
		}

		if (InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance == null)
		{
			var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
			mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNetObject>()));
			InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;
		}

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");

		var arrowCtorTarget = typeof(Arrow).GetConstructor(new[] { typeof(Color) })!;
		var arrowCtorDelegate = new ArrowCtorHook((orig, self, color) => { });
		this.arrowCtorHook = new Hook(arrowCtorTarget, arrowCtorDelegate);

		var arrowUpdateTargetTarget = typeof(Arrow).GetMethod(nameof(Arrow.UpdateTarget), BindingFlags.Public | BindingFlags.Instance)!;
		var arrowUpdateTargetDelegate = new ArrowUpdateTargetHook((orig, self, target) => { });
		this.arrowUpdateTargetHook = new Hook(arrowUpdateTargetTarget, arrowUpdateTargetDelegate);

		var arrowUpdateTarget = typeof(Arrow).GetMethod(nameof(Arrow.Update), BindingFlags.Public | BindingFlags.Instance);
		if (arrowUpdateTarget != null)
		{
			var arrowUpdateDelegate = new ArrowUpdateHook((orig, self) => { });
			this.arrowUpdateHook = new Hook(arrowUpdateTarget, arrowUpdateDelegate);
		}

		var arrowClearTarget = typeof(Arrow).GetMethod(nameof(Arrow.Clear), BindingFlags.Public | BindingFlags.Instance);
		if (arrowClearTarget != null)
		{
			var arrowClearDelegate = new ArrowClearHook((orig, self) => { });
			this.arrowClearHook = new Hook(arrowClearTarget, arrowClearDelegate);
		}
	}

	public void Dispose()
	{
		this.arrowCtorHook.Dispose();
		this.arrowUpdateTargetHook.Dispose();
		this.arrowUpdateHook?.Dispose();
		this.arrowClearHook?.Dispose();
	}

	private static void SetupShipStatusMock()
	{
		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(x => x.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;
	}

	private static Mock<PlayerControl> CreatePlayerMock(byte playerId, bool isDead = false, bool disconnected = false)
	{
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.PlayerId).Returns(playerId);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);
		mockData.SetupGet(d => d.IsDead).Returns(isDead);
		mockData.SetupGet(d => d.Disconnected).Returns(disconnected);

		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		return mockPlayer;
	}

	[Fact]
	public void EndInspect_And_Reset_ClearsAllTargets()
	{
		var system = new InspectorInspectSystem(InspectorInspectSystem.InspectMode.Sabotage | InspectorInspectSystem.InspectMode.Vent);
		Assert.False(system.IsDirty);

		var mockPlayer1 = CreatePlayerMock(1);
		PlayerCache.AddPlayerControl(mockPlayer1.Object);

		var rStart = new Mock<MessageReader>();
		rStart.SetupSequence(r => r.ReadByte())
			.Returns((byte)InspectorInspectSystem.Ops.StartInspect);
		system.UpdateSystem(mockPlayer1.Object, rStart.Object);

		var allTargetField = typeof(InspectorInspectSystem).GetField("allTarget", BindingFlags.NonPublic | BindingFlags.Instance);
		var allTarget = (IDictionary)allTargetField!.GetValue(system)!;
		Assert.True(allTarget.Contains((byte)1));

		system.EndInspect(1);
		Assert.False(allTarget.Contains((byte)1));

		system.UpdateSystem(mockPlayer1.Object, rStart.Object);
		Assert.True(allTarget.Contains((byte)1));

		system.Reset(ResetTiming.MeetingStart, null);
		Assert.Empty(allTarget);
	}

	[Fact]
	public void UpdateSystem_AllOps_UpdatesTargetPlayerContainer()
	{
		var system = new InspectorInspectSystem(InspectorInspectSystem.InspectMode.Ability);

		var mockPlayer1 = CreatePlayerMock(1);
		var mockPlayer2 = CreatePlayerMock(2);

		PlayerCache.AddPlayerControl(mockPlayer1.Object);
		PlayerCache.AddPlayerControl(mockPlayer2.Object);

		var allTargetField = typeof(InspectorInspectSystem).GetField("allTarget", BindingFlags.NonPublic | BindingFlags.Instance);
		var allTarget = (IDictionary)allTargetField!.GetValue(system)!;

		// StartInspect
		var rStart = new Mock<MessageReader>();
		rStart.SetupSequence(r => r.ReadByte())
			.Returns((byte)InspectorInspectSystem.Ops.StartInspect);
		system.UpdateSystem(mockPlayer1.Object, rStart.Object);

		Assert.True(allTarget.Contains((byte)1));
		var targetContainer = allTarget[(byte)1];
		Assert.NotNull(targetContainer);

		// Add
		var rAdd = new Mock<MessageReader>();
		rAdd.SetupSequence(r => r.ReadByte())
			.Returns((byte)InspectorInspectSystem.Ops.Add)
			.Returns((byte)2);
		system.UpdateSystem(mockPlayer1.Object, rAdd.Object);

		var containMethod = targetContainer.GetType().GetMethod("Contain", BindingFlags.Public | BindingFlags.Instance);
		Assert.NotNull(containMethod);
		bool containsPlayer2 = (bool)containMethod.Invoke(targetContainer, new object[] { mockPlayer2.Object })!;
		Assert.True(containsPlayer2);

		// EndInspect
		var rEnd = new Mock<MessageReader>();
		rEnd.SetupSequence(r => r.ReadByte())
			.Returns((byte)InspectorInspectSystem.Ops.EndInspect);
		system.UpdateSystem(mockPlayer1.Object, rEnd.Object);

		Assert.False(allTarget.Contains((byte)1));
	}

	[Fact]
	public void Deteriorate_WhenLocalPlayerDeadOrDisconnected_RemovesLocalPlayerFromAllTarget()
	{
		var system = new InspectorInspectSystem(InspectorInspectSystem.InspectMode.Sabotage);

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(true);
		localPlayerMock.SetupGet(p => p.Data).Returns(mockData.Object);
		localPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)1);

		var rStart = new Mock<MessageReader>();
		rStart.SetupSequence(r => r.ReadByte())
			.Returns((byte)InspectorInspectSystem.Ops.StartInspect);
		system.UpdateSystem(localPlayerMock.Object, rStart.Object);

		var allTargetField = typeof(InspectorInspectSystem).GetField("allTarget", BindingFlags.NonPublic | BindingFlags.Instance);
		var allTarget = (IDictionary)allTargetField!.GetValue(system)!;
		Assert.True(allTarget.Contains((byte)1));

		system.Deteriorate(0.1f);

		Assert.False(allTarget.Contains((byte)1));
	}
}
