using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Moq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.AgencyTests;

public static class PlayerGetClosestPlayerInRangePatch
{
	public static PlayerControl? OverrideTarget;

	public static bool Prefix(ref PlayerControl? __result)
	{
		if (OverrideTarget != null)
		{
			__result = OverrideTarget;
			return false;
		}
		return true;
	}
}

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class AgencyTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private static bool isHarmonyPatched = false;

	public AgencyTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		SetupVector2Helpers();
		SetupTaskHelpers();
		SetupPhysicsHelpers();
		SetupHarmony();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupGameDataMock();
		SetupHudManagerMock();

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		var mockLocalTransform = new Mock<Transform>(IntPtr.Zero);
		localPlayerMock.SetupGet(p => p.transform).Returns(mockLocalTransform.Object);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.Object).Returns(localPlayerMock.Object);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		localPlayerMock.SetupGet(p => p.Data).Returns(mockData.Object);

		mockClient = MockSetupHelper.SetupAmongUsClientMock();

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);

		MockSetupHelper.SetupOptionManager();

		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		if (InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance == null)
		{
			var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
			mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNetObject>()));
			InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;
		}

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
			mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>>()))
				.Returns(mockReader.Object);
			Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;
		}

		SetupShipStatusMock();
	}

	private static void SetupHarmony()
	{
		if (!isHarmonyPatched)
		{
			var harmony = new Harmony("test.agency.getclosestplayer");
			var targetMethod = typeof(Player).GetMethod(nameof(Player.GetClosestPlayerInRange), BindingFlags.Public | BindingFlags.Static);
			var prefixMethod = typeof(PlayerGetClosestPlayerInRangePatch).GetMethod(nameof(PlayerGetClosestPlayerInRangePatch.Prefix), BindingFlags.Public | BindingFlags.Static);
			if (targetMethod != null && prefixMethod != null)
			{
				harmony.Patch(targetMethod, prefix: new HarmonyMethod(prefixMethod));
			}
			isHarmonyPatched = true;
		}
		PlayerGetClosestPlayerInRangePatch.OverrideTarget = null;
	}

	private static void SetupTaskHelpers()
	{
		var mockTaskIsEmergency = new Mock<MockPlayerTaskTaskIsEmergencyHelper>();
		mockTaskIsEmergency.Setup(x => x.Invoke(It.IsAny<PlayerTask>())).Returns(false);
		MockPlayerTaskTaskIsEmergencyHelper.Instance = mockTaskIsEmergency.Object;
	}

	private static void SetupVector2Helpers()
	{
		var mockSub = new Mock<MockVector2op_SubtractionHelper>();
		mockSub.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>()))
			.Returns((Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y));
		MockVector2op_SubtractionHelper.Instance = mockSub.Object;

		var mockMag = new Mock<MockVector3MagnitudeHelper>();
		mockMag.Setup(x => x.Invoke(It.IsAny<Vector3>()))
			.Returns((Vector3 v) => MathF.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z));
		MockVector3MagnitudeHelper.Instance = mockMag.Object;
	}

	private static void SetupPhysicsHelpers()
	{
		var mockRaycast = new Mock<UnityEngine.MockPhysics2DRaycastAllHelper>();
		mockRaycast.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>()))
			.Returns((Il2CppStructArray<RaycastHit2D>)null!);
		UnityEngine.MockPhysics2DRaycastAllHelper.Instance = mockRaycast.Object;

		var mockRaycast2 = new Mock<UnityEngine.MockPhysics2DRaycastAllHelper2>();
		mockRaycast2.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>(), It.IsAny<float>()))
			.Returns((Il2CppStructArray<RaycastHit2D>)null!);
		UnityEngine.MockPhysics2DRaycastAllHelper2.Instance = mockRaycast2.Object;

		var mockRaycast3 = new Mock<UnityEngine.MockPhysics2DRaycastAllHelper3>();
		mockRaycast3.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>(), It.IsAny<float>(), It.IsAny<int>()))
			.Returns((Il2CppStructArray<RaycastHit2D>)null!);
		UnityEngine.MockPhysics2DRaycastAllHelper3.Instance = mockRaycast3.Object;

		var mockRaycast4 = new Mock<UnityEngine.MockPhysics2DRaycastAllHelper4>();
		mockRaycast4.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<float>()))
			.Returns((Il2CppStructArray<RaycastHit2D>)null!);
		UnityEngine.MockPhysics2DRaycastAllHelper4.Instance = mockRaycast4.Object;

		var mockImplicitRaycast = new Mock<UnityEngine.MockRaycastHit2Dop_ImplicitHelper>();
		mockImplicitRaycast.Setup(x => x.Invoke(It.IsAny<RaycastHit2D>())).Returns(false);
		UnityEngine.MockRaycastHit2Dop_ImplicitHelper.Instance = mockImplicitRaycast.Object;
	}

	private static void SetupHudManagerMock()
	{
		var systemManager = (ExtremeSystemTypeManager)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeSystemTypeManager));
		var allSystems = new Dictionary<ExtremeSystemType, IExtremeSystemType>();
		typeof(ExtremeSystemTypeManager)
			.GetField("allSystems", BindingFlags.NonPublic | BindingFlags.Instance)?
			.SetValue(systemManager, allSystems);
		typeof(ExtremeSystemTypeManager)
			.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static)?
			.SetValue(null, systemManager);

		var mockIntroHelper = new Mock<MockIntroCutsceneget_InstanceHelper>();
		mockIntroHelper.Setup(x => x.Invoke()).Returns((IntroCutscene)null!);
		MockIntroCutsceneget_InstanceHelper.Instance = mockIntroHelper.Object;

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		var mockDestroyableMeetingHelper = new Mock<MockDestroyableSingletonget_InstanceHelper<MeetingHud>>();
		mockDestroyableMeetingHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockDestroyableSingletonget_InstanceHelper<MeetingHud>.Instance = mockDestroyableMeetingHelper.Object;

		var mockExileControllerHelper = new Mock<MockExileControllerget_InstanceHelper>();
		mockExileControllerHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockExileControllerget_InstanceHelper.Instance = mockExileControllerHelper.Object;

		var mockDestroyableExileHelper = new Mock<MockDestroyableSingletonget_InstanceHelper<ExileController>>();
		mockDestroyableExileHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockDestroyableSingletonget_InstanceHelper<ExileController>.Instance = mockDestroyableExileHelper.Object;

		var mockGetKeyDown = new Mock<MockInputGetKeyDownHelper>();
		mockGetKeyDown.Setup(x => x.Invoke(It.IsAny<KeyCode>())).Returns(false);
		MockInputGetKeyDownHelper.Instance = mockGetKeyDown.Object;

		var mockGetKeyDownInt = new Mock<MockInputGetKeyDownIntHelper>();
		mockGetKeyDownInt.Setup(x => x.Invoke(It.IsAny<KeyCode>())).Returns(false);
		MockInputGetKeyDownIntHelper.Instance = mockGetKeyDownInt.Object;

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

		var mockLabelText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockLabelText.SetupProperty(t => t.color);
		mockLabelText.SetupProperty(t => t.fontMaterial);
		mockLabelText.SetupProperty(t => t.text);

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
		mockPassiveButton.SetupGet(p => p.OnClick).Returns(mockOnClick.Object);

		var mockKillButton = new Mock<KillButton>(IntPtr.Zero);
		mockKillButton.SetupGet(b => b.transform).Returns(mockTransform.Object);
		mockKillButton.SetupGet(b => b.gameObject).Returns(mockGameObject.Object);
		mockKillButton.SetupGet(b => b.graphic).Returns(mockSpriteRenderer.Object);
		mockKillButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
		mockKillButton.SetupGet(b => b.cooldownTimerText).Returns(mockCoolText.Object);
		mockKillButton.Setup(b => b.GetComponent<PassiveButton>()).Returns(mockPassiveButton.Object);
		mockKillButton.SetupGet(b => b.isActiveAndEnabled).Returns(true);
		mockKillButton.Setup(b => b.OverrideText(It.IsAny<string>()));
		mockKillButton.Setup(b => b.SetCoolDown(It.IsAny<float>(), It.IsAny<float>()));
		mockKillButton.Setup(b => b.SetCooldownFill(It.IsAny<float>()));

		var mockUseButton = new Mock<UseButton>(IntPtr.Zero);
		mockUseButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
		mockUseButton.SetupGet(b => b.transform).Returns(mockTransform.Object);

		var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		hudMock.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
		hudMock.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);

		var mockInstantiate = new Mock<MockObjectInstantiateHelper>();
		mockInstantiate.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Vector3>(), It.IsAny<Quaternion>()))
			.Returns((UnityEngine.Object original, Vector3 pos, Quaternion rot) => original);
		MockObjectInstantiateHelper.Instance = mockInstantiate.Object;

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate7 = new Mock<MockObjectInstantiateHelper7>();
		mockInstantiate7.Setup(x => x.Invoke(It.IsAny<Material>()))
			.Returns(mockMaterial.Object);
		MockObjectInstantiateHelper7.Instance = mockInstantiate7.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		var mockUnityActionImplicit = new Mock<MockUnityActionop_ImplicitHelper>();
		mockUnityActionImplicit.Setup(x => x.Invoke(It.IsAny<Action>()))
			.Returns((Action action) => action != null ? new UnityAction(IntPtr.Zero) : null!);
		MockUnityActionop_ImplicitHelper.Instance = mockUnityActionImplicit.Object;

		var mockAssetBundle = new Mock<AssetBundle>(IntPtr.Zero);
		var mockAudioClip = new Mock<AudioClip>(IntPtr.Zero);
		mockAssetBundle.Setup(b => b.LoadAsset(It.IsAny<string>(), It.IsAny<Il2CppSystem.Type>())).Returns(mockAudioClip.Object);

		var field = typeof(UnityObjectLoader).GetField("cachedBundle", BindingFlags.NonPublic | BindingFlags.Static);
		if (field?.GetValue(null) is Dictionary<string, AssetBundle> dict)
		{
			dict[ObjectPath.SoundEffect] = mockAssetBundle.Object;
		}

		var mockSoundManager = new Mock<SoundManager>(IntPtr.Zero);
		var mockAudioSource = new Mock<AudioSource>(IntPtr.Zero);
		mockSoundManager.Setup(s => s.PlaySound(It.IsAny<AudioClip>(), It.IsAny<bool>(), It.IsAny<float>(), null))
			.Returns(mockAudioSource.Object);

		var mockSoundInstanceHelper = new Mock<MockSoundManagerget_InstanceHelper>();
		mockSoundInstanceHelper.Setup(x => x.Invoke()).Returns(mockSoundManager.Object);
		MockSoundManagerget_InstanceHelper.Instance = mockSoundInstanceHelper.Object;
	}

	private static void SetupShipStatusMock()
	{
		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		mockShipStatus.SetupGet(s => s.enabled).Returns(true);

		var mockTaskCommon = new Mock<NormalPlayerTask>(IntPtr.Zero);
		mockTaskCommon.SetupGet(t => t.Index).Returns(1);
		var mockTaskLong = new Mock<NormalPlayerTask>(IntPtr.Zero);
		mockTaskLong.SetupGet(t => t.Index).Returns(2);
		var mockTaskShort = new Mock<NormalPlayerTask>(IntPtr.Zero);
		mockTaskShort.SetupGet(t => t.Index).Returns(3);

		var commonArray = new Il2CppReferenceArray<NormalPlayerTask>(new NormalPlayerTask[] { mockTaskCommon.Object });
		var longArray = new Il2CppReferenceArray<NormalPlayerTask>(new NormalPlayerTask[] { mockTaskLong.Object });
		var shortArray = new Il2CppReferenceArray<NormalPlayerTask>(new NormalPlayerTask[] { mockTaskShort.Object });

		mockShipStatus.SetupGet(s => s.CommonTasks).Returns(commonArray);
		mockShipStatus.SetupGet(s => s.LongTasks).Returns(longArray);
		mockShipStatus.SetupGet(s => s.ShortTasks).Returns(shortArray);

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;
	}

	[Fact]
	public void Constructor_InitializesCorrectly()
	{
		// Arrange & Act
		var agency = new Agency();

		// Assert
		Assert.NotNull(agency);
		Assert.Equal(ExtremeRoleId.Agency, agency.Core.Id);
		Assert.Equal(byte.MaxValue, agency.TargetPlayer);
	}

	[Fact]
	public void RoleSpecificInit_ReadsOptionsCorrectly()
	{
		// Arrange
		var agency = new Agency();
		agency.CreateRoleAllOption();

		// Act
		agency.Initialize();

		// Assert
		Assert.True(agency.CanSeeTaskBar);
		Assert.NotNull(agency.TakeTask);
		Assert.Empty(agency.TakeTask);
	}

	[Fact]
	public void ButtonProperty_GetterAndSetterWork()
	{
		// Arrange
		var agency = new Agency();
		var behavior = new NullBehaviour();
		var mockActivator = new Mock<IButtonAutoActivator>();
		var button = new ExtremeAbilityButton(behavior, mockActivator.Object, KeyCode.F);

		// Act
		agency.Button = button;

		// Assert
		Assert.Same(button, agency.Button);
	}

	[Fact]
	public void CreateAbility_SetsButton()
	{
		// Arrange
		var agency = new Agency();
		string spriteKey = $"{ObjectPath.AgencyTakeTask}115";
		if (!LruCache<string, Sprite>.TryGetValue(spriteKey, out _))
		{
			var mockSprite = new Mock<Sprite>(IntPtr.Zero);
			LruCache<string, Sprite>.Add(spriteKey, mockSprite.Object);
		}

		// Act
		agency.CreateAbility();

		// Assert
		Assert.NotNull(agency.Button);
	}

	[Fact]
	public void ResetOnMeetingStartAndEnd_ExecutesWithoutError()
	{
		// Arrange
		var agency = new Agency();

		// Act & Assert
		agency.ResetOnMeetingStart();
		agency.ResetOnMeetingEnd(null);
	}

	[Fact]
	public void IsAbilityUse_WhenNoTargetInRange_ReturnsFalse()
	{
		// Arrange
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.CanMove).Returns(true);

		var mockLocalData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockLocalData.SetupGet(d => d.Object).Returns(localPlayerMock.Object);
		mockLocalData.SetupGet(d => d.IsDead).Returns(false);
		mockLocalData.SetupGet(d => d.Disconnected).Returns(false);
		localPlayerMock.SetupGet(p => p.Data).Returns(mockLocalData.Object);

		var agency = new Agency();
		agency.CreateRoleAllOption();
		agency.Initialize();

		PlayerGetClosestPlayerInRangePatch.OverrideTarget = null;

		// Act
		bool isUse = agency.IsAbilityUse();

		// Assert
		Assert.False(isUse);
		Assert.Equal(byte.MaxValue, agency.TargetPlayer);
	}

	[Fact]
	public void IsAbilityUse_WhenTargetInRange_ReturnsTrueAndSetsTargetPlayer()
	{
		// Arrange
		byte targetId = 2;

		var agency = new Agency();
		agency.CreateRoleAllOption();
		agency.Initialize();

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.CanMove).Returns(true);

		var mockLocalData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockLocalData.SetupGet(d => d.Object).Returns(localPlayerMock.Object);
		mockLocalData.SetupGet(d => d.IsDead).Returns(false);
		mockLocalData.SetupGet(d => d.Disconnected).Returns(false);
		localPlayerMock.SetupGet(p => p.Data).Returns(mockLocalData.Object);

		var mockTargetPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetPlayer.SetupGet(p => p.PlayerId).Returns(targetId);

		PlayerGetClosestPlayerInRangePatch.OverrideTarget = mockTargetPlayer.Object;

		// Act
		bool isUse = agency.IsAbilityUse();

		// Assert
		Assert.True(isUse);
		Assert.Equal(targetId, agency.TargetPlayer);

		// Clean up
		PlayerGetClosestPlayerInRangePatch.OverrideTarget = null;
	}

	[Fact]
	public void TakeTargetPlayerTask_CompletesTasksAndRemovesTasks()
	{
		// Arrange
		byte targetPlayerId = 10;
		byte localPlayerId = 99;

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns(localPlayerId);

		var mockTask1 = new Mock<NormalPlayerTask>(IntPtr.Zero);
		mockTask1.SetupGet(t => t.Id).Returns(10u);
		var mockGameObject1 = new Mock<GameObject>(IntPtr.Zero);
		mockTask1.SetupGet(t => t.gameObject).Returns(mockGameObject1.Object);

		var mockTask2 = new Mock<NormalPlayerTask>(IntPtr.Zero);
		mockTask2.SetupGet(t => t.Id).Returns(20u);
		var mockGameObject2 = new Mock<GameObject>(IntPtr.Zero);
		mockTask2.SetupGet(t => t.gameObject).Returns(mockGameObject2.Object);

		var mockTaskList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTaskList.SetupGet(l => l.Count).Returns(2);
		mockTaskList.Setup(l => l[0]).Returns(mockTask1.Object);
		mockTaskList.Setup(l => l[1]).Returns(mockTask2.Object);

		var mockTargetPlayerControl = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetPlayerControl.SetupGet(p => p.PlayerId).Returns(targetPlayerId);
		mockTargetPlayerControl.SetupGet(p => p.myTasks).Returns(mockTaskList.Object);

		var mockTargetData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTargetPlayerControl.SetupGet(p => p.Data).Returns(mockTargetData.Object);

		PlayerCache.AddPlayerControl(localPlayerMock.Object);
		PlayerCache.AddPlayerControl(mockTargetPlayerControl.Object);

		var removeTaskIds = new List<int> { 10 };

		// Act
		Agency.TakeTargetPlayerTask(targetPlayerId, removeTaskIds);

		// Assert
		mockTargetPlayerControl.Verify(p => p.CompleteTask(10u), Times.Once);
		mockTargetPlayerControl.Verify(p => p.CompleteTask(20u), Times.Never);
		mockTask1.Verify(t => t.OnRemove(), Times.Once);
		mockTask2.Verify(t => t.OnRemove(), Times.Never);
		mockTargetData.Verify(d => d.MarkDirty(), Times.Once);
	}

	[Fact]
	public void Update_WhenNotInTaskPhaseOrNoTakeTask_DoesNothing()
	{
		// Arrange
		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
		GameProgressSystem.Current = GameProgressSystem.Progress.Meeting;

		var agency = new Agency();
		agency.CreateRoleAllOption();
		agency.Initialize();
		agency.TakeTask.Add(Agency.TakeTaskType.Normal);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		// Act
		agency.Update(mockPlayer.Object);

		// Assert
		Assert.Single(agency.TakeTask);
	}

	[Fact]
	public void Update_WhenInTaskPhaseWithCompletedTask_ReplacesTask()
	{
		// Arrange
		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;

		byte playerId = 5;
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

		var agency = new Agency();
		agency.CreateRoleAllOption();
		agency.Initialize();

		agency.TakeTask.Add(Agency.TakeTaskType.Normal);
		agency.TakeTask.Add(Agency.TakeTaskType.Long);
		agency.TakeTask.Add(Agency.TakeTaskType.Common);

		var mockTask1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		mockTask1.SetupGet(t => t.Complete).Returns(true);

		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(mockTask1.Object);

		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockPlayerInfo.SetupGet(p => p.Tasks).Returns(mockTasksList.Object);

		var mockGameData = GameData.Instance;
		Mock.Get(mockGameData).Setup(g => g.GetPlayerById(playerId)).Returns(mockPlayerInfo.Object);

		// Act
		agency.Update(mockPlayer.Object);

		// Assert
		Assert.Equal(2, agency.TakeTask.Count);
		Assert.Equal(Agency.TakeTaskType.Long, agency.TakeTask[0]);
	}

	[Fact]
	public void UseAbility_WhenTargetRoleHasNoTaskAndGaugeHigh_TakeNumZero_ReturnsTrue()
	{
		// Arrange
		byte targetId = 2;

		var agency = new Agency();
		agency.CreateRoleAllOption();
		agency.Initialize();
		agency.TargetPlayer = targetId;

		var targetRole = new SpecialCrew();
		targetRole.HasTask = false;

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole.Add(targetId, targetRole);

		var mockGameData = GameData.Instance;
		Mock.Get(mockGameData).SetupGet(g => g.TotalTasks).Returns(10);
		Mock.Get(mockGameData).SetupGet(g => g.CompletedTasks).Returns(10); // 100% completed > 0.9f

		// Act
		bool result = agency.UseAbility();

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void UseAbility_WhenTargetRoleHasTaskAndTasksAvailable_TransfersTasksAndReturnsTrue()
	{
		// Arrange
		byte targetId = 2;

		var agency = new Agency();
		agency.CreateRoleAllOption();
		agency.Initialize();
		agency.TargetPlayer = targetId;

		var targetRole = new SpecialCrew();
		targetRole.HasTask = true;

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole.Add(targetId, targetRole);

		var mockTask1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		mockTask1.SetupGet(t => t.Complete).Returns(false);
		mockTask1.SetupGet(t => t.TypeId).Returns((byte)1);
		mockTask1.SetupGet(t => t.Id).Returns(100u);

		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(mockTask1.Object);

		var mockTargetInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTargetInfo.SetupGet(p => p.Tasks).Returns(mockTasksList.Object);

		var mockGameData = GameData.Instance;
		Mock.Get(mockGameData).Setup(g => g.GetPlayerById(targetId)).Returns(mockTargetInfo.Object);

		var mockTargetPlayerControl = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetPlayerControl.SetupGet(p => p.PlayerId).Returns(targetId);
		var mockTargetData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTargetPlayerControl.SetupGet(p => p.Data).Returns(mockTargetData.Object);

		var mockPlayerTask = new Mock<NormalPlayerTask>(IntPtr.Zero);
		mockPlayerTask.SetupGet(t => t.Id).Returns(100u);
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockPlayerTask.SetupGet(t => t.gameObject).Returns(mockGameObject.Object);

		var mockTaskList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTaskList.SetupGet(l => l.Count).Returns(1);
		mockTaskList.Setup(l => l[0]).Returns(mockPlayerTask.Object);

		mockTargetPlayerControl.SetupGet(p => p.myTasks).Returns(mockTaskList.Object);

		PlayerCache.AddPlayerControl(mockTargetPlayerControl.Object);

		var mockAllList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerControl>>(IntPtr.Zero);
		mockAllList.SetupGet(l => l.Count).Returns(1);
		mockAllList.Setup(l => l[0]).Returns(mockTargetPlayerControl.Object);

		var mockAllHelper = new Mock<MockPlayerControlget_AllPlayerControlsHelper>();
		mockAllHelper.Setup(h => h.Invoke()).Returns(mockAllList.Object);
		MockPlayerControlget_AllPlayerControlsHelper.Instance = mockAllHelper.Object;

		// Act
		bool result = agency.UseAbility();

		// Assert
		Assert.True(result);
		Assert.Single(agency.TakeTask);
		Assert.Equal(Agency.TakeTaskType.Common, agency.TakeTask[0]);
		Assert.Equal(byte.MaxValue, agency.TargetPlayer);
	}
}
