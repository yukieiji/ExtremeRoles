using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor.Nullifier;

using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using MonoMod.RuntimeDetour;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class NullifierTests : IDisposable
{
	private sealed class MockUnityObjectFactory : IUnityObjectFactory
	{
		public readonly List<GameObject> CreatedGameObjects = [];

		public GameObject CreateGameObject(string name)
		{
			var mockGo = new Mock<GameObject>(IntPtr.Zero);
			var mockTrans = new Mock<Transform>(IntPtr.Zero);
			var mockSr = new Mock<SpriteRenderer>(IntPtr.Zero);

			mockGo.SetupGet(g => g.transform).Returns(mockTrans.Object);
			mockSr.SetupProperty(s => s.sprite);
			mockSr.SetupProperty(s => s.color, Color.white);
			mockGo.Setup(g => g.AddComponent<SpriteRenderer>()).Returns(mockSr.Object);
			mockGo.Setup(g => g.AddComponent(It.IsAny<Il2CppSystem.Type>())).Returns(mockSr.Object);

			this.CreatedGameObjects.Add(mockGo.Object);
			return mockGo.Object;
		}

		public Sprite LoadRoleSprite(ExtremeRoleId roleId) => new Mock<Sprite>(IntPtr.Zero).Object;
		public Sprite LoadRoleSprite(ExtremeRoleId roleId, string img) => new Mock<Sprite>(IntPtr.Zero).Object;
		public Material CreateSpriteMaterial() => new Mock<Material>(IntPtr.Zero).Object;
		public Vector3 CreateMapPos(Vector2 target, float offset = 1000.0f) => new Vector3(target.x, target.y, target.y / offset);
		public Mesh CreateMesh() => new Mock<Mesh>(IntPtr.Zero).Object;
	}

	private delegate ExtremeConsole CreateConsoleObjDelegate(ExtremeConsoleSystem self, Vector3 pos, string? name);
	private delegate GameObject ComponentGetGameObjectDelegate(ComponentOrig orig, Component self);
	private delegate GameObject ComponentOrig(Component self);
	private delegate Transform ComponentGetTransformDelegate(ComponentTransOrig orig, Component self);
	private delegate Transform ComponentTransOrig(Component self);
	private delegate Transform GameObjectGetTransformDelegate(GameObjectTransOrig orig, GameObject self);
	private delegate Transform GameObjectTransOrig(GameObject self);

	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;
	private readonly MockUnityObjectFactory mockFactory;
	private readonly Hook createConsoleObjHook;
	private readonly Hook compGetGameObjectHook;
	private readonly Hook compGetTransformHook;
	private readonly Hook goGetTransformHook;
	private static readonly Dictionary<IntPtr, (GameObject go, Transform trans)> consoleObjects = [];

	public NullifierTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupUnityObjectOperators();
		MockSetupHelper.SetupVector3Helpers();
		MockSetupHelper.SetupPaletteHelpers();

		var cachedPtrField = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);

		MockPlayerControlget_LocalPlayerHelper.Instance = null!;
		this.mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		cachedPtrField?.SetValue(this.mockLocalPlayer.Object, (IntPtr)1);
		this.mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)0);
		this.mockLocalPlayer.SetupGet(p => p.NetId).Returns((uint)1);
		this.mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);

		var mockPlayerTransform = new Mock<Transform>(IntPtr.Zero);
		this.mockLocalPlayer.SetupGet(p => p.transform).Returns(mockPlayerTransform.Object);
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));

		var mockLocalData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockLocalData.Object, (IntPtr)1);
		mockLocalData.SetupGet(d => d.PlayerId).Returns((byte)0);
		mockLocalData.SetupGet(d => d.IsDead).Returns(false);
		mockLocalData.SetupGet(d => d.Disconnected).Returns(false);
		mockLocalData.SetupGet(d => d.Object).Returns(this.mockLocalPlayer.Object);
		this.mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData.Object);

		PlayerCache.RemovePlayerControl(_ => true);
		PlayerCache.AddPlayerControl(this.mockLocalPlayer.Object);

		this.mockFactory = new MockUnityObjectFactory();

		var system = new NullifierSystem(
			2.5f,
			10,
			10.0f,
			NullifierTargetTeam.AllImpostorOther,
			this.mockFactory);
		ExtremeSystemTypeManager.Instance.TryAdd(ExtremeSystemType.NullifierSystem, system);

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		if (ExtremeGameModeManager.Instance == null)
		{
			ExtremeGameModeManager.Create(GameModes.Normal);
		}

		MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		SetupAssetBundleMock();

		this.mockClient = MockSetupHelper.SetupAmongUsClientMock();
		this.mockClient.SetupGet(c => c.AmHost).Returns(true);

		var writtenBytes = new List<byte>();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockWriter.Setup(w => w.Write(It.IsAny<byte>())).Callback((byte b) => writtenBytes.Add(b));
		mockWriter.Setup(w => w.ToByteArray(It.IsAny<bool>())).Returns(new Mock<Il2CppStructArray<byte>>(IntPtr.Zero).Object);

		this.mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockGet = new Mock<Hazel.MockMessageWriterGetHelper>();
		mockGet.Setup(g => g.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
		Hazel.MockMessageWriterGetHelper.Instance = mockGet.Object;

		var mockReaderHelper = new Mock<Hazel.MockMessageReaderGetHelper>();
		mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppStructArray<byte>>()))
			.Returns(() =>
			{
				var mockReader = new Mock<MessageReader>();
				if (writtenBytes.Count > 0)
				{
					var bytesCopy = new List<byte>(writtenBytes);
					writtenBytes.Clear();
					var seqByte = mockReader.SetupSequence(r => r.ReadByte());
					foreach (var b in bytesCopy)
					{
						seqByte.Returns(b);
					}
					var seqSingle = mockReader.SetupSequence(r => r.ReadSingle());
					seqSingle.Returns(0f).Returns(0f);
				}
				return mockReader.Object;
			});
		Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;

		var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
		mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNetObject>()));
		InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		var mockExileControllerHelper = new Mock<MockExileControllerget_InstanceHelper>();
		mockExileControllerHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockExileControllerget_InstanceHelper.Instance = mockExileControllerHelper.Object;

		if (ExtremeRolesPlugin.ShipState == null)
		{
			var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(
				nameof(ExtremeRolesPlugin.ShipState),
				BindingFlags.Public | BindingFlags.Static);
			shipStateProp?.SetValue(null, new ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus());
		}

		var compGetGameObjectMethod = typeof(Component).GetProperty(nameof(Component.gameObject))!.GetGetMethod()!;
		var compGetTransformMethod = typeof(Component).GetProperty(nameof(Component.transform))!.GetGetMethod()!;

		this.compGetGameObjectHook = new Hook(compGetGameObjectMethod, new ComponentGetGameObjectDelegate((orig, self) =>
		{
			if (self != null && cachedPtrField != null)
			{
				var val = cachedPtrField.GetValue(self);
				if (val is IntPtr ptr && consoleObjects.TryGetValue(ptr, out var tuple))
				{
					return tuple.go;
				}
			}
			foreach (var kv in consoleObjects)
			{
				return kv.Value.go;
			}
			return null!;
		}));

		this.compGetTransformHook = new Hook(compGetTransformMethod, new ComponentGetTransformDelegate((orig, self) =>
		{
			if (self != null && cachedPtrField != null)
			{
				var val = cachedPtrField.GetValue(self);
				if (val is IntPtr ptr && consoleObjects.TryGetValue(ptr, out var tuple))
				{
					return tuple.trans;
				}
			}
			return null!;
		}));

		var goGetTransformMethod = typeof(GameObject).GetProperty(nameof(GameObject.transform))!.GetGetMethod()!;
		this.goGetTransformHook = new Hook(goGetTransformMethod, new GameObjectGetTransformDelegate((orig, self) =>
		{
			if (self != null && cachedPtrField != null)
			{
				var val = cachedPtrField.GetValue(self);
				if (val is IntPtr ptr && consoleObjects.TryGetValue(ptr, out var tuple))
				{
					return tuple.trans;
				}
			}
			return null!;
		}));

		var targetMethod = typeof(ExtremeConsoleSystem).GetMethod(
			nameof(ExtremeConsoleSystem.CreateConsoleObj),
			new[] { typeof(Vector3), typeof(string) })!;
		var hookDelegate = new CreateConsoleObjDelegate((self, pos, name) =>
		{
			var mockGo = new Mock<GameObject>(IntPtr.Zero);
			var mockTrans = new Mock<Transform>(IntPtr.Zero);
			mockTrans.SetupGet(t => t.position).Returns(pos);
			mockTrans.SetupProperty(t => t.localScale);
			mockGo.SetupGet(g => g.transform).Returns(mockTrans.Object);

			var console = (ExtremeConsole)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(ExtremeConsole));
			var ptr = (IntPtr)100;
			cachedPtrField?.SetValue(console, ptr);
			cachedPtrField?.SetValue(mockGo.Object, ptr);
			consoleObjects[ptr] = (mockGo.Object, mockTrans.Object);

			var mockCollider = new Mock<CircleCollider2D>(IntPtr.Zero);
			mockCollider.SetupProperty(c => c.isTrigger);
			mockCollider.SetupProperty(c => c.radius);
			mockGo.Setup(g => g.AddComponent<CircleCollider2D>()).Returns(mockCollider.Object);

			return console;
		});
		this.createConsoleObjHook = new Hook(targetMethod, hookDelegate);
	}

	public void Dispose()
	{
		this.createConsoleObjHook?.Dispose();
		this.compGetGameObjectHook?.Dispose();
		this.compGetTransformHook?.Dispose();
		this.goGetTransformHook?.Dispose();
		consoleObjects.Clear();
	}

	private static void SetupAssetBundleMock()
	{
		var mockSprite = new Mock<Sprite>(IntPtr.Zero);

		var mockAssetBundle = new Mock<AssetBundle>(IntPtr.Zero);
		mockAssetBundle.Setup(b => b.LoadAsset(It.IsAny<string>(), It.IsAny<Il2CppSystem.Type>()))
			.Returns(mockSprite.Object);

		var field = typeof(UnityObjectLoader).GetField("cachedBundle", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
		if (field?.GetValue(null) is Dictionary<string, AssetBundle> dict)
		{
			dict[ObjectPath.GetRoleAssetPath(ExtremeRoleId.Nullifier)] = mockAssetBundle.Object;
		}
	}

	private static void InitializeRole(SingleRoleBase role, byte playerId = 0)
	{
		role.CreateRoleAllOption();
		role.Initialize();
		ExtremeRoleManager.GameRole[playerId] = role;
	}

	private static void AddSealForTest(NullifierSystem system, int id, Vector2 pos)
	{
		var mockText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		var mockTextGo = new Mock<GameObject>(IntPtr.Zero);
		mockText.SetupGet(t => t.gameObject).Returns(mockTextGo.Object);
		mockText.SetupProperty(t => t.text);

		var behavior = new NullifierSystem.NullifierConsoleBehavior(id, 10, mockText.Object);

		var mockGo = new Mock<GameObject>(IntPtr.Zero);
		var mockTrans = new Mock<Transform>(IntPtr.Zero);
		mockTrans.SetupGet(t => t.position).Returns(new Vector3(pos.x, pos.y, 0f));
		mockGo.SetupGet(g => g.transform).Returns(mockTrans.Object);

		var sealCircle = new NullifierSystem.SealCircle(behavior, mockGo.Object);

		var placedSealsField = typeof(NullifierSystem).GetField("placedSeals", BindingFlags.NonPublic | BindingFlags.Instance);
		var placedSeals = (IDictionary)placedSealsField?.GetValue(system)!;
		placedSeals[id] = sealCircle;
	}

	[Fact]
	public void Initialize_RegistersNullifierRoleAndImpostorTeam()
	{
		var role = new NullifierRole();
		InitializeRole(role);

		Assert.Equal(ExtremeRoleId.Nullifier, role.Core.Id);
		Assert.True(role.IsImpostor());
	}

	[Fact]
	public void NullifierRole_CheckAbility_ReturnsFalseIfPlayerMoves()
	{
		var role = new NullifierRole();
		InitializeRole(role);

		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));
		role.UseAbility();

		// 静止している間はtrue
		Assert.True(role.CheckAbility());

		// 移動した場合はfalse（詠唱中断）
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(1f, 1f));
		Assert.False(role.CheckAbility());
	}

	[Fact]
	public void NullifierSystem_ReserveSeal_PlacesAfterMeetingEnd()
	{
		var mockCoolTextTrans = new Mock<Transform>(IntPtr.Zero);
		mockCoolTextTrans.SetupProperty(t => t.localPosition);
		mockCoolTextTrans.SetupProperty(t => t.localScale);

		var mockCoolTextGo = new Mock<GameObject>(IntPtr.Zero);
		mockCoolTextGo.SetupGet(g => g.transform).Returns(mockCoolTextTrans.Object);

		var mockCoolText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		mockCoolText.SetupGet(t => t.gameObject).Returns(mockCoolTextGo.Object);
		mockCoolText.SetupGet(t => t.transform).Returns(mockCoolTextTrans.Object);
		mockCoolText.SetupProperty(t => t.text);
		mockCoolText.SetupProperty(t => t.alignment);
		mockCoolText.SetupProperty(t => t.color);

		var mockKillButton = new Mock<KillButton>(IntPtr.Zero);
		mockKillButton.SetupGet(b => b.cooldownTimerText).Returns(mockCoolText.Object);

		var mockHud = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		mockHud.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate6 = new Mock<MockObjectInstantiateHelper6>();
		mockInstantiate6.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>(), It.IsAny<bool>()))
			.Returns((UnityEngine.Object original, Transform parent, bool stay) => original);
		MockObjectInstantiateHelper6.Instance = mockInstantiate6.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<Il2CppSystem.Object>(), It.IsAny<Transform>()))
			.Returns((Il2CppSystem.Object original, Transform parent) => original);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		var mockInstantiate11 = new Mock<MockObjectInstantiateHelper11>();
		mockInstantiate11.Setup(x => x.Invoke(It.IsAny<Il2CppSystem.Object>(), It.IsAny<Transform>(), It.IsAny<bool>()))
			.Returns((Il2CppSystem.Object original, Transform parent, bool stay) => original);
		MockObjectInstantiateHelper11.Instance = mockInstantiate11.Object;

		var system = new NullifierSystem(
			2.5f,
			10,
			10.0f,
			NullifierTargetTeam.AllImpostorOther,
			this.mockFactory);

		system.ReserveSeal(new Vector2(1f, 2f), false);

		var scheduledReservationsField = typeof(NullifierSystem).GetField("scheduledReservations", BindingFlags.NonPublic | BindingFlags.Instance);
		var scheduledReservations = (IList)scheduledReservationsField?.GetValue(system)!;

		var placedSealsField = typeof(NullifierSystem).GetField("placedSeals", BindingFlags.NonPublic | BindingFlags.Instance);
		var placedSeals = (IDictionary)placedSealsField?.GetValue(system)!;

		Assert.Single(scheduledReservations);
		Assert.Empty(placedSeals);

		system.Reset(ResetTiming.MeetingEnd);
		Assert.Empty(scheduledReservations);
		Assert.Single(placedSeals);
	}

	[Fact]
	public void NullifierSystem_LocksAndUnlocksAbilityButton_ForCrewmate()
	{
		var system = new NullifierSystem(
			2.5f,
			10,
			10.0f,
			NullifierTargetTeam.CrewmateOnly,
			this.mockFactory);

		// 役職をクルーメイトとして登録
		var crewRole = new Sheriff();
		InitializeRole(crewRole, 0);

		AddSealForTest(system, 0, new Vector2(0f, 0f));

		// 陣の範囲内にいるのでロックされる
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));
		system.Deteriorate(0.1f);
		Assert.True(ButtonLockSystem.IsAbilityButtonLock());

		// 陣の範囲外へ移動するとアンロック
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(50f, 50f));
		system.Deteriorate(0.1f);
		Assert.False(ButtonLockSystem.IsAbilityButtonLock());
	}

	[Fact]
	public void NullifierSystem_DoesNotLockImpostor()
	{
		var system = new NullifierSystem(
			2.5f,
			10,
			10.0f,
			NullifierTargetTeam.AllImpostorOther,
			this.mockFactory);

		// 役職をナリファイア（インポスター）として登録
		var role = new NullifierRole();
		InitializeRole(role, 0);

		AddSealForTest(system, 0, new Vector2(0f, 0f));

		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));
		system.Deteriorate(0.1f);
		Assert.False(ButtonLockSystem.IsAbilityButtonLock());
	}

	[Fact]
	public void NullifierConsoleBehavior_ClickAndRestoreTimer()
	{
		var mockText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		var mockTextGo = new Mock<GameObject>(IntPtr.Zero);
		mockText.SetupGet(t => t.gameObject).Returns(mockTextGo.Object);
		mockText.SetupProperty(t => t.text);

		var behavior = new NullifierSystem.NullifierConsoleBehavior(0, 10, mockText.Object);

		var remainingClicksField = typeof(NullifierSystem.NullifierConsoleBehavior).GetField("remainingClicks", BindingFlags.NonPublic | BindingFlags.Instance);
		var inactiveTimerField = typeof(NullifierSystem.NullifierConsoleBehavior).GetField("inactiveTimer", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.Equal(10, (int)remainingClicksField?.GetValue(behavior)!);

		behavior.Use();
		Assert.Equal(9, (int)remainingClicksField?.GetValue(behavior)!);
		Assert.Equal(0.0f, (float)inactiveTimerField?.GetValue(behavior)!);

		// 5秒経過（まだリセットされない）
		behavior.UpdateTimer(5.0f, 10.0f);
		Assert.Equal(9, (int)remainingClicksField?.GetValue(behavior)!);

		// さらに5秒経過（合計10秒に達したので10回に復元）
		behavior.UpdateTimer(5.0f, 10.0f);
		Assert.Equal(10, (int)remainingClicksField?.GetValue(behavior)!);
		Assert.Equal(0.0f, (float)inactiveTimerField?.GetValue(behavior)!);
	}
}
