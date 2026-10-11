using System;
using System.Collections.Generic;
using System.Reflection;

using AmongUs.GameOptions;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using MonoMod.RuntimeDetour;
using Moq;
using UnityEngine;
using UnityEngine.Events;
using Xunit;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class FlasherTests : IDisposable
{
	private sealed class TestDummyRole : SingleRoleBase
	{
		public TestDummyRole(RoleArgs args) : base(args) { }

		protected override void RoleSpecificInit() { }

		protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) { }
	}

	private delegate float Vector2MagOrig(ref Vector2 self);
	private delegate float Vector2MagHook(Vector2MagOrig orig, ref Vector2 self);

	private delegate bool AnyNonTriggersOrig(Vector2 start, Vector2 dir, float distance, int layerMask);
	private delegate bool AnyNonTriggersHook(AnyNonTriggersOrig orig, Vector2 start, Vector2 dir, float distance, int layerMask);

	private readonly Hook magHook;
	private readonly Hook physicsHook;

	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;
	private readonly List<NetworkedPlayerInfo> playerInfoList = new();
	private readonly List<byte> sentRpcBytes = new();

	public FlasherTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupVector3Helpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		if (ExtremeGameModeManager.Instance == null)
		{
			ExtremeGameModeManager.Create(GameModes.Normal);
		}

		SetupLobbyBehaviourMock();
		SetupTranslationControllerMock();
		SetupShipStatusMock();
		SetupConstantsMock();
		SetupSpriteCacheMock();
		SetupAssetBundleMock();
		SetupHudManagerMock();

		var mockSqrt = new Mock<UnityEngine.MockMathfSqrtHelper>();
		mockSqrt.Setup(x => x.Invoke(It.IsAny<float>())).Returns((float f) => MathF.Sqrt(f));
		UnityEngine.MockMathfSqrtHelper.Instance = mockSqrt.Object;

		var mockSub = new Mock<MockVector2op_SubtractionHelper>();
		mockSub.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>()))
			.Returns((Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y));
		MockVector2op_SubtractionHelper.Instance = mockSub.Object;

		var mockActionImplicit = new Mock<Il2CppSystem.MockActionop_ImplicitHelper<float>>();
		mockActionImplicit.Setup(x => x.Invoke(It.IsAny<Action<float>>()))
			.Returns((Action<float> act) => act != null ? new Il2CppSystem.Action<float>(IntPtr.Zero) : null!);
		Il2CppSystem.MockActionop_ImplicitHelper<float>.Instance = mockActionImplicit.Object;

		var mockIEnum = new Mock<Il2CppSystem.Collections.IEnumerator>(IntPtr.Zero);
		var mockLerpHelper = new Mock<MockEffectsLerpHelper>();
		mockLerpHelper.Setup(x => x.Invoke(It.IsAny<float>(), It.IsAny<Il2CppSystem.Action<float>>()))
			.Returns(mockIEnum.Object);
		MockEffectsLerpHelper.Instance = mockLerpHelper.Object;

		var magTarget = typeof(Vector2).GetProperty("magnitude")!.GetGetMethod()!;
		var magHookDelegate = new Vector2MagHook(getMagnitudeHook);
		this.magHook = new Hook(magTarget, magHookDelegate);

		var physicsTarget = typeof(PhysicsHelpers).GetMethod(nameof(PhysicsHelpers.AnyNonTriggersBetween), new[] { typeof(Vector2), typeof(Vector2), typeof(float), typeof(int) })!;
		var physicsHookDelegate = new AnyNonTriggersHook((orig, start, dir, dist, mask) => false);
		this.physicsHook = new Hook(physicsTarget, physicsHookDelegate);

		this.mockClient = MockSetupHelper.SetupAmongUsClientMock();
		this.mockClient.SetupGet(c => c.AmHost).Returns(true);

		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockWriter.Setup(w => w.Write(It.IsAny<byte>())).Callback((byte b) => this.sentRpcBytes.Add(b));
		mockWriter.Setup(w => w.ToByteArray(It.IsAny<bool>())).Returns(new Mock<Il2CppStructArray<byte>>(IntPtr.Zero).Object);

		this.mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var cachedPtrField = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);

		this.mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		cachedPtrField?.SetValue(this.mockLocalPlayer.Object, (IntPtr)1);
		this.mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		this.mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(10f, 20f));

		var mockLocalTransform = new Mock<Transform>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockLocalTransform.Object, (IntPtr)101);
		mockLocalTransform.SetupGet(t => t.position).Returns(new Vector3(10f, 20f, 0f));
		this.mockLocalPlayer.SetupGet(p => p.transform).Returns(mockLocalTransform.Object);

		var mockLocalData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockLocalData.Object, (IntPtr)1);
		mockLocalData.SetupGet(d => d.PlayerId).Returns((byte)1);
		mockLocalData.SetupGet(d => d.IsDead).Returns(false);
		mockLocalData.SetupGet(d => d.Disconnected).Returns(false);
		mockLocalData.SetupGet(d => d.Object).Returns(this.mockLocalPlayer.Object);
		this.mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData.Object);

		this.playerInfoList.Add(mockLocalData.Object);

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockList.SetupGet(l => l.Count).Returns(() => this.playerInfoList.Count);
		mockList.Setup(l => l[It.IsAny<int>()]).Returns((int i) => this.playerInfoList[i]);
		mockGameData.SetupGet(g => g.AllPlayers).Returns(mockList.Object);

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		var mockExileControllerHelper = new Mock<MockExileControllerget_InstanceHelper>();
		mockExileControllerHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockExileControllerget_InstanceHelper.Instance = mockExileControllerHelper.Object;

		var mockMinigameHelper = new Mock<MockMinigameget_InstanceHelper>();
		mockMinigameHelper.Setup(h => h.Invoke()).Returns((Minigame)null!);
		MockMinigameget_InstanceHelper.Instance = mockMinigameHelper.Object;

		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;

		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		var mockGet = new Mock<Hazel.MockMessageWriterGetHelper>();
		mockGet.Setup(g => g.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
		Hazel.MockMessageWriterGetHelper.Instance = mockGet.Object;

		var mockReaderHelper = new Mock<Hazel.MockMessageReaderGetHelper>();
		mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppStructArray<byte>>()))
			.Returns(() =>
			{
				var mockReader = new Mock<MessageReader>();
				if (this.sentRpcBytes.Count > 0)
				{
					var bytesCopy = new List<byte>(this.sentRpcBytes);
					this.sentRpcBytes.Clear();
					var seq = mockReader.SetupSequence(r => r.ReadByte());
					foreach (var b in bytesCopy)
					{
						seq.Returns(b);
					}
				}
				return mockReader.Object;
			});
		Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;

		var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
		mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNetObject>()));
		InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;

		if (ExtremeRolesPlugin.ShipState == null)
		{
			var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(
				nameof(ExtremeRolesPlugin.ShipState),
				BindingFlags.Public | BindingFlags.Static);
			shipStateProp?.SetValue(null, new ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus());
		}
	}

	public void Dispose()
	{
		this.magHook.Dispose();
		this.physicsHook.Dispose();
	}

	private static float getMagnitudeHook(Vector2MagOrig orig, ref Vector2 self)
	{
		return (float)Math.Sqrt(self.x * self.x + self.y * self.y);
	}

	private static void SetupSpriteCacheMock()
	{
		string spriteKey = $"{ObjectPath.TestButton}115";
		if (!LruCache<string, Sprite>.TryGetValue(spriteKey, out _))
		{
			var mockSprite = new Mock<Sprite>(IntPtr.Zero);
			LruCache<string, Sprite>.Add(spriteKey, mockSprite.Object);
		}
	}

	private static void SetupAssetBundleMock()
	{
		var mockSprite = new Mock<Sprite>(IntPtr.Zero);

		var mockAssetBundle = new Mock<AssetBundle>(IntPtr.Zero);
		mockAssetBundle.Setup(b => b.LoadAsset(It.IsAny<string>(), It.IsAny<Il2CppSystem.Type>()))
			.Returns(mockSprite.Object);

		var field = typeof(UnityObjectLoader).GetField("cachedBundle", BindingFlags.NonPublic | BindingFlags.Static);
		if (field?.GetValue(null) is Dictionary<string, AssetBundle> dict)
		{
			dict[ObjectPath.GetRoleAssetPath(ExtremeRoleId.Flasher)] = mockAssetBundle.Object;
		}
	}

	private static void SetupConstantsMock()
	{
		var mockMask1 = new Mock<MockConstantsget_PlayersOnlyMaskHelper>();
		mockMask1.Setup(m => m.Invoke()).Returns(0);
		MockConstantsget_PlayersOnlyMaskHelper.Instance = mockMask1.Object;

		var mockMask2 = new Mock<MockConstantsget_ShipAndObjectsMaskHelper>();
		mockMask2.Setup(m => m.Invoke()).Returns(0);
		MockConstantsget_ShipAndObjectsMaskHelper.Instance = mockMask2.Object;
	}

	private static void SetupShipStatusMock()
	{
		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(x => x.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;
	}

	private static void SetupLobbyBehaviourMock()
	{
		var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
		var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
		mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
		MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
	}

	private static void SetupTranslationControllerMock()
	{
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
	}

	private static void SetupHudManagerMock()
	{
		if (MockVector3get_oneHelper.Instance == null)
		{
			var mockOne = new Mock<MockVector3get_oneHelper>();
			mockOne.Setup(x => x.Invoke()).Returns(new Vector3(1f, 1f, 1f));
			MockVector3get_oneHelper.Instance = mockOne.Object;
		}

		var mockObjectImplicitInt = new Mock<Il2CppSystem.MockObjectop_ImplicitHelper6>();
		mockObjectImplicitInt.Setup(x => x.Invoke(It.IsAny<int>())).Returns(new Mock<Il2CppSystem.Object>(IntPtr.Zero).Object);
		Il2CppSystem.MockObjectop_ImplicitHelper6.Instance = mockObjectImplicitInt.Object;

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
		mockTransform.SetupProperty(t => t.localPosition);
		mockTransform.SetupProperty(t => t.position);
		mockTransform.SetupProperty(t => t.localScale);

		var mockMaterial = new Mock<Material>(IntPtr.Zero);
		mockMaterial.Setup(m => m.SetFloat(It.IsAny<string>(), It.IsAny<float>()));

		var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockSpriteRenderer.SetupProperty(s => s.sprite);
		mockSpriteRenderer.SetupProperty(s => s.color);
		mockSpriteRenderer.SetupProperty(s => s.enabled);
		mockSpriteRenderer.SetupGet(s => s.material).Returns(mockMaterial.Object);
		mockSpriteRenderer.SetupGet(s => s.gameObject).Returns(mockGameObject.Object);
		mockSpriteRenderer.SetupGet(s => s.transform).Returns(mockTransform.Object);

		var mockLabelText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		mockLabelText.SetupProperty(t => t.color);
		mockLabelText.SetupProperty(t => t.fontMaterial);
		mockLabelText.SetupProperty(t => t.text);
		mockLabelText.SetupGet(t => t.transform).Returns(mockTransform.Object);

		var mockCoolText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
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
		mockPassiveButton.SetupGet(p => p.OnClick).Returns(mockOnClick.Object);

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

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		var mockCoroutine = new Mock<Coroutine>(IntPtr.Zero);

		var mockHud = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		mockHud.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
		mockHud.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);
		mockHud.SetupGet(h => h.FullScreen).Returns(mockSpriteRenderer.Object);
		mockHud.SetupGet(h => h.transform).Returns(mockTransform.Object);
		mockHud.Setup(h => h.StartCoroutine(It.IsAny<Il2CppSystem.Collections.IEnumerator>()))
			.Returns(mockCoroutine.Object);
		mockHud.Setup(h => h.StopCoroutine(It.IsAny<Coroutine>()));
	}

	private static void InitializeRole(SingleRoleBase role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole[playerId] = role;
	}

	private Mock<PlayerControl> createTargetPlayer(byte playerId, Vector2 position, SingleRoleBase role, bool isDead = false, bool inVent = false)
	{
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var cachedPtrField = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);
		cachedPtrField?.SetValue(mockPlayer.Object, (IntPtr)(playerId + 10));

		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
		mockPlayer.SetupGet(p => p.inVent).Returns(inVent);
		mockPlayer.Setup(p => p.GetTruePosition()).Returns(position);

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockTransform.Object, (IntPtr)(playerId + 100));
		mockTransform.SetupGet(t => t.position).Returns(new Vector3(position.x, position.y, 0f));
		mockPlayer.SetupGet(p => p.transform).Returns(mockTransform.Object);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockData.Object, (IntPtr)(playerId + 10));
		mockData.SetupGet(d => d.PlayerId).Returns(playerId);
		mockData.SetupGet(d => d.IsDead).Returns(isDead);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		this.playerInfoList.Add(mockData.Object);

		role.CreateRoleAllOption();
		role.Initialize();
		ExtremeRoleManager.GameRole[playerId] = role;

		return mockPlayer;
	}

	[Fact]
	public void Initialize_RegistersFlasherRoleAndImpostorTeam()
	{
		var role = new Flasher();

		InitializeRole(role);

		Assert.Equal(ExtremeRoleId.Flasher, role.Core.Id);
		Assert.True(role.IsImpostor());
	}

	[Fact]
	public void IsAbilityUse_ReturnsTrue_WhenPlayerCanMove()
	{
		var role = new Flasher();
		InitializeRole(role);

		bool canUse = role.IsAbilityUse();

		Assert.True(canUse);
	}

	[Fact]
	public void UseChargedAbility_SendsRpcWithOnlyValidCrewmatesInRange()
	{
		var flasherRole = new Flasher();
		InitializeRole(flasherRole, playerId: 1);

		// Target 11: Crewmate in range (5f away, default radius 8f)
		createTargetPlayer(11, new Vector2(15f, 20f), new TestDummyRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white)));

		// Target 12: Crewmate far out of range (1000f away)
		createTargetPlayer(12, new Vector2(1000f, 1000f), new TestDummyRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white)));

		// Target 13: Teammate Impostor in range
		createTargetPlayer(13, new Vector2(12f, 20f), new TestDummyRole(RoleArgs.BuildImpostor(ExtremeRoleId.Assassin)));

		// Target 14: Dead Crewmate in range
		createTargetPlayer(14, new Vector2(11f, 20f), new TestDummyRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white)), isDead: true);

		bool result = flasherRole.UseChargedAbility(0.8f);

		Assert.True(result);
		// sentRpcBytes contains RPC writer payload: callerId=1, targets.Count=1, targetId=11
		Assert.Contains((byte)11, this.sentRpcBytes);
		Assert.DoesNotContain((byte)13, this.sentRpcBytes);
		Assert.DoesNotContain((byte)14, this.sentRpcBytes);
	}

	[Fact]
	public void FlasherScreenEffect_ThrowsException_WhenHoldTimeInvalid()
	{
		Func<FlasherScreenEffect> createWithZeroHoldTime = () => new FlasherScreenEffect(Color.white, 0.0f, 0.5f);
		Func<FlasherScreenEffect> createWithNegativeHoldTime = () => new FlasherScreenEffect(Color.white, -1.0f, 0.5f);

		Assert.Throws<ArgumentOutOfRangeException>(createWithZeroHoldTime);
		Assert.Throws<ArgumentOutOfRangeException>(createWithNegativeHoldTime);
	}

	[Fact]
	public void RpcFlash_TriggersScreenEffectForTargetLocalPlayer()
	{
		var role = new Flasher();
		InitializeRole(role, playerId: 2);

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte()).Returns((byte)2).Returns((byte)1); // callerId = 2, targetId = 1
		mockReader.SetupSequence(r => r.ReadSingle()).Returns(0.8f).Returns(3.0f); // chargeGauge, activeTime
		mockReader.Setup(r => r.ReadPackedInt32()).Returns(1); // targetCount = 1

		var mockHud = HudManager.Instance;

		Flasher.RpcFlash(mockReader.Object);

		Mock.Get(mockHud).Verify(h => h.StartCoroutine(It.IsAny<Il2CppSystem.Collections.IEnumerator>()), Times.Once);
	}

	[Fact]
	public void RpcFlash_DoesNotTriggerScreenEffect_WhenLocalPlayerNotInTargetList()
	{
		var role = new Flasher();
		InitializeRole(role, playerId: 2);

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte()).Returns((byte)2).Returns((byte)3); // callerId = 2, targetId = 3
		mockReader.SetupSequence(r => r.ReadSingle()).Returns(0.8f).Returns(3.0f); // chargeGauge, activeTime
		mockReader.Setup(r => r.ReadPackedInt32()).Returns(1); // targetCount = 1

		var mockHud = HudManager.Instance;

		Flasher.RpcFlash(mockReader.Object);

		Mock.Get(mockHud).Verify(h => h.StartCoroutine(It.IsAny<Il2CppSystem.Collections.IEnumerator>()), Times.Never);
	}

	[Fact]
	public void ResetOnMeetingStartAndEnd_HidesActiveFlasherEffect()
	{
		var flasherRole = new Flasher();
		InitializeRole(flasherRole, playerId: 1);

		var mockHud = HudManager.Instance;

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte()).Returns((byte)1).Returns((byte)1); // callerId = 1, targetId = 1
		mockReader.SetupSequence(r => r.ReadSingle()).Returns(1.0f).Returns(3.0f);
		mockReader.Setup(r => r.ReadPackedInt32()).Returns(1);

		Flasher.RpcFlash(mockReader.Object);

		flasherRole.ResetOnMeetingStart();
		flasherRole.ResetOnMeetingEnd(null);

		Mock.Get(mockHud).Verify(h => h.StopCoroutine(It.IsAny<Coroutine>()), Times.AtLeastOnce());
	}
}
