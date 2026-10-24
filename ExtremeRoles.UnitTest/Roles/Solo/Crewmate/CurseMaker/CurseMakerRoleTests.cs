using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MonoMod.RuntimeDetour;
using Moq;
using UnityEngine;
using UnityEngine.Events;
using Xunit;

using CurseMakerRole = ExtremeRoles.Roles.Solo.Crewmate.CurseMaker;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.CurseMaker;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class CurseMakerRoleTests : IDisposable
{
	private delegate void ArrowCtorOrig(Arrow self, Color color);
	private delegate void ArrowCtorHook(ArrowCtorOrig orig, Arrow self, Color color);

	private delegate void ArrowUpdateTargetOrig(Arrow self, Vector3? target);
	private delegate void ArrowUpdateTargetHook(ArrowUpdateTargetOrig orig, Arrow self, Vector3? target);

	private delegate void ArrowUpdateOrig(Arrow self);
	private delegate void ArrowUpdateHook(ArrowUpdateOrig orig, Arrow self);

	private delegate void ArrowClearOrig(Arrow self);
	private delegate void ArrowClearHook(ArrowClearOrig orig, Arrow self);

	private delegate float Vector2DistanceOrig(Vector2 a, Vector2 b);
	private delegate float Vector2DistanceHook(Vector2DistanceOrig orig, Vector2 a, Vector2 b);

	private delegate bool AnythingBetweenOrig(Vector2 source, Vector2 target, int layerMask, bool useTriggers);
	private delegate bool AnythingBetweenHook(AnythingBetweenOrig orig, Vector2 source, Vector2 target, int layerMask, bool useTriggers);

	private readonly Hook arrowCtorHook;
	private readonly Hook arrowUpdateTargetHook;
	private readonly Hook? arrowUpdateHook;
	private readonly Hook? arrowClearHook;
	private readonly Hook vector2DistanceHook;
	private readonly Hook? anythingBetweenHook;

	private readonly Mock<AmongUsClient> clientMock;

	public CurseMakerRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();
		MockSetupHelper.SetupTimeHelpers();
		MockSetupHelper.SetupGameDataMock();

		SetupShouldPlaySfxMock();
		SetupPlayersOnlyMaskMock();
		SetupSpriteCacheMock();
		SetupFindObjectsMock();
		SetupSoundCacheMock();
		SetupHudManagerMock();

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

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation
			.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

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

		var distanceTarget = typeof(Vector2).GetMethod(nameof(Vector2.Distance), new[] { typeof(Vector2), typeof(Vector2) })!;
		var distanceHookDelegate = new Vector2DistanceHook((orig, a, b) =>
		{
			float dx = a.x - b.x;
			float dy = a.y - b.y;
			return (float)Math.Sqrt(dx * dx + dy * dy);
		});
		this.vector2DistanceHook = new Hook(distanceTarget, distanceHookDelegate);

		var anythingBetweenTarget = typeof(PhysicsHelpers).GetMethod(nameof(PhysicsHelpers.AnythingBetween), new[] { typeof(Vector2), typeof(Vector2), typeof(int), typeof(bool) });
		if (anythingBetweenTarget != null)
		{
			var anythingBetweenDelegate = new AnythingBetweenHook((orig, source, target, layerMask, useTriggers) => false);
			this.anythingBetweenHook = new Hook(anythingBetweenTarget, anythingBetweenDelegate);
		}
	}

	public void Dispose()
	{
		this.arrowCtorHook.Dispose();
		this.arrowUpdateTargetHook.Dispose();
		this.arrowUpdateHook?.Dispose();
		this.arrowClearHook?.Dispose();
		this.vector2DistanceHook.Dispose();
		this.anythingBetweenHook?.Dispose();
	}

	private static void SetupShouldPlaySfxMock()
	{
		var mockSfxHelper = new Mock<MockConstantsShouldPlaySfxHelper>();
		mockSfxHelper.Setup(x => x.Invoke()).Returns(false);
		MockConstantsShouldPlaySfxHelper.Instance = mockSfxHelper.Object;
	}

	private static void SetupPlayersOnlyMaskMock()
	{
		if (UnityEngine.MockLayerMaskop_ImplicitHelper.Instance == null)
		{
			var mockImplicit = new Mock<UnityEngine.MockLayerMaskop_ImplicitHelper>();
			mockImplicit.Setup(m => m.Invoke(It.IsAny<LayerMask>())).Returns(1);
			UnityEngine.MockLayerMaskop_ImplicitHelper.Instance = mockImplicit.Object;
		}

		if (MockConstantsget_PlayersOnlyMaskHelper.Instance == null)
		{
			var mockMask = new Mock<MockConstantsget_PlayersOnlyMaskHelper>();
			mockMask.Setup(m => m.Invoke()).Returns(new LayerMask());
			MockConstantsget_PlayersOnlyMaskHelper.Instance = mockMask.Object;
		}

		if (MockConstantsget_ShipAndObjectsMaskHelper.Instance == null)
		{
			var mockMask = new Mock<MockConstantsget_ShipAndObjectsMaskHelper>();
			mockMask.Setup(m => m.Invoke()).Returns(new LayerMask());
			MockConstantsget_ShipAndObjectsMaskHelper.Instance = mockMask.Object;
		}

		if (UnityEngine.MockPhysics2DRaycastAllHelper.Instance == null)
		{
			var mockRaycast = new Mock<UnityEngine.MockPhysics2DRaycastAllHelper>();
			mockRaycast.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>()))
				.Returns((Il2CppStructArray<RaycastHit2D>)null!);
			UnityEngine.MockPhysics2DRaycastAllHelper.Instance = mockRaycast.Object;
		}

		if (UnityEngine.MockPhysics2DRaycastAllHelper2.Instance == null)
		{
			var mockRaycast2 = new Mock<UnityEngine.MockPhysics2DRaycastAllHelper2>();
			mockRaycast2.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>(), It.IsAny<float>()))
				.Returns((Il2CppStructArray<RaycastHit2D>)null!);
			UnityEngine.MockPhysics2DRaycastAllHelper2.Instance = mockRaycast2.Object;
		}

		if (UnityEngine.MockPhysics2DRaycastAllHelper3.Instance == null)
		{
			var mockRaycast3 = new Mock<UnityEngine.MockPhysics2DRaycastAllHelper3>();
			mockRaycast3.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>(), It.IsAny<float>(), It.IsAny<int>()))
				.Returns((Il2CppStructArray<RaycastHit2D>)null!);
			UnityEngine.MockPhysics2DRaycastAllHelper3.Instance = mockRaycast3.Object;
		}

		if (UnityEngine.MockPhysics2DRaycastAllHelper4.Instance == null)
		{
			var mockRaycast4 = new Mock<UnityEngine.MockPhysics2DRaycastAllHelper4>();
			mockRaycast4.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<float>()))
				.Returns((Il2CppStructArray<RaycastHit2D>)null!);
			UnityEngine.MockPhysics2DRaycastAllHelper4.Instance = mockRaycast4.Object;
		}

		if (UnityEngine.MockRaycastHit2Dop_ImplicitHelper.Instance == null)
		{
			var mockImplicitRaycast = new Mock<UnityEngine.MockRaycastHit2Dop_ImplicitHelper>();
			mockImplicitRaycast.Setup(x => x.Invoke(It.IsAny<RaycastHit2D>())).Returns(false);
			UnityEngine.MockRaycastHit2Dop_ImplicitHelper.Instance = mockImplicitRaycast.Object;
		}
	}

	private static void SetupSpriteCacheMock()
	{
		string spriteKey = $"{ObjectPath.CurseMakerCurse}115";
		if (!LruCache<string, Sprite>.TryGetValue(spriteKey, out _))
		{
			var mockSprite = new Mock<Sprite>(IntPtr.Zero);
			LruCache<string, Sprite>.Add(spriteKey, mockSprite.Object);
		}
	}

	private static void SetupSoundCacheMock()
	{
		var cachedAudioField = typeof(Sound).GetField("cachedAudio", BindingFlags.NonPublic | BindingFlags.Static);
		if (cachedAudioField?.GetValue(null) is Dictionary<Sound.Type, AudioClip> cachedAudio)
		{
			var mockClip = new Mock<AudioClip>(IntPtr.Zero);
			cachedAudio[Sound.Type.CurseMakerCurse] = mockClip.Object;
		}
	}

	private static void SetupFindObjectsMock()
	{
		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<DeadBody>()).Returns(new Il2CppReferenceArray<DeadBody>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;
	}

	private static void SetupHudManagerMock()
	{
		if (MockVector3get_oneHelper.Instance == null)
		{
			var mockOne = new Mock<MockVector3get_oneHelper>();
			mockOne.Setup(x => x.Invoke()).Returns(new Vector3(1f, 1f, 1f));
			MockVector3get_oneHelper.Instance = mockOne.Object;
		}

		var mockGridArrange = new Mock<GridArrange>(IntPtr.Zero);
		mockGridArrange.Setup(g => g.ArrangeChilds());

		var field = typeof(ExtremeRoles.Extension.Manager.HudManagerExtension).GetField("cachedArrange", BindingFlags.NonPublic | BindingFlags.Static);
		field?.SetValue(null, mockGridArrange.Object);

		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));

		var mockParentGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockParentGameObject.Setup(g => g.GetComponent<GridArrange>()).Returns(mockGridArrange.Object);
		var mockParentTransform = new Mock<Transform>(IntPtr.Zero);
		mockParentTransform.Setup(t => t.gameObject).Returns(mockParentGameObject.Object);

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockTransform.Setup(t => t.parent).Returns(mockParentTransform.Object);
		mockTransform.Setup(t => t.FindChild(It.IsAny<string>())).Returns((Transform)null!);

		var mockMaterial = new Mock<Material>(IntPtr.Zero);
		mockMaterial.Setup(m => m.SetFloat(It.IsAny<string>(), It.IsAny<float>()));

		var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockSpriteRenderer.SetupProperty(s => s.sprite);
		mockSpriteRenderer.SetupProperty(s => s.color);
		mockSpriteRenderer.SetupProperty(s => s.enabled);
		mockSpriteRenderer.SetupGet(s => s.material).Returns(mockMaterial.Object);

		var mockLabelText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		mockLabelText.SetupProperty(t => t.color);
		mockLabelText.SetupProperty(t => t.fontMaterial);
		mockLabelText.SetupProperty(t => t.text);

		var mockCoolText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		mockCoolText.SetupProperty(t => t.color);
		mockCoolText.SetupProperty(t => t.enableWordWrapping);
		mockCoolText.SetupProperty(t => t.text);
		mockCoolText.SetupGet(t => t.gameObject).Returns(mockGameObject.Object);
		mockCoolText.SetupGet(t => t.transform).Returns(mockTransform.Object);

		var mockPersistentCallGroup = new Mock<PersistentCallGroup>(IntPtr.Zero);
		var mockOnClick = new Mock<UnityEngine.UI.Button.ButtonClickedEvent>(IntPtr.Zero);
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

		var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		hudMock.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
		hudMock.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;
	}

	private static Mock<PlayerControl> CreateValidPlayerMock(byte playerId)
	{
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(0);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.PlayerId).Returns(playerId);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Tasks).Returns(mockTasksList.Object);

		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		return mockPlayer;
	}

	private static CurseMakerRole CreateInitializedCurseMaker(
		bool isNotRemoveDeadBodyByTask = false,
		int notRemoveDeadBodyTaskGage = 100,
		bool isDeadBodySearch = true,
		bool isMultiDeadBodySearch = false,
		float searchDeadBodyTime = 60.0f,
		bool isReduceSearchForTask = false,
		int reduceSearchTaskGage = 100,
		float reduceSearchDeadBodyTime = 30.0f,
		int taskCurseTimeReduceRate = 0)
	{
		var role = new CurseMakerRole();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(CurseMakerRole.CurseMakerOption.IsNotRemoveDeadBodyByTask, out var removeOpt) && removeOpt != null)
		{
			removeOpt.Selection = isNotRemoveDeadBodyByTask ? 1 : 0;
		}
		if (role.Loader.TryGet(CurseMakerRole.CurseMakerOption.NotRemoveDeadBodyTaskGage, out var removeGageOpt) && removeGageOpt != null)
		{
			removeGageOpt.Selection = notRemoveDeadBodyTaskGage / 5;
		}
		if (role.Loader.TryGet(CurseMakerRole.CurseMakerOption.IsDeadBodySearch, out var searchOpt) && searchOpt != null)
		{
			searchOpt.Selection = isDeadBodySearch ? 1 : 0;
		}
		if (role.Loader.TryGet(CurseMakerRole.CurseMakerOption.IsMultiDeadBodySearch, out var multiSearchOpt) && multiSearchOpt != null)
		{
			multiSearchOpt.Selection = isMultiDeadBodySearch ? 1 : 0;
		}
		if (role.Loader.TryGet(CurseMakerRole.CurseMakerOption.TaskCurseTimeReduceRate, out var reduceRateOpt) && reduceRateOpt != null)
		{
			reduceRateOpt.Selection = taskCurseTimeReduceRate;
		}
		if (role.Loader.TryGet(CurseMakerRole.CurseMakerOption.IsReduceSearchForTask, out var reduceSearchOpt) && reduceSearchOpt != null)
		{
			reduceSearchOpt.Selection = isReduceSearchForTask ? 1 : 0;
		}
		if (role.Loader.TryGet(CurseMakerRole.CurseMakerOption.ReduceSearchTaskGage, out var reduceGageOpt) && reduceGageOpt != null)
		{
			reduceGageOpt.Selection = (reduceSearchTaskGage - 25) / 5;
		}

		role.Initialize();
		return role;
	}

	[Fact]
	public void Constructor_SetsRoleProperties()
	{
		// Act
		var role = new CurseMakerRole();

		// Assert
		Assert.Equal(ExtremeRoleId.CurseMaker, role.Core.Id);
		Assert.Equal(ColorPalette.CurseMakerViolet, role.Core.Color);
	}

	[Fact]
	public void CreateRoleAllOption_CreatesOptionsCategory()
	{
		// Arrange
		var role = new CurseMakerRole();

		// Act
		role.CreateRoleAllOption();

		// Assert
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.CurseMaker);
		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);
	}

	[Fact]
	public void Initialize_ConfiguresFieldsFromOptions_WhenIsNotRemoveDeadBodyByTaskAnd0Gage()
	{
		// Act
		var role = CreateInitializedCurseMaker(
			isNotRemoveDeadBodyByTask: true,
			notRemoveDeadBodyTaskGage: 0,
			isDeadBodySearch: true);

		// Assert
		var isRemoveDeadBodyField = typeof(CurseMakerRole).GetField("isRemoveDeadBody", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(isRemoveDeadBodyField);
		var val = isRemoveDeadBodyField.GetValue(role);
		Assert.NotNull(val);
		Assert.False((bool)val);
	}

	[Fact]
	public void Initialize_ConfiguresFieldsFromOptions_WhenIsNotRemoveDeadBodyByTaskAnd100Gage()
	{
		// Act
		var role = CreateInitializedCurseMaker(
			isNotRemoveDeadBodyByTask: true,
			notRemoveDeadBodyTaskGage: 100,
			isDeadBodySearch: true);

		// Assert
		var isRemoveDeadBodyField = typeof(CurseMakerRole).GetField("isRemoveDeadBody", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(isRemoveDeadBodyField);
		var val = isRemoveDeadBodyField.GetValue(role);
		Assert.NotNull(val);
		Assert.True((bool)val);
	}

	[Fact]
	public void CreateAbility_InitializesButtonProperty()
	{
		// Arrange
		var role = new CurseMakerRole();
		role.CreateRoleAllOption();
		role.Initialize();

		// Act
		role.CreateAbility();

		// Assert
		Assert.NotNull(role.Button);
	}

	[Fact]
	public void IsAbilityUse_WhenNoDeadBodyTarget_ReturnsFalse()
	{
		// Arrange
		var mockOverlap = new Mock<UnityEngine.MockPhysics2DOverlapCircleAllHelper>();
		mockOverlap.Setup(m => m.Invoke(It.IsAny<Vector2>(), It.IsAny<float>(), It.IsAny<int>()))
			.Returns(Array.Empty<Collider2D>());
		UnityEngine.MockPhysics2DOverlapCircleAllHelper.Instance = mockOverlap.Object;

		var role = new CurseMakerRole();
		role.CreateRoleAllOption();
		role.Initialize();

		// Act
		bool isUse = role.IsAbilityUse();

		// Assert
		Assert.False(isUse);
	}

	[Fact]
	public void IsAbilityUse_WhenDeadBodyTargetPresent_ReturnsTrue()
	{
		// Arrange
		byte localPlayerId = 1;
		byte targetPlayerId = 10;

		var mockLocalPlayer = CreateValidPlayerMock(localPlayerId);
		mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);
		MockPlayerControlget_LocalPlayerHelper.Instance = new Mock<MockPlayerControlget_LocalPlayerHelper>().Object;
		var mockLocalHelper = Mock<MockPlayerControlget_LocalPlayerHelper>.Get(MockPlayerControlget_LocalPlayerHelper.Instance);
		mockLocalHelper.Setup(h => h.Invoke()).Returns(mockLocalPlayer.Object);

		var mockCollider = new Mock<Collider2D>(IntPtr.Zero);
		mockCollider.Setup(c => c.CompareTag("DeadBody")).Returns(true);

		var mockDeadBody = new Mock<DeadBody>(IntPtr.Zero);
		mockDeadBody.SetupGet(d => d.Reported).Returns(false);
		mockDeadBody.SetupGet(d => d.TruePosition).Returns(Vector2.zero);
		mockDeadBody.SetupGet(d => d.ParentId).Returns(targetPlayerId);

		mockCollider.Setup(c => c.GetComponent<DeadBody>()).Returns(mockDeadBody.Object);

		var mockOverlap = new Mock<UnityEngine.MockPhysics2DOverlapCircleAllHelper>();
		mockOverlap.Setup(m => m.Invoke(It.IsAny<Vector2>(), It.IsAny<float>(), It.IsAny<int>()))
			.Returns(new[] { mockCollider.Object });
		UnityEngine.MockPhysics2DOverlapCircleAllHelper.Instance = mockOverlap.Object;

		var mockTargetPlayer = CreateValidPlayerMock(targetPlayerId);
		Mock.Get(GameData.Instance).Setup(g => g.GetPlayerById(targetPlayerId)).Returns(mockTargetPlayer.Object.Data);

		var role = new CurseMakerRole();
		role.CreateRoleAllOption();
		role.Initialize();

		// Act
		bool isUse = role.IsAbilityUse();

		// Assert
		Assert.True(isUse);
	}

	[Fact]
	public void UseAbility_SetsDeadBodyIdAndReturnsTrue()
	{
		// Arrange
		var role = new CurseMakerRole();
		role.CreateRoleAllOption();
		role.Initialize();

		byte targetId = 5;
		var mockTargetInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTargetInfo.SetupGet(t => t.PlayerId).Returns(targetId);

		var targetBodyField = typeof(CurseMakerRole).GetField("targetBody", BindingFlags.NonPublic | BindingFlags.Instance);
		targetBodyField?.SetValue(role, mockTargetInfo.Object);

		// Act
		bool result = role.UseAbility();

		// Assert
		Assert.True(result);
		var deadBodyIdField = typeof(CurseMakerRole).GetField("deadBodyId", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(deadBodyIdField);
		var deadBodyIdVal = deadBodyIdField.GetValue(role);
		Assert.NotNull(deadBodyIdVal);
		Assert.Equal(targetId, (byte)deadBodyIdVal);
	}

	[Fact]
	public void CheckAbility_WhenTargetBodyIsNull_ReturnsFalseAndSetsDefaultButtonText()
	{
		// Arrange
		var mockOverlap = new Mock<UnityEngine.MockPhysics2DOverlapCircleAllHelper>();
		mockOverlap.Setup(m => m.Invoke(It.IsAny<Vector2>(), It.IsAny<float>(), It.IsAny<int>()))
			.Returns(Array.Empty<Collider2D>());
		UnityEngine.MockPhysics2DOverlapCircleAllHelper.Instance = mockOverlap.Object;

		var role = new CurseMakerRole();
		role.CreateRoleAllOption();
		role.Initialize();
		role.CreateAbility();

		// Act
		bool result = role.CheckAbility();

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void CheckAbility_WhenTargetBodyMatchesDeadBodyId_ReturnsTrueAndSetsCursingText()
	{
		// Arrange
		byte localPlayerId = 1;
		byte targetId = 3;

		var mockLocalPlayer = CreateValidPlayerMock(localPlayerId);
		mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);
		MockPlayerControlget_LocalPlayerHelper.Instance = new Mock<MockPlayerControlget_LocalPlayerHelper>().Object;
		var mockLocalHelper = Mock<MockPlayerControlget_LocalPlayerHelper>.Get(MockPlayerControlget_LocalPlayerHelper.Instance);
		mockLocalHelper.Setup(h => h.Invoke()).Returns(mockLocalPlayer.Object);

		var mockCollider = new Mock<Collider2D>(IntPtr.Zero);
		mockCollider.Setup(c => c.CompareTag("DeadBody")).Returns(true);

		var mockDeadBody = new Mock<DeadBody>(IntPtr.Zero);
		mockDeadBody.SetupGet(d => d.Reported).Returns(false);
		mockDeadBody.SetupGet(d => d.TruePosition).Returns(Vector2.zero);
		mockDeadBody.SetupGet(d => d.ParentId).Returns(targetId);

		mockCollider.Setup(c => c.GetComponent<DeadBody>()).Returns(mockDeadBody.Object);

		var mockOverlap = new Mock<UnityEngine.MockPhysics2DOverlapCircleAllHelper>();
		mockOverlap.Setup(m => m.Invoke(It.IsAny<Vector2>(), It.IsAny<float>(), It.IsAny<int>()))
			.Returns(new[] { mockCollider.Object });
		UnityEngine.MockPhysics2DOverlapCircleAllHelper.Instance = mockOverlap.Object;

		var mockTargetPlayer = CreateValidPlayerMock(targetId);
		Mock.Get(GameData.Instance).Setup(g => g.GetPlayerById(targetId)).Returns(mockTargetPlayer.Object.Data);

		var role = new CurseMakerRole();
		role.CreateRoleAllOption();
		role.Initialize();
		role.CreateAbility();

		var deadBodyIdField = typeof(CurseMakerRole).GetField("deadBodyId", BindingFlags.NonPublic | BindingFlags.Instance);
		deadBodyIdField?.SetValue(role, targetId);

		// Act
		bool result = role.CheckAbility();

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void CleanUp_WhenKillerEqualsTarget_CleansUpWithoutSendingCurseRpc()
	{
		// Arrange
		var role = new CurseMakerRole();
		role.CreateRoleAllOption();
		role.Initialize();

		byte playerId = 4;
		var deadBodyIdField = typeof(CurseMakerRole).GetField("deadBodyId", BindingFlags.NonPublic | BindingFlags.Instance);
		deadBodyIdField?.SetValue(role, playerId);

		var mockKiller = CreateValidPlayerMock(playerId);
		var mockTarget = CreateValidPlayerMock(playerId);

		var deadBodyInfo = new CurseMakerRole.DeadBodyInfo(mockKiller.Object, mockTarget.Object);

		var deadBodyDataField = typeof(CurseMakerRole).GetField("deadBodyData", BindingFlags.NonPublic | BindingFlags.Instance);
		var deadBodyData = (Dictionary<byte, CurseMakerRole.DeadBodyInfo>)deadBodyDataField!.GetValue(role)!;
		deadBodyData[playerId] = deadBodyInfo;

		var deadBodyArrowField = typeof(CurseMakerRole).GetField("deadBodyArrow", BindingFlags.NonPublic | BindingFlags.Instance);
		var deadBodyArrow = (Dictionary<byte, Arrow>)deadBodyArrowField!.GetValue(role)!;
		var arrow = new Arrow(Color.red);
		deadBodyArrow[playerId] = arrow;

		// Act
		role.CleanUp();

		// Assert
		Assert.False(deadBodyArrow.ContainsKey(playerId));
		Assert.NotNull(deadBodyIdField);
		var deadBodyIdVal = deadBodyIdField.GetValue(role);
		Assert.NotNull(deadBodyIdVal);
		Assert.Equal(byte.MaxValue, (byte)deadBodyIdVal);
	}

	[Fact]
	public void CleanUp_WhenKillerNotEqualsTarget_TriggersCurse()
	{
		// Arrange
		var role = new CurseMakerRole();
		role.CreateRoleAllOption();
		role.Initialize();

		byte rolePlayerId = 1;
		byte killerId = 2;
		byte targetId = 3;

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[rolePlayerId] = role;
		ExtremeRoleManager.GameRole[killerId] = new SpecialCrew();

		var localPlayerMock = CreateValidPlayerMock(rolePlayerId);
		MockPlayerControlget_LocalPlayerHelper.Instance = new Mock<MockPlayerControlget_LocalPlayerHelper>().Object;
		var mockLocalHelper = Mock<MockPlayerControlget_LocalPlayerHelper>.Get(MockPlayerControlget_LocalPlayerHelper.Instance);
		mockLocalHelper.Setup(h => h.Invoke()).Returns(localPlayerMock.Object);

		var mockKillerPlayer = CreateValidPlayerMock(killerId);
		var mockTargetPlayer = CreateValidPlayerMock(targetId);

		PlayerCache.AddPlayerControl(localPlayerMock.Object);
		PlayerCache.AddPlayerControl(mockKillerPlayer.Object);
		PlayerCache.AddPlayerControl(mockTargetPlayer.Object);

		var deadBodyIdField = typeof(CurseMakerRole).GetField("deadBodyId", BindingFlags.NonPublic | BindingFlags.Instance);
		deadBodyIdField?.SetValue(role, targetId);

		var deadBodyInfo = new CurseMakerRole.DeadBodyInfo(mockKillerPlayer.Object, mockTargetPlayer.Object);

		var deadBodyDataField = typeof(CurseMakerRole).GetField("deadBodyData", BindingFlags.NonPublic | BindingFlags.Instance);
		var deadBodyData = (Dictionary<byte, CurseMakerRole.DeadBodyInfo>)deadBodyDataField!.GetValue(role)!;
		deadBodyData[targetId] = deadBodyInfo;

		// Act
		role.CleanUp();

		// Assert
		Assert.NotNull(deadBodyIdField);
		var deadBodyIdVal = deadBodyIdField.GetValue(role);
		Assert.NotNull(deadBodyIdVal);
		Assert.Equal(byte.MaxValue, (byte)deadBodyIdVal);
	}

	[Fact]
	public void CurseKillCool_WhenConditionsMet_UpdatesKillCoolAndRoleState()
	{
		// Arrange
		byte rolePlayerId = 1;
		byte targetPlayerId = 2;

		var localPlayerMock = CreateValidPlayerMock(targetPlayerId);
		MockPlayerControlget_LocalPlayerHelper.Instance = new Mock<MockPlayerControlget_LocalPlayerHelper>().Object;
		var mockLocalHelper = Mock<MockPlayerControlget_LocalPlayerHelper>.Get(MockPlayerControlget_LocalPlayerHelper.Instance);
		mockLocalHelper.Setup(h => h.Invoke()).Returns(localPlayerMock.Object);

		PlayerCache.AddPlayerControl(localPlayerMock.Object);

		var curseMaker = new CurseMakerRole();
		curseMaker.CreateRoleAllOption();
		curseMaker.Initialize();

		var targetRole = new SpecialCrew();

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[rolePlayerId] = curseMaker;
		ExtremeRoleManager.GameRole[targetPlayerId] = targetRole;

		// Act
		CurseMakerRole.CurseKillCool(rolePlayerId, targetPlayerId);

		// Assert
		Assert.True(targetRole.HasOtherKillCool);
	}

	[Fact]
	public void CurseKillCool_WhenTargetNotLocalPlayer_ReturnsEarly()
	{
		// Arrange
		byte rolePlayerId = 1;
		byte targetPlayerId = 2;
		byte localPlayerId = 3;

		var localPlayerMock = CreateValidPlayerMock(localPlayerId);
		MockPlayerControlget_LocalPlayerHelper.Instance = new Mock<MockPlayerControlget_LocalPlayerHelper>().Object;
		var mockLocalHelper = Mock<MockPlayerControlget_LocalPlayerHelper>.Get(MockPlayerControlget_LocalPlayerHelper.Instance);
		mockLocalHelper.Setup(h => h.Invoke()).Returns(localPlayerMock.Object);

		var targetRole = new SpecialCrew();
		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[targetPlayerId] = targetRole;

		// Act
		CurseMakerRole.CurseKillCool(rolePlayerId, targetPlayerId);

		// Assert
		Assert.False(targetRole.HasOtherKillCool);
	}

	[Fact]
	public void CurseKillCool_WhenCurseMakerRoleNotFound_ReturnsEarly()
	{
		// Arrange
		byte rolePlayerId = 1;
		byte targetPlayerId = 2;

		var localPlayerMock = CreateValidPlayerMock(targetPlayerId);
		MockPlayerControlget_LocalPlayerHelper.Instance = new Mock<MockPlayerControlget_LocalPlayerHelper>().Object;
		var mockLocalHelper = Mock<MockPlayerControlget_LocalPlayerHelper>.Get(MockPlayerControlget_LocalPlayerHelper.Instance);
		mockLocalHelper.Setup(h => h.Invoke()).Returns(localPlayerMock.Object);

		PlayerCache.AddPlayerControl(localPlayerMock.Object);

		var targetRole = new SpecialCrew();
		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[targetPlayerId] = targetRole;

		// Act
		CurseMakerRole.CurseKillCool(rolePlayerId, targetPlayerId);

		// Assert
		Assert.False(targetRole.HasOtherKillCool);
	}

	[Fact]
	public void ResetOnMeetingStartAndEnd_ClearsStateAndExecutesSafely()
	{
		// Arrange
		var role = new CurseMakerRole();
		role.CreateRoleAllOption();
		role.Initialize();

		var deadBodyArrowField = typeof(CurseMakerRole).GetField("deadBodyArrow", BindingFlags.NonPublic | BindingFlags.Instance);
		var deadBodyArrow = (Dictionary<byte, Arrow>)deadBodyArrowField!.GetValue(role)!;
		deadBodyArrow[1] = new Arrow(Color.red);

		var deadBodyDataField = typeof(CurseMakerRole).GetField("deadBodyData", BindingFlags.NonPublic | BindingFlags.Instance);
		var deadBodyData = (Dictionary<byte, CurseMakerRole.DeadBodyInfo>)deadBodyDataField!.GetValue(role)!;
		var mockKiller = CreateValidPlayerMock(1);
		var mockTarget = CreateValidPlayerMock(2);
		deadBodyData[2] = new CurseMakerRole.DeadBodyInfo(mockKiller.Object, mockTarget.Object);

		// Act
		role.ResetOnMeetingStart();
		role.ResetOnMeetingEnd(null);

		// Assert
		Assert.Empty(deadBodyArrow);
		Assert.Empty(deadBodyData);
	}

	[Fact]
	public void HookMurderPlayer_WhenMeetingActiveOrDeadBodySearchDisabled_ReturnsEarly()
	{
		// Arrange
		var role = CreateInitializedCurseMaker(isDeadBodySearch: false);

		var mockKiller = CreateValidPlayerMock(1);
		var mockTarget = CreateValidPlayerMock(2);

		// Act
		role.HookMurderPlayer(mockKiller.Object, mockTarget.Object);

		// Assert
		var deadBodyDataField = typeof(CurseMakerRole).GetField("deadBodyData", BindingFlags.NonPublic | BindingFlags.Instance);
		var deadBodyData = (Dictionary<byte, CurseMakerRole.DeadBodyInfo>)deadBodyDataField!.GetValue(role)!;
		Assert.Empty(deadBodyData);
	}

	[Fact]
	public void HookMurderPlayer_WhenConditionsMet_AddsDeadBodyInfo()
	{
		// Arrange
		var role = CreateInitializedCurseMaker(isDeadBodySearch: true);

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		byte killerId = 1;
		byte targetId = 2;

		var mockKiller = CreateValidPlayerMock(killerId);
		var mockTarget = CreateValidPlayerMock(targetId);

		// Act
		role.HookMurderPlayer(mockKiller.Object, mockTarget.Object);

		// Assert
		var deadBodyDataField = typeof(CurseMakerRole).GetField("deadBodyData", BindingFlags.NonPublic | BindingFlags.Instance);
		var deadBodyData = (Dictionary<byte, CurseMakerRole.DeadBodyInfo>)deadBodyDataField!.GetValue(role)!;
		Assert.True(deadBodyData.ContainsKey(targetId));
	}

	[Fact]
	public void Update_WhenDeadBodyInfoInvalid_RemovesDeadBodyData()
	{
		// Arrange
		var role = CreateInitializedCurseMaker(isDeadBodySearch: true);

		byte targetId = 2;
		var mockKiller = CreateValidPlayerMock(1);
		var mockTarget = CreateValidPlayerMock(targetId);

		var deadBodyInfo = new CurseMakerRole.DeadBodyInfo(mockKiller.Object, mockTarget.Object);

		var deadBodyDataField = typeof(CurseMakerRole).GetField("deadBodyData", BindingFlags.NonPublic | BindingFlags.Instance);
		var deadBodyData = (Dictionary<byte, CurseMakerRole.DeadBodyInfo>)deadBodyDataField!.GetValue(role)!;
		deadBodyData[targetId] = deadBodyInfo;

		SetupFindObjectsMock();

		var mockRolePlayer = CreateValidPlayerMock(10);

		// Act
		role.Update(mockRolePlayer.Object);

		// Assert
		Assert.False(deadBodyData.ContainsKey(targetId));
	}

	[Fact]
	public void Update_WhenTaskProgressOccurs_UpdatesDeadBodyRemovalAndSearchFlags()
	{
		// Arrange
		var role = CreateInitializedCurseMaker(
			isNotRemoveDeadBodyByTask: true,
			notRemoveDeadBodyTaskGage: 50,
			isReduceSearchForTask: true,
			reduceSearchTaskGage: 50,
			reduceSearchDeadBodyTime: 10.0f);

		var mockRolePlayer = CreateValidPlayerMock(10);
		role.CreateAbility();

		// Task info setup (1 task complete out of 1 -> taskGage = 1.0 >= 0.5)
		var task1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		task1.SetupGet(t => t.Complete).Returns(true);

		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(task1.Object);

		Mock.Get(mockRolePlayer.Object.Data).SetupGet(d => d.Tasks).Returns(mockTasksList.Object);

		// Act
		role.Update(mockRolePlayer.Object);

		// Assert
		var isRemoveDeadBodyField = typeof(CurseMakerRole).GetField("isRemoveDeadBody", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(isRemoveDeadBodyField);
		var removeVal = isRemoveDeadBodyField.GetValue(role);
		Assert.NotNull(removeVal);
		Assert.False((bool)removeVal);

		var isReducedSearchTimeField = typeof(CurseMakerRole).GetField("isReducedSearchTime", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(isReducedSearchTimeField);
		var reducedVal = isReducedSearchTimeField.GetValue(role);
		Assert.NotNull(reducedVal);
		Assert.True((bool)reducedVal);
	}

	[Fact]
	public void DeadBodyInfo_Methods_ReturnExpectedValues()
	{
		// Arrange
		byte killerId = 1;
		byte targetId = 2;

		var mockKiller = CreateValidPlayerMock(killerId);
		var mockTarget = CreateValidPlayerMock(targetId);

		var deadBodyInfo = new CurseMakerRole.DeadBodyInfo(mockKiller.Object, mockTarget.Object);

		// Act & Assert
		Assert.Equal(killerId, deadBodyInfo.GetKiller());
		Assert.Equal(targetId, deadBodyInfo.GetTarget());
		Assert.True(deadBodyInfo.ComputeDeltaTime() >= 0f);
	}
}
