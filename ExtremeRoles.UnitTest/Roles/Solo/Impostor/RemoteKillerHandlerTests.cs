using System;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class RemoteKillerHandlerTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public RemoteKillerHandlerTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		mockClient = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		SetupTranslationControllerMock();
		SetupSpriteCacheMock();
		SetupPlayerHasTaskMock();
		SetupMinigameMock(null);
		SetupCommonSingletonsMock();
		SetupHudManagerMock();

		mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockPhysics = new Mock<PlayerPhysics>(IntPtr.Zero);
		mockPhysics.SetupGet(p => p.DoingCustomAnimation).Returns(false);
		mockLocalPlayer.SetupGet(p => p.MyPhysics).Returns(mockPhysics.Object);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockLocalPlayer.Object);
		mockLocalPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);

		var emptyTasks = new Il2CppSystem.Collections.Generic.List<PlayerTask>();
		mockLocalPlayer.SetupGet(p => p.myTasks).Returns(emptyTasks);

		MockSetupHelper.SetupGameDataMock();
	}

	private static Mock<PlayerControl> SetupMockTargetPlayer(byte playerId)
	{
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

		var mockPhysics = new Mock<PlayerPhysics>(IntPtr.Zero);
		mockPhysics.SetupGet(p => p.DoingCustomAnimation).Returns(false);
		mockPlayer.SetupGet(p => p.MyPhysics).Returns(mockPhysics.Object);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		mockPlayer.SetupGet(p => p.CanMove).Returns(true);

		return mockPlayer;
	}

	private static void SetupTranslationControllerMock()
	{
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
	}

	private static void SetupSpriteCacheMock()
	{
		var mockSprite = new Mock<Sprite>(IntPtr.Zero);

		string suicideKey = $"{ObjectPath.SucideSprite}115";
		if (!LruCache<string, Sprite>.TryGetValue(suicideKey, out _))
		{
			LruCache<string, Sprite>.Add(suicideKey, mockSprite.Object);
		}

		string umbrerKey = $"{ObjectPath.UmbrerFeatVirus}115";
		if (!LruCache<string, Sprite>.TryGetValue(umbrerKey, out _))
		{
			LruCache<string, Sprite>.Add(umbrerKey, mockSprite.Object);
		}
	}

	private static void SetupPlayerHasTaskMock()
	{
		var mockHasTask = new Mock<MockPlayerTaskPlayerHasTaskOfTypeHelper>();
		mockHasTask.Setup(x => x.Invoke<IHudOverrideTask>(It.IsAny<PlayerControl>())).Returns(false);
		MockPlayerTaskPlayerHasTaskOfTypeHelper.Instance = mockHasTask.Object;
	}

	private static void SetupMinigameMock(Minigame? minigame)
	{
		var mockMinigameHelper = new Mock<MockMinigameget_InstanceHelper>();
		mockMinigameHelper.Setup(x => x.Invoke()).Returns(minigame!);
		MockMinigameget_InstanceHelper.Instance = mockMinigameHelper.Object;
	}

	private static void SetupCommonSingletonsMock()
	{
		var mockMeetingHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHelper.Object;

		var mockCustomizationHelper = new Mock<MockPlayerCustomizationMenuget_InstanceHelper>();
		mockCustomizationHelper.Setup(x => x.Invoke()).Returns((PlayerCustomizationMenu)null!);
		MockPlayerCustomizationMenuget_InstanceHelper.Instance = mockCustomizationHelper.Object;

		var mockExileHelper = new Mock<MockExileControllerget_InstanceHelper>();
		mockExileHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockExileControllerget_InstanceHelper.Instance = mockExileHelper.Object;

		var mockIntroHelper = new Mock<MockIntroCutsceneget_InstanceHelper>();
		mockIntroHelper.Setup(x => x.Invoke()).Returns((IntroCutscene)null!);
		MockIntroCutsceneget_InstanceHelper.Instance = mockIntroHelper.Object;

		var mockMapHelper = new Mock<MockMapBehaviourget_InstanceHelper>();
		mockMapHelper.Setup(x => x.Invoke()).Returns((MapBehaviour)null!);
		MockMapBehaviourget_InstanceHelper.Instance = mockMapHelper.Object;
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

		var mockChat = new Mock<ChatController>(IntPtr.Zero);
		mockChat.SetupGet(c => c.IsOpenOrOpening).Returns(false);

		var mockKillOverlay = new Mock<KillOverlay>(IntPtr.Zero);
		mockKillOverlay.SetupGet(k => k.IsOpen).Returns(false);

		var mockGameMenu = new Mock<OptionsMenuBehaviour>(IntPtr.Zero);
		mockGameMenu.SetupGet(g => g.IsOpen).Returns(false);

		var mockHud = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		mockHud.SetupGet(h => h.Chat).Returns(mockChat.Object);
		mockHud.SetupGet(h => h.KillOverlay).Returns(mockKillOverlay.Object);
		mockHud.SetupGet(h => h.GameMenu).Returns(mockGameMenu.Object);
		mockHud.SetupGet(h => h.IsIntroDisplayed).Returns(false);
		mockHud.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
	}

	[Fact]
	public void RobHandler_CreateBehavior_ConfiguresActiveTimeCorrectly()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 1);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerRobHandler(status, role);

		// Act
		var behavior = handler.CreateBehavior(30.0f, 2.5f);

		// Assert
		Assert.NotNull(behavior);
		Assert.Equal(2.5f, behavior.ActiveTime);
	}

	[Fact]
	public void RobHandler_IsUseRob_WhenNoPlayerInRange_ReturnsFalse()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 1);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerRobHandler(status, role);

		// Act
		bool canUse = handler.IsUseRob();

		// Assert
		Assert.False(canUse);
	}

	[Fact]
	public void RobHandler_IsRobCheck_WhenCurrentTargetIsNull_ReturnsFalse()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 1);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerRobHandler(status, role);

		// Act
		bool isChecking = handler.IsRobCheck();

		// Assert
		Assert.False(isChecking);
	}

	[Fact]
	public void RobHandler_RobCleanUp_AddsTargetToExecutionTargets()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 1);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerRobHandler(status, role);

		var mockTarget = new Mock<PlayerControl>(IntPtr.Zero);
		mockTarget.SetupGet(p => p.PlayerId).Returns((byte)2);

		var field = typeof(RemoteKillerRobHandler).GetField("currentRobTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field?.SetValue(handler, mockTarget.Object);

		// Act
		handler.RobCleanUp();

		// Assert
		Assert.True(status.HasExecutionTarget(2));
	}

	[Fact]
	public void RobHandler_RobForceCleanUp_ClearsTarget()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 1);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerRobHandler(status, role);

		var mockTarget = new Mock<PlayerControl>(IntPtr.Zero);
		var field = typeof(RemoteKillerRobHandler).GetField("currentRobTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field?.SetValue(handler, mockTarget.Object);

		// Act
		handler.RobForceCleanUp();

		// Assert
		var curTarget = field?.GetValue(handler);
		Assert.Null(curTarget);
	}

	[Fact]
	public void RobHandler_RecordTargetContacts_WhenTargetNullOrDead_ReturnsEarly()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 1);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerRobHandler(status, role);

		// Act & Assert (target 99 does not exist)
		handler.RecordTargetContacts(99);
	}

	[Fact]
	public void PurgeHandler_CreateBehavior_ConfiguresActiveTimeAndCountCorrectly()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 1);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerPurgeHandler(status, role);

		// Act
		var behavior = handler.CreateBehavior(30.0f, 4.5f, 3);

		// Assert
		Assert.NotNull(behavior);
		Assert.Equal(4.5f, behavior.ActiveTime);
		Assert.Equal(3, behavior.AbilityCount);
	}

	[Fact]
	public void PurgeHandler_IsUsePurgeCheck_WhenCharging_CallsIsAbilityUseWithMinigame()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 1);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerPurgeHandler(status, role);

		// Act
		bool canUseCharging = handler.IsUsePurgeCheck(true, 0f);

		// Assert
		Assert.True(canUseCharging);
	}

	[Fact]
	public void PurgeHandler_IsPurgeCheck_WhenTargetNull_ReturnsFalse()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 1);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerPurgeHandler(status, role);

		// Act
		bool check = handler.IsPurgeCheck();

		// Assert
		Assert.False(check);
	}

	[Fact]
	public void PurgeHandler_PurgeStartAbility_WhenTargetNull_ReturnsFalse()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 1);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerPurgeHandler(status, role);

		// Act
		bool result = handler.PurgeStartAbility(0f);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void PurgeHandler_PurgeStartAbility_WhenTargetSelected_TriggersRpcAndReturnsTrue()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 1);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerPurgeHandler(status, role);

		var mockTarget = SetupMockTargetPlayer((byte)2);

		var field = typeof(RemoteKillerPurgeHandler).GetField("selectedPurgeTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field?.SetValue(handler, mockTarget.Object);

		mockClient.Invocations.Clear();

		// Act
		bool result = handler.PurgeStartAbility(0f);

		// Assert
		Assert.True(result);
		mockClient.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.Once);
	}

	[Fact]
	public void PurgeHandler_PurgeCleanUp_RemovesExecutionTargetFromStatus()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 1);
		status.AddExecutionTarget(2, 1);

		var role = new RemoteKillerRole();
		var handler = new RemoteKillerPurgeHandler(status, role);

		var mockTarget = SetupMockTargetPlayer((byte)2);

		var field = typeof(RemoteKillerPurgeHandler).GetField("selectedPurgeTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field?.SetValue(handler, mockTarget.Object);

		mockClient.Invocations.Clear();

		// Act
		handler.PurgeCleanUp();

		// Assert
		Assert.False(status.HasExecutionTarget(2));
		mockClient.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.AtLeastOnce);
	}

	[Fact]
	public void PurgeHandler_PurgeForceCleanUp_WhenPurging_SendsRpcAndResetsTarget()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 1);
		status.SetPurging(true);

		var role = new RemoteKillerRole();
		var handler = new RemoteKillerPurgeHandler(status, role);

		mockClient.Invocations.Clear();

		// Act
		handler.PurgeForceCleanUp();

		// Assert
		mockClient.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.Once);
	}
}
