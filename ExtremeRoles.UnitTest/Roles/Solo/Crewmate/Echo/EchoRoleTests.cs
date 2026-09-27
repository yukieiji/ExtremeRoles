using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using Hazel;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using MonoMod.RuntimeDetour;
using Moq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Xunit;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.EchoTests;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class EchoRoleTests : IDisposable
{
	private delegate RPCOperator.RpcCaller CreateCaller1Orig(RPCOperator.Command ops);
	private delegate RPCOperator.RpcCaller CreateCaller1Hook(CreateCaller1Orig orig, RPCOperator.Command ops);

	private delegate RPCOperator.RpcCaller CreateCaller2Orig(uint netId, RPCOperator.Command ops);
	private delegate RPCOperator.RpcCaller CreateCaller2Hook(CreateCaller2Orig orig, uint netId, RPCOperator.Command ops);

	private delegate void RpcWriteByteOrig(RPCOperator.RpcCaller self, byte value);
	private delegate void RpcWriteByteHook(RpcWriteByteOrig orig, RPCOperator.RpcCaller self, byte value);

	private delegate void RpcWriteFloatOrig(RPCOperator.RpcCaller self, float value);
	private delegate void RpcWriteFloatHook(RpcWriteFloatOrig orig, RPCOperator.RpcCaller self, float value);

	private delegate void RpcDisposeOrig(RPCOperator.RpcCaller self);
	private delegate void RpcDisposeHook(RpcDisposeOrig orig, RPCOperator.RpcCaller self);

	private delegate Coroutine StartCoroutineOrig(MonoBehaviour self, IEnumerator coroutine);
	private delegate Coroutine StartCoroutineHook(StartCoroutineOrig orig, MonoBehaviour self, IEnumerator coroutine);

	private delegate Il2CppSystem.Type TypeFromPointerInternalOrig(IntPtr classPointer, string typeName, bool throwOnFailure);
	private delegate Il2CppSystem.Type TypeFromPointerInternalHook(TypeFromPointerInternalOrig orig, IntPtr classPointer, string typeName, bool throwOnFailure);

	private delegate float Vector2SqrMagOrig(ref Vector2 self);
	private delegate float Vector2SqrMagHook(Vector2SqrMagOrig orig, ref Vector2 self);

	private delegate float Vector3SqrMagOrig(ref Vector3 self);
	private delegate float Vector3SqrMagHook(Vector3SqrMagOrig orig, ref Vector3 self);

	private delegate void Vector2CtorOrig(ref Vector2 self, float x, float y);
	private delegate void Vector2CtorHook(Vector2CtorOrig orig, ref Vector2 self, float x, float y);

	private delegate bool IsInValidNetPlayerOrig(NetworkedPlayerInfo? self);
	private delegate bool IsInValidNetPlayerHook(IsInValidNetPlayerOrig orig, NetworkedPlayerInfo? self);

	private delegate bool IsInValidPlayerControlOrig(PlayerControl? self);
	private delegate bool IsInValidPlayerControlHook(IsInValidPlayerControlOrig orig, PlayerControl? self);

	private delegate Vector2 GetTruePositionOrig(PlayerControl self);
	private delegate Vector2 GetTruePositionHook(GetTruePositionOrig orig, PlayerControl self);

	private readonly Hook createCaller1Hook;
	private readonly Hook createCaller2Hook;
	private readonly Hook rpcWriteByteHook;
	private readonly Hook rpcWriteFloatHook;
	private readonly Hook rpcDisposeHook;
	private readonly Hook startCoroutineHook;
	private readonly Hook typeFromPointerHook;
	private readonly Hook sqrMagHook;
	private readonly Hook v3SqrMagHook;
	private readonly Hook v2CtorHook;
	private readonly Hook isInValidNetPlayerHook;
	private readonly Hook isInValidPlayerControlHook;
	private readonly Hook getTruePositionHook;

	private readonly Mock<AmongUsClient> clientMock;
	private readonly Mock<PlayerControl> localPlayerMock;

	public static int StartCoroutineCallCount { get; private set; }
	internal static readonly Dictionary<IntPtr, Vector2> MockPointerPositions = new();

	public EchoRoleTests()
	{
		StartCoroutineCallCount = 0;
		MockPointerPositions.Clear();

		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();

		var v2CtorTarget = typeof(Vector2).GetConstructor(new[] { typeof(float), typeof(float) })!;
		var v2CtorHookDelegate = new Vector2CtorHook(getV2CtorHook);
		this.v2CtorHook = new Hook(v2CtorTarget, v2CtorHookDelegate);

		var isInValidNetPlayerTarget = typeof(PlayerControlExtension)
			.GetMethod(nameof(PlayerControlExtension.IsInValid), new[] { typeof(NetworkedPlayerInfo) })!;
		var isInValidNetPlayerHookDelegate = new IsInValidNetPlayerHook(getIsInValidNetPlayerHook);
		this.isInValidNetPlayerHook = new Hook(isInValidNetPlayerTarget, isInValidNetPlayerHookDelegate);

		var isInValidPlayerControlTarget = typeof(PlayerControlExtension)
			.GetMethod(nameof(PlayerControlExtension.IsInValid), new[] { typeof(PlayerControl) })!;
		var isInValidPlayerControlHookDelegate = new IsInValidPlayerControlHook(getIsInValidPlayerControlHook);
		this.isInValidPlayerControlHook = new Hook(isInValidPlayerControlTarget, isInValidPlayerControlHookDelegate);

		var getTruePositionTarget = typeof(PlayerControl)
			.GetMethod(nameof(PlayerControl.GetTruePosition), BindingFlags.Public | BindingFlags.Instance)!;
		var getTruePositionHookDelegate = new GetTruePositionHook(getGetTruePositionHook);
		this.getTruePositionHook = new Hook(getTruePositionTarget, getTruePositionHookDelegate);

		var mockSub2 = new Mock<MockVector2op_SubtractionHelper>();
		mockSub2.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>()))
			.Returns((Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y));
		MockVector2op_SubtractionHelper.Instance = mockSub2.Object;

		var mockSub3 = new Mock<MockVector3op_SubtractionHelper>();
		mockSub3.Setup(x => x.Invoke(It.IsAny<Vector3>(), It.IsAny<Vector3>()))
			.Returns((Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z));
		MockVector3op_SubtractionHelper.Instance = mockSub3.Object;

		var mockColorSub = new Mock<MockColorop_SubtractionHelper>();
		mockColorSub.Setup(x => x.Invoke(It.IsAny<Color>(), It.IsAny<Color>()))
			.Returns((Color a, Color b) => new Color(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a));
		MockColorop_SubtractionHelper.Instance = mockColorSub.Object;

		var mockSqrMag = new Mock<MockVector2SqrMagnitudeHelper>();
		mockSqrMag.Setup(x => x.Invoke(It.IsAny<Vector2>()))
			.Returns((Vector2 v) => v.x * v.x + v.y * v.y);
		MockVector2SqrMagnitudeHelper.Instance = mockSqrMag.Object;

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		this.localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		this.localPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)1);
		this.localPlayerMock.SetupGet(p => p.CanMove).Returns(true);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(this.localPlayerMock.Object);
		this.localPlayerMock.SetupGet(p => p.Data).Returns(mockData.Object);

		var mockPlayerTransform = new Mock<Transform>(IntPtr.Zero);
		mockPlayerTransform.SetupGet(t => t.position).Returns(new Vector3(0f, 0f, 0f));
		this.localPlayerMock.SetupGet(p => p.transform).Returns(mockPlayerTransform.Object);

		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		this.clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		this.clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		SetupAssetBundleMock();

		var createCaller1Target = typeof(RPCOperator).GetMethod(nameof(RPCOperator.CreateCaller), new[] { typeof(RPCOperator.Command) })!;
		var createCaller1HookDelegate = new CreateCaller1Hook(getCreateCaller1Hook);
		this.createCaller1Hook = new Hook(createCaller1Target, createCaller1HookDelegate);

		var createCaller2Target = typeof(RPCOperator).GetMethod(nameof(RPCOperator.CreateCaller), new[] { typeof(uint), typeof(RPCOperator.Command) })!;
		var createCaller2HookDelegate = new CreateCaller2Hook(getCreateCaller2Hook);
		this.createCaller2Hook = new Hook(createCaller2Target, createCaller2HookDelegate);

		var rpcWriteByteTarget = typeof(RPCOperator.RpcCaller).GetMethod(nameof(RPCOperator.RpcCaller.WriteByte))!;
		var rpcWriteByteHookDelegate = new RpcWriteByteHook(getRpcWriteByteHook);
		this.rpcWriteByteHook = new Hook(rpcWriteByteTarget, rpcWriteByteHookDelegate);

		var rpcWriteFloatTarget = typeof(RPCOperator.RpcCaller).GetMethod(nameof(RPCOperator.RpcCaller.WriteFloat))!;
		var rpcWriteFloatHookDelegate = new RpcWriteFloatHook(getRpcWriteFloatHook);
		this.rpcWriteFloatHook = new Hook(rpcWriteFloatTarget, rpcWriteFloatHookDelegate);

		var rpcDisposeTarget = typeof(RPCOperator.RpcCaller).GetMethod(nameof(RPCOperator.RpcCaller.Dispose))!;
		var rpcDisposeHookDelegate = new RpcDisposeHook(getRpcDisposeHook);
		this.rpcDisposeHook = new Hook(rpcDisposeTarget, rpcDisposeHookDelegate);

		var startCoroutineTarget = typeof(MonoBehaviourExtensions)
			.GetMethod(nameof(MonoBehaviourExtensions.StartCoroutine), new[] { typeof(MonoBehaviour), typeof(IEnumerator) })!;
		var startCoroutineHookDelegate = new StartCoroutineHook(getStartCoroutineHook);
		this.startCoroutineHook = new Hook(startCoroutineTarget, startCoroutineHookDelegate);

		var typeFromPointerTarget = typeof(Il2CppType)
			.GetMethod("TypeFromPointerInternal", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)!;
		var typeFromPointerHookDelegate = new TypeFromPointerInternalHook(getTypeFromPointerInternalHook);
		this.typeFromPointerHook = new Hook(typeFromPointerTarget, typeFromPointerHookDelegate);

		var sqrMagTarget = typeof(Vector2).GetProperty("sqrMagnitude")!.GetGetMethod()!;
		var sqrMagHookDelegate = new Vector2SqrMagHook(getSqrMagnitudeHook);
		this.sqrMagHook = new Hook(sqrMagTarget, sqrMagHookDelegate);

		var v3SqrMagTarget = typeof(Vector3).GetProperty("sqrMagnitude")!.GetGetMethod()!;
		var v3SqrMagHookDelegate = new Vector3SqrMagHook(getVector3SqrMagnitudeHook);
		this.v3SqrMagHook = new Hook(v3SqrMagTarget, v3SqrMagHookDelegate);

		SetLobbyMode(false);
	}

	public void Dispose()
	{
		this.v2CtorHook.Dispose();
		this.isInValidNetPlayerHook.Dispose();
		this.isInValidPlayerControlHook.Dispose();
		this.getTruePositionHook.Dispose();
		this.createCaller1Hook.Dispose();
		this.createCaller2Hook.Dispose();
		this.rpcWriteByteHook.Dispose();
		this.rpcWriteFloatHook.Dispose();
		this.rpcDisposeHook.Dispose();
		this.startCoroutineHook.Dispose();
		this.typeFromPointerHook.Dispose();
		this.sqrMagHook.Dispose();
		this.v3SqrMagHook.Dispose();
	}

	private static RPCOperator.RpcCaller getCreateCaller1Hook(CreateCaller1Orig orig, RPCOperator.Command ops)
	{
		return (RPCOperator.RpcCaller)RuntimeHelpers.GetUninitializedObject(typeof(RPCOperator.RpcCaller));
	}

	private static RPCOperator.RpcCaller getCreateCaller2Hook(CreateCaller2Orig orig, uint netId, RPCOperator.Command ops)
	{
		return (RPCOperator.RpcCaller)RuntimeHelpers.GetUninitializedObject(typeof(RPCOperator.RpcCaller));
	}

	private static void getRpcWriteByteHook(RpcWriteByteOrig orig, RPCOperator.RpcCaller self, byte value) { }
	private static void getRpcWriteFloatHook(RpcWriteFloatOrig orig, RPCOperator.RpcCaller self, float value) { }
	private static void getRpcDisposeHook(RpcDisposeOrig orig, RPCOperator.RpcCaller self) { }

	private static Coroutine getStartCoroutineHook(StartCoroutineOrig orig, MonoBehaviour self, IEnumerator coroutine)
	{
		StartCoroutineCallCount++;
		return (Coroutine)RuntimeHelpers.GetUninitializedObject(typeof(Coroutine));
	}

	private static Il2CppSystem.Type getTypeFromPointerInternalHook(TypeFromPointerInternalOrig orig, IntPtr classPointer, string typeName, bool throwOnFailure)
	{
		return (Il2CppSystem.Type)RuntimeHelpers.GetUninitializedObject(typeof(Il2CppSystem.Type));
	}

	private static float getSqrMagnitudeHook(Vector2SqrMagOrig orig, ref Vector2 self)
	{
		return self.x * self.x + self.y * self.y;
	}

	private static float getVector3SqrMagnitudeHook(Vector3SqrMagOrig orig, ref Vector3 self)
	{
		return self.x * self.x + self.y * self.y + self.z * self.z;
	}

	private static void getV2CtorHook(Vector2CtorOrig orig, ref Vector2 self, float x, float y)
	{
		self.x = x;
		self.y = y;
	}

	private static bool getIsInValidNetPlayerHook(IsInValidNetPlayerOrig orig, NetworkedPlayerInfo? self)
	{
		return self == null || self.IsDead || self.Disconnected;
	}

	private static bool getIsInValidPlayerControlHook(IsInValidPlayerControlOrig orig, PlayerControl? self)
	{
		return self == null || self.Data == null || self.Data.IsDead || self.Data.Disconnected;
	}

	private static Vector2 getGetTruePositionHook(GetTruePositionOrig orig, PlayerControl self)
	{
		if (self != null && MockPointerPositions.TryGetValue(self.Pointer, out var pos))
		{
			return pos;
		}
		return orig(self);
	}

	private void SetLobbyMode(bool isLobby)
	{
		if (isLobby)
		{
			this.clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.NotJoined);
		}
		else
		{
			this.clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
		}
	}

	private static void SetupAssetBundleMock()
	{
		var mockAssetBundle = new Mock<AssetBundle>(IntPtr.Zero);
		var mockSprite = new Mock<Sprite>(IntPtr.Zero);

		mockAssetBundle.Setup(b => b.LoadAsset(It.IsAny<string>(), It.IsAny<Il2CppSystem.Type>()))
			.Returns(mockSprite.Object);

		var field = typeof(UnityObjectLoader).GetField("cachedBundle", BindingFlags.NonPublic | BindingFlags.Static);
		if (field?.GetValue(null) is Dictionary<string, AssetBundle> dict)
		{
			dict[ObjectPath.GetRoleAssetPath(ExtremeRoleId.Echo)] = mockAssetBundle.Object;
		}
	}

	private static Mock<ObjectPoolBehavior> SetupMockPool(Echo echo)
	{
		var mockPool = new Mock<ObjectPoolBehavior>(IntPtr.Zero);
		var mockList = new Mock<Il2CppSystem.Collections.Generic.List<PoolableBehavior>>(IntPtr.Zero);
		var mockEnum = new Mock<Il2CppSystem.Collections.Generic.List<PoolableBehavior>.Enumerator>(IntPtr.Zero);
		mockList.Setup(l => l.GetEnumerator()).Returns(mockEnum.Object);

		mockPool.SetupGet(p => p.activeChildren).Returns(mockList.Object);

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);

		var mockImageRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockImageRenderer.SetupProperty(r => r.sortingOrder);
		mockImageRenderer.SetupProperty(r => r.color);

		var mockPing = new Mock<PingBehaviour>(IntPtr.Zero);
		mockPing.SetupGet(p => p.transform).Returns(mockTransform.Object);
		mockPing.SetupGet(p => p.gameObject).Returns(mockGameObject.Object);
		mockPing.SetupGet(p => p.image).Returns(mockImageRenderer.Object);
		mockPing.SetupProperty(p => p.target);
		mockPing.SetupProperty(p => p.AmSeeker);
		mockPing.SetupProperty(p => p.MaxScale);

		mockPool.Setup(p => p.Get<PingBehaviour>()).Returns(mockPing.Object);

		typeof(Echo).GetField("innerPool", BindingFlags.NonPublic | BindingFlags.Instance)!
			.SetValue(echo, mockPool.Object);

		return mockPool;
	}

	private static Mock<HudManager> SetupHudManagerMock()
	{
		if (MockVector3get_oneHelper.Instance == null)
		{
			var mockOne = new Mock<MockVector3get_oneHelper>();
			mockOne.Setup(x => x.Invoke()).Returns(new Vector3(1f, 1f, 1f));
			MockVector3get_oneHelper.Instance = mockOne.Object;
		}

		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));

		var mockGridArrange = new Mock<GridArrange>(IntPtr.Zero);
		mockGridArrange.Setup(g => g.ArrangeChilds());

		var mockParentGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockParentGameObject.Setup(g => g.GetComponent<GridArrange>()).Returns(mockGridArrange.Object);

		var mockParentTransform = new Mock<Transform>(IntPtr.Zero);
		mockParentTransform.SetupGet(t => t.gameObject).Returns(mockParentGameObject.Object);

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockTransform.SetupGet(t => t.parent).Returns(mockParentTransform.Object);
		mockTransform.Setup(t => t.FindChild(It.IsAny<string>())).Returns((Transform)null!);

		var mockMaterial = new Mock<Material>(IntPtr.Zero);
		mockMaterial.Setup(m => m.SetFloat(It.IsAny<string>(), It.IsAny<float>()));

		var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockSpriteRenderer.SetupProperty(s => s.sprite);
		mockSpriteRenderer.SetupProperty(s => s.color);
		mockSpriteRenderer.SetupProperty(s => s.enabled);
		mockSpriteRenderer.SetupGet(s => s.material).Returns(mockMaterial.Object);

		var mockLabelText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockLabelText.SetupProperty(t => t.color);
		mockLabelText.SetupProperty(t => t.fontMaterial);
		mockLabelText.SetupProperty(t => t.text);
		mockLabelText.SetupGet(t => t.transform).Returns(mockTransform.Object);
		mockLabelText.SetupGet(t => t.gameObject).Returns(mockGameObject.Object);

		var mockCoolText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockCoolText.SetupProperty(t => t.color);
		mockCoolText.SetupProperty(t => t.enableWordWrapping);
		mockCoolText.SetupProperty(t => t.text);
		mockCoolText.SetupGet(t => t.gameObject).Returns(mockGameObject.Object);
		mockCoolText.SetupGet(t => t.transform).Returns(mockTransform.Object);

		var mockPersistentCallGroup = new Mock<PersistentCallGroup>(IntPtr.Zero);
		mockPersistentCallGroup.Setup(p => p.Clear());

		var mockOnClick = new Mock<UnityEngine.UI.Button.ButtonClickedEvent>(IntPtr.Zero);
		mockOnClick.Setup(e => e.RemoveAllListeners());
		mockOnClick.Setup(e => e.AddListener(It.IsAny<UnityAction>()));
		mockOnClick.SetupGet(e => e.m_PersistentCalls).Returns(mockPersistentCallGroup.Object);

		var mockPassiveButton = new Mock<PassiveButton>(IntPtr.Zero);
		mockPassiveButton.SetupGet(b => b.OnClick).Returns(mockOnClick.Object);

		var mockKillButton = new Mock<KillButton>(IntPtr.Zero);
		mockKillButton.SetupGet(b => b.transform).Returns(mockTransform.Object);
		mockKillButton.SetupGet(b => b.gameObject).Returns(mockGameObject.Object);
		mockKillButton.SetupGet(b => b.graphic).Returns(mockSpriteRenderer.Object);
		mockKillButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
		mockKillButton.SetupGet(b => b.cooldownTimerText).Returns(mockCoolText.Object);
		mockKillButton.Setup(b => b.GetComponent<PassiveButton>()).Returns(mockPassiveButton.Object);
		mockKillButton.SetupGet(b => b.isActiveAndEnabled).Returns(true);

		var mockUseButton = new Mock<UseButton>(IntPtr.Zero);
		mockUseButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
		mockUseButton.SetupGet(b => b.transform).Returns(mockTransform.Object);

		var mockHud = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		mockHud.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
		mockHud.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);

		var mockTaskPanel = new Mock<TaskPanelBehaviour>(IntPtr.Zero);
		var mockTextMesh = new Mock<TextMeshPro>(IntPtr.Zero);
		mockTaskPanel.SetupGet(t => t.taskText).Returns(mockTextMesh.Object);
		mockHud.SetupGet(h => h.TaskPanel).Returns(mockTaskPanel.Object);

		var mockInstantiate = new Mock<MockObjectInstantiateHelper>();
		mockInstantiate.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Vector3>(), It.IsAny<Quaternion>()))
			.Returns((UnityEngine.Object original, Vector3 pos, Quaternion rot) => original);
		MockObjectInstantiateHelper.Instance = mockInstantiate.Object;

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<Il2CppSystem.Object>(), It.IsAny<Transform>()))
			.Returns((Il2CppSystem.Object original, Transform parent) => original);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		return mockHud;
	}

	private static Mock<PlayerControl> CreateMockPlayer(byte id, Vector2 position)
	{
		IntPtr fakePlayerPtr = Marshal.AllocHGlobal(1024);
		Marshal.Copy(new byte[1024], 0, fakePlayerPtr, 1024);

		IntPtr fakeDataPtr = Marshal.AllocHGlobal(1024);
		Marshal.Copy(new byte[1024], 0, fakeDataPtr, 1024);

		var playerIdField = typeof(InnerNetObject).GetField("NativeFieldInfoPtr_PlayerId", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
			?? typeof(PlayerControl).GetField("NativeFieldInfoPtr_PlayerId", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

		if (playerIdField != null)
		{
			IntPtr fieldPtr = (IntPtr)playerIdField.GetValue(null)!;
			if (fieldPtr != IntPtr.Zero)
			{
				int offset = (int)IL2CPP.il2cpp_field_get_offset(fieldPtr);
				Marshal.WriteInt32(fakePlayerPtr + offset, id);
				Marshal.WriteByte(fakePlayerPtr + offset, id);
			}
		}

		var mockPlayer = new Mock<PlayerControl>(fakePlayerPtr);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(id);

		MockPointerPositions[fakePlayerPtr] = position;

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockTransform.SetupGet(t => t.position).Returns(new Vector3(position.x, position.y, 0f));
		mockPlayer.SetupGet(p => p.transform).Returns(mockTransform.Object);

		var mockData = new Mock<NetworkedPlayerInfo>(fakeDataPtr);
		mockData.SetupGet(d => d.PlayerId).Returns(id);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);

		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		return mockPlayer;
	}

	[Fact]
	public void CreateRoleAllOption_CreatesExpectedOptions()
	{
		// Arrange
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Echo);
		var role = new Echo();

		// Act
		role.CreateRoleAllOption();

		// Assert
		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);
	}

	[Theory]
	[InlineData(10.0f, 2.0f, Echo.EmitAttentionMode.EmitAll, true, true)]
	[InlineData(15.0f, 3.0f, Echo.EmitAttentionMode.EmitNotCrewmate, false, false)]
	[InlineData(5.0f, 1.0f, Echo.EmitAttentionMode.DisableKey, true, false)]
	public void RoleSpecificInit_SetsExpectedFields(
		float range,
		float showTime,
		Echo.EmitAttentionMode attentionMode,
		bool isDetectDeadBody,
		bool canSeparatePlayer)
	{
		// Arrange
		var role = new Echo();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Echo.Option.Range, out var rangeOpt) && rangeOpt != null)
		{
			// Range: min=5.0, step=0.5
			rangeOpt.Selection = (int)((range - 5.0f) / 0.5f);
		}

		if (role.Loader.TryGet(Echo.Option.ShowTime, out var showTimeOpt) && showTimeOpt != null)
		{
			// ShowTime: min=0.5, step=0.25
			showTimeOpt.Selection = (int)((showTime - 0.5f) / 0.25f);
		}

		if (role.Loader.TryGet(Echo.Option.AttentionMode, out var attentionOpt) && attentionOpt != null)
		{
			attentionOpt.Selection = (int)attentionMode;
		}

		if (role.Loader.TryGet(Echo.Option.IsDetectDeadBody, out var deadBodyOpt) && deadBodyOpt != null)
		{
			deadBodyOpt.Selection = isDetectDeadBody ? 1 : 0;
		}

		if (role.Loader.TryGet(Echo.Option.CanSeparatePlayer, out var separateOpt) && separateOpt != null)
		{
			separateOpt.Selection = canSeparatePlayer ? 1 : 0;
		}

		// Act
		role.Initialize();

		// Assert
		float expectedSqrRange = range * range;
		float rangeSqrVal = (float)typeof(Echo).GetField("echoLocationRangeSquare", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(role)!;
		float pingTimeVal = (float)typeof(Echo).GetField("pingTime", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(role)!;
		bool deadBodyVal = (bool)typeof(Echo).GetField("isDetectDeadBody", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(role)!;
		bool separateVal = (bool)typeof(Echo).GetField("canSeparatePlayer", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(role)!;
		var modeVal = (Echo.EmitAttentionMode)typeof(Echo).GetField("mode", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(role)!;

		Assert.Equal(expectedSqrRange, rangeSqrVal);
		Assert.Equal(showTime, pingTimeVal);
		Assert.Equal(isDetectDeadBody, deadBodyVal);
		Assert.Equal(canSeparatePlayer, separateVal);
		Assert.Equal(attentionMode, modeVal);
	}

	[Fact]
	public void CreateAbility_InitializesButton()
	{
		// Arrange
		SetupHudManagerMock();
		var role = new Echo();

		// Act
		role.CreateAbility();

		// Assert
		Assert.NotNull(role.Button);
	}

	[Fact]
	public void IsAbilityUse_ReturnsCommonUseValue()
	{
		// Arrange
		var role = new Echo();

		// Act
		bool result = role.IsAbilityUse();

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void ResetOnMeetingStart_WhenModeNotDisableKey_ResetsAndClearsCoroutines()
	{
		// Arrange
		SetupHudManagerMock();

		var role = new Echo();
		role.CreateRoleAllOption();
		role.Initialize();

		SetupMockPool(role);

		var coroutinesList = (List<Coroutine?>)typeof(Echo).GetField("coroutine", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(role)!;
		var dummyCoroutine = (Coroutine)RuntimeHelpers.GetUninitializedObject(typeof(Coroutine));
		coroutinesList.Add(dummyCoroutine);

		// Act
		role.ResetOnMeetingStart();

		// Assert
		Assert.Empty(coroutinesList);
	}

	[Fact]
	public void ResetOnMeetingStart_WhenModeIsDisableKey_ResetsAndClearsCoroutines()
	{
		// Arrange
		SetupHudManagerMock();

		var role = new Echo();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Echo.Option.AttentionMode, out var attentionOpt) && attentionOpt != null)
		{
			attentionOpt.Selection = (int)Echo.EmitAttentionMode.DisableKey;
		}

		role.Initialize();
		SetupMockPool(role);

		var coroutinesList = (List<Coroutine?>)typeof(Echo).GetField("coroutine", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(role)!;
		var dummyCoroutine = (Coroutine)RuntimeHelpers.GetUninitializedObject(typeof(Coroutine));
		coroutinesList.Add(dummyCoroutine);

		// Act
		role.ResetOnMeetingStart();

		// Assert
		Assert.Empty(coroutinesList);
	}

	[Fact]
	public void ResetOnMeetingEnd_DoesNotThrow()
	{
		// Arrange
		var role = new Echo();

		// Act & Assert
		role.ResetOnMeetingEnd(null);
	}

	[Fact]
	public void UseAbility_WhenModeNotDisableKey_StartsCoroutineAndReturnsTrue()
	{
		// Arrange
		SetupHudManagerMock();

		var role = new Echo();
		role.CreateRoleAllOption();
		role.Initialize();

		SetupMockPool(role);

		// Act
		bool result = role.UseAbility();

		// Assert
		Assert.True(result);
		Assert.Equal(1, StartCoroutineCallCount);
	}

	[Fact]
	public void UseAbility_WhenModeIsDisableKey_StartsCoroutineAndReturnsTrue()
	{
		// Arrange
		SetupHudManagerMock();

		var role = new Echo();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Echo.Option.AttentionMode, out var attentionOpt) && attentionOpt != null)
		{
			attentionOpt.Selection = (int)Echo.EmitAttentionMode.DisableKey;
		}

		role.Initialize();
		SetupMockPool(role);

		// Act
		bool result = role.UseAbility();

		// Assert
		Assert.True(result);
		Assert.Equal(1, StartCoroutineCallCount);
	}

	[Fact]
	public void Rpc_WhenPlayerNotFound_ReturnsEarly()
	{
		// Arrange
		byte playerId = 99;
		ExtremeRoleManager.GameRole.Clear();

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns(playerId)
			.Returns((byte)Echo.RpcOps.Emit);

		var readerObj = mockReader.Object;

		// Act & Assert
		Echo.Rpc(in readerObj);
	}

	[Fact]
	public void Rpc_Reset_CallsReset()
	{
		// Arrange
		SetupHudManagerMock();

		byte playerId = 1;
		var role = new Echo();
		role.CreateRoleAllOption();
		role.Initialize();

		SetupMockPool(role);

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[playerId] = role;

		var coroutinesList = (List<Coroutine?>)typeof(Echo).GetField("coroutine", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(role)!;
		var dummyCoroutine = (Coroutine)RuntimeHelpers.GetUninitializedObject(typeof(Coroutine));
		coroutinesList.Add(dummyCoroutine);

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns(playerId)
			.Returns((byte)Echo.RpcOps.Reset);

		var readerObj = mockReader.Object;

		// Act
		Echo.Rpc(in readerObj);

		// Assert
		Assert.Empty(coroutinesList);
	}

	[Theory]
	[InlineData(Echo.EmitAttentionMode.EmitAll, true, true)]
	[InlineData(Echo.EmitAttentionMode.EmitNotCrewmate, true, false)] // Crewmate local player role -> EmitNotCrewmate returns false
	[InlineData(Echo.EmitAttentionMode.DisableKey, true, false)]
	public void Rpc_Emit_HandlesShowEchoEmitPosCorrectly(
		Echo.EmitAttentionMode mode,
		bool isLocalPlayerCrewmate,
		bool expectedShowPing)
	{
		// Arrange
		SetupHudManagerMock();

		byte echoPlayerId = 2;
		byte localPlayerId = 1;

		var echoRole = new Echo();
		echoRole.CreateRoleAllOption();

		if (echoRole.Loader.TryGet(Echo.Option.AttentionMode, out var attentionOpt) && attentionOpt != null)
		{
			attentionOpt.Selection = (int)mode;
		}

		echoRole.Initialize();
		SetupMockPool(echoRole);

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[echoPlayerId] = echoRole;

		if (isLocalPlayerCrewmate)
		{
			var localRole = new SpecialCrew();
			localRole.CreateRoleAllOption();
			localRole.Initialize();
			ExtremeRoleManager.GameRole[localPlayerId] = localRole;
		}
		else
		{
			var localRole = new SpecialImpostor();
			localRole.CreateRoleAllOption();
			localRole.Initialize();
			ExtremeRoleManager.GameRole[localPlayerId] = localRole;
		}

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns(echoPlayerId)
			.Returns((byte)Echo.RpcOps.Emit);

		mockReader.SetupSequence(r => r.ReadSingle())
			.Returns(5.0f)
			.Returns(10.0f);

		var readerObj = mockReader.Object;

		// Act
		Echo.Rpc(in readerObj);

		// Assert
		int expectedCallCount = expectedShowPing ? 1 : 0;
		Assert.Equal(expectedCallCount, StartCoroutineCallCount);
	}

	[Fact]
	public void EmitEchoLocation_WhenNoOtherPlayersOrDeadBodiesInRange_YieldsBreakImmediately()
	{
		// Arrange
		SetupHudManagerMock();

		MockPointerPositions.Clear();
		PlayerCache.RemovePlayerControl(p => true);
		PlayerCache.AddPlayerControl(this.localPlayerMock.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<DeadBody>()).Returns(new Il2CppReferenceArray<DeadBody>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var role = new Echo();
		role.CreateRoleAllOption();
		role.Initialize();
		SetupMockPool(role);

		var emitMethod = typeof(Echo).GetMethod("emitEchoLocation", BindingFlags.NonPublic | BindingFlags.Instance)!;

		// Act
		var enumerator = (IEnumerator)emitMethod.Invoke(role, null)!;

		// Assert
		Assert.False(enumerator.MoveNext());
	}

	[Fact]
	public void AddPlayerLocationInfo_FiltersAndSortsPlayersByDistanceCorrectly()
	{
		// Arrange
		MockPointerPositions.Clear();

		var sourcePlayer = this.localPlayerMock;
		MockPointerPositions[sourcePlayer.Object.Pointer] = new Vector2(0f, 0f);

		var playerNear = CreateMockPlayer(20, new Vector2(2f, 0f)); // sqrDist = 4
		var playerFar = CreateMockPlayer(30, new Vector2(6f, 0f));  // sqrDist = 36
		var playerOut = CreateMockPlayer(40, new Vector2(50f, 0f)); // sqrDist = 2500 > 100

		var role = new Echo();
		role.CreateRoleAllOption();
		role.Initialize();

		PlayerCache.RemovePlayerControl(p => true);
		PlayerCache.AddPlayerControl(sourcePlayer.Object);
		PlayerCache.AddPlayerControl(playerNear.Object);
		PlayerCache.AddPlayerControl(playerFar.Object);
		PlayerCache.AddPlayerControl(playerOut.Object);

		typeof(Echo).GetField("echoLocationRangeSquare", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(role, 100.0f);

		var addPlayerInfoMethod = typeof(Echo).GetMethod("addPlayerLocationInfo", BindingFlags.NonPublic | BindingFlags.Instance)!;

		var locationInfoType = typeof(Echo).GetNestedType("LocationInfo", BindingFlags.NonPublic)!;
		var listType = typeof(List<>).MakeGenericType(locationInfoType);
		var resultList = Activator.CreateInstance(listType)!;

		// Act
		addPlayerInfoMethod.Invoke(role, new object[] { sourcePlayer.Object, resultList });

		// Assert
		int count = (int)listType.GetProperty("Count")!.GetValue(resultList)!;
		Assert.Equal(2, count); // playerNear and playerFar

		var item0 = listType.GetProperty("Item")!.GetValue(resultList, new object[] { 0 });
		var item1 = listType.GetProperty("Item")!.GetValue(resultList, new object[] { 1 });

		var pos0 = (Vector3)locationInfoType.GetProperty("Pos")!.GetValue(item0)!;
		var pos1 = (Vector3)locationInfoType.GetProperty("Pos")!.GetValue(item1)!;

		float normDist0 = (float)locationInfoType.GetProperty("NormedDistance")!.GetValue(item0)!;
		float normDist1 = (float)locationInfoType.GetProperty("NormedDistance")!.GetValue(item1)!;

		Assert.Equal(2f, pos0.x);
		Assert.Equal(6f, pos1.x);
		Assert.Equal(0.04f, normDist0, 2); // 4 / 100 = 0.04
		Assert.Equal(0.36f, normDist1, 2); // 36 / 100 = 0.36
	}

	[Fact]
	public void EmitEchoLocation_WithDeadBodies_ProcessesAllStepsAndCleansUp()
	{
		// Arrange
		SetupHudManagerMock();

		MockPointerPositions.Clear();
		PlayerCache.RemovePlayerControl(p => true);
		PlayerCache.AddPlayerControl(this.localPlayerMock.Object);

		var mockBodyTransform = new Mock<Transform>(IntPtr.Zero);
		mockBodyTransform.SetupGet(t => t.position).Returns(new Vector3(3f, 0f, 0f)); // sqrDist = 9 <= 100

		var mockDeadBody = new Mock<DeadBody>(IntPtr.Zero);
		mockDeadBody.SetupGet(b => b.transform).Returns(mockBodyTransform.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<DeadBody>()).Returns(new Il2CppReferenceArray<DeadBody>([mockDeadBody.Object]));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var role = new Echo();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Echo.Option.Range, out var rangeOpt) && rangeOpt != null)
		{
			rangeOpt.Selection = (int)((10.0f - 5.0f) / 0.5f); // Range = 10 -> sqrRange = 100
		}

		if (role.Loader.TryGet(Echo.Option.IsDetectDeadBody, out var deadBodyOpt) && deadBodyOpt != null)
		{
			deadBodyOpt.Selection = 1; // true
		}

		role.Initialize();
		var mockPool = SetupMockPool(role);

		var emitMethod = typeof(Echo).GetMethod("emitEchoLocation", BindingFlags.NonPublic | BindingFlags.Instance)!;

		// Act
		var enumerator = (IEnumerator)emitMethod.Invoke(role, null)!;
		int stepCount = 0;
		while (enumerator.MoveNext())
		{
			stepCount++;
		}

		// Assert
		Assert.True(stepCount > 0);
		mockPool.Verify(p => p.Get<PingBehaviour>(), Times.Once());
	}

	[Fact]
	public void AddDeadBodyLocationInfo_DetectsDeadBodiesWhenEnabled()
	{
		// Arrange
		Vector3 sourcePos = new Vector3(0f, 0f, 0f);

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockTransform.SetupGet(t => t.position).Returns(new Vector3(4f, 0f, 0f)); // sqrDist = 16

		var mockBody = new Mock<DeadBody>(IntPtr.Zero);
		mockBody.SetupGet(b => b.transform).Returns(mockTransform.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<DeadBody>()).Returns(new Il2CppReferenceArray<DeadBody>([mockBody.Object]));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var role = new Echo();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Echo.Option.Range, out var rangeOpt) && rangeOpt != null)
		{
			rangeOpt.Selection = (int)((10.0f - 5.0f) / 0.5f); // Range = 10
		}

		if (role.Loader.TryGet(Echo.Option.IsDetectDeadBody, out var deadBodyOpt) && deadBodyOpt != null)
		{
			deadBodyOpt.Selection = 1; // true
		}

		if (role.Loader.TryGet(Echo.Option.CanSeparatePlayer, out var separateOpt) && separateOpt != null)
		{
			separateOpt.Selection = 1; // true
		}

		role.Initialize();

		var addDeadBodyInfoMethod = typeof(Echo).GetMethod("addDeadBodyLocationInfo", BindingFlags.NonPublic | BindingFlags.Instance)!;

		var locationInfoType = typeof(Echo).GetNestedType("LocationInfo", BindingFlags.NonPublic)!;
		var listType = typeof(List<>).MakeGenericType(locationInfoType);
		var resultList = Activator.CreateInstance(listType)!;

		// Act
		addDeadBodyInfoMethod.Invoke(role, new object[] { sourcePos, resultList });

		// Assert
		int count = (int)listType.GetProperty("Count")!.GetValue(resultList)!;
		Assert.Equal(1, count);

		var item = listType.GetProperty("Item")!.GetValue(resultList, new object[] { 0 });
		bool isDeadbody = (bool)locationInfoType.GetProperty("IsDeadbody")!.GetValue(item)!;

		Assert.True(isDeadbody);
	}

	[Fact]
	public void SetUpPing_WhenIsDeadbodyTrue_LeavesDefaultImageColor()
	{
		// Arrange
		var role = new Echo();
		var mockPool = SetupMockPool(role);

		var locationInfoType = typeof(Echo).GetNestedType("LocationInfo", BindingFlags.NonPublic)!;
		var locationInfo = Activator.CreateInstance(locationInfoType, new object[] { new Vector3(5f, 5f, 0f), true /* IsDeadbody */, 0.5f })!;

		var setUpPingMethod = typeof(Echo).GetMethod("setUpPing", BindingFlags.NonPublic | BindingFlags.Instance)!;

		// Act
		var pingResult = (PingBehaviour)setUpPingMethod.Invoke(role, new object[] { mockPool.Object, locationInfo })!;

		// Assert
		Assert.NotNull(pingResult);
		Assert.NotNull(pingResult.image);
	}

	[Fact]
	public void SetUpPing_And_HidePing_ConfiguresAndResetsPingBehaviour()
	{
		// Arrange
		var role = new Echo();
		var mockPool = SetupMockPool(role);

		var mockImageRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockImageRenderer.SetupProperty(r => r.sortingOrder);
		mockImageRenderer.SetupProperty(r => r.color);

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockTransform.SetupProperty(t => t.position);

		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));

		var mockPing = new Mock<PingBehaviour>(IntPtr.Zero);
		mockPing.SetupGet(p => p.transform).Returns(mockTransform.Object);
		mockPing.SetupGet(p => p.gameObject).Returns(mockGameObject.Object);
		mockPing.SetupProperty(p => p.target);
		mockPing.SetupProperty(p => p.AmSeeker);
		mockPing.SetupProperty(p => p.MaxScale);
		mockPing.SetupGet(p => p.image).Returns(mockImageRenderer.Object);
		mockPing.Setup(p => p.SetImageEnabled(It.IsAny<bool>()));
		mockPing.Setup(p => p.UpdatePosition());

		mockPool.Setup(p => p.Get<PingBehaviour>()).Returns(mockPing.Object);

		var locationInfoType = typeof(Echo).GetNestedType("LocationInfo", BindingFlags.NonPublic)!;
		var locationInfo = Activator.CreateInstance(locationInfoType, new object[] { new Vector3(5f, 5f, 0f), false, 0.5f })!;

		var setUpPingMethod = typeof(Echo).GetMethod("setUpPing", BindingFlags.NonPublic | BindingFlags.Instance)!;
		var hidePingMethod = typeof(Echo).GetMethod("hidePing", BindingFlags.NonPublic | BindingFlags.Static)!;

		// Act 1: setUpPing
		var pingResult = (PingBehaviour)setUpPingMethod.Invoke(role, new object[] { mockPool.Object, locationInfo })!;

		// Assert 1: setUpPing
		Assert.Same(mockPing.Object, pingResult);
		Assert.Equal(new Vector3(5f, 5f, 0f), mockPing.Object.target);
		Assert.Equal(0.54f, mockPing.Object.MaxScale, 2); // 0.9 * ((1.0 - 0.5) * 0.8 + 0.2) = 0.9 * 0.6 = 0.54
		mockPing.Verify(p => p.SetImageEnabled(true), Times.Once());
		mockGameObject.Verify(g => g.SetActive(true), Times.Once());

		// Act 2: hidePing
		hidePingMethod.Invoke(null, new object[] { mockPing.Object });

		// Assert 2: hidePing
		Assert.Equal(Vector3.zero, mockPing.Object.target);
		mockPing.Verify(p => p.SetImageEnabled(false), Times.Once());
		mockGameObject.Verify(g => g.SetActive(false), Times.Once());
	}

	[Fact]
	public void WaitNextTarget_WhenFirstPingExpired_DequeuesAndYields()
	{
		// Arrange
		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);

		var mockPing = new Mock<PingBehaviour>(IntPtr.Zero);
		mockPing.SetupGet(p => p.transform).Returns(mockTransform.Object);
		mockPing.SetupGet(p => p.gameObject).Returns(mockGameObject.Object);

		var pingInfoType = typeof(Echo).GetNestedType("PingInfo", BindingFlags.NonPublic)!;
		var pingInfo = Activator.CreateInstance(pingInfoType, new object[] { mockPing.Object, 0.1f })!;

		var queueType = typeof(Queue<>).MakeGenericType(pingInfoType);
		var queue = Activator.CreateInstance(queueType)!;
		queueType.GetMethod("Enqueue")!.Invoke(queue, new object[] { pingInfo });

		var locationInfoType = typeof(Echo).GetNestedType("LocationInfo", BindingFlags.NonPublic)!;
		var nextLocation = Activator.CreateInstance(locationInfoType, new object[] { new Vector3(1f, 0f, 0f), false, 0.5f })!;

		var waitNextTargetMethod = typeof(Echo).GetMethod("waitNextTarget", BindingFlags.NonPublic | BindingFlags.Static)!;

		// Act
		var enumerator = (IEnumerator)waitNextTargetMethod.Invoke(null, new object[] { nextLocation, queue })!;

		int steps = 0;
		while (enumerator.MoveNext())
		{
			steps++;
		}

		// Assert
		Assert.True(steps >= 2); // 1 for removePingWaitTime yield, 1 for lastWaitTime yield
	}
}
