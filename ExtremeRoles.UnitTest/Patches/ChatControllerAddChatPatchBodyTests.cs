using System;
using AmongUs.Data;
using AmongUs.Data.Settings;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Patches.Controller;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Patches;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ChatControllerAddChatPatchBodyTests : IDisposable
{
	private sealed class DummySingleRole : SingleRoleBase
	{
		private readonly Color _nameColor;
		private readonly Color _seeColor;

		public DummySingleRole(RoleCore core, Color nameColor, Color seeColor)
			: base(new RoleArgs(core, RoleProp.None))
		{
			_nameColor = nameColor;
			_seeColor = seeColor;
		}

		protected override void CreateSpecificOption(ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory factory) { }
		protected override void RoleSpecificInit() { }

		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
		public override Color GetNameColor(bool isDead) => _nameColor;
		public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => _seeColor;
	}

	public ChatControllerAddChatPatchBodyTests()
	{
		ResetState();
	}

	public void Dispose()
	{
		ResetState();
	}

	private static void ResetState()
	{
		PlayerCache.RemovePlayerControl(_ => true);
		MockPlayerControlget_LocalPlayerHelper.Instance = null;
		MockAmongUsClientget_InstanceHelper.Instance = null;
		MockMeetingHudget_InstanceHelper.Instance = null;

		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupOptionManager();
		MockSetupHelper.SetupTimeHelpers();
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var mockVectorOne = new Mock<MockVector3get_oneHelper>();
		mockVectorOne.Setup(x => x.Invoke()).Returns(new Vector3(1f, 1f, 1f));
		MockVector3get_oneHelper.Instance = mockVectorOne.Object;

		var mockIneq = new Mock<MockObjectop_InequalityHelper>();
		mockIneq.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<UnityEngine.Object>()))
			.Returns((UnityEngine.Object x, UnityEngine.Object y) => !ReferenceEquals(x, y));
		MockObjectop_InequalityHelper.Instance = mockIneq.Object;

		var mockEq = new Mock<MockObjectop_EqualityHelper>();
		mockEq.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<UnityEngine.Object>()))
			.Returns((UnityEngine.Object x, UnityEngine.Object y) => ReferenceEquals(x, y));
		MockObjectop_EqualityHelper.Instance = mockEq.Object;

		var mockImplicitBool = new Mock<MockObjectop_ImplicitHelper>();
		mockImplicitBool.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>())).Returns((UnityEngine.Object obj) => !ReferenceEquals(obj, null));
		MockObjectop_ImplicitHelper.Instance = mockImplicitBool.Object;

		var mockCensorWords = new Mock<MockBlockedWordsCensorWordsHelper>();
		mockCensorWords.Setup(x => x.Invoke(It.IsAny<string>(), It.IsAny<bool>())).Returns((string s, bool b) => s);
		MockBlockedWordsCensorWordsHelper.Instance = mockCensorWords.Object;

		if (Il2CppSystem.MockActionop_ImplicitHelper.Instance == null)
		{
			var mockActionImplicit = new Mock<Il2CppSystem.MockActionop_ImplicitHelper>();
			mockActionImplicit.Setup(x => x.Invoke(It.IsAny<Action>()))
				.Returns((Action act) => act != null ? new Il2CppSystem.Action(IntPtr.Zero) : null!);
			Il2CppSystem.MockActionop_ImplicitHelper.Instance = mockActionImplicit.Object;
		}

		if (Il2CppSystem.MockActionop_ImplicitHelper<float>.Instance == null)
		{
			var mockActionImplicitFloat = new Mock<Il2CppSystem.MockActionop_ImplicitHelper<float>>();
			mockActionImplicitFloat.Setup(x => x.Invoke(It.IsAny<Action<float>>()))
				.Returns((Action<float> act) => act != null ? new Il2CppSystem.Action<float>(IntPtr.Zero) : null!);
			Il2CppSystem.MockActionop_ImplicitHelper<float>.Instance = mockActionImplicitFloat.Object;
		}

		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(false);

		SetMeetingHudInstance(null);
		SetupDataManagerSettings(censorChat: false);
	}

	private static void SetMeetingHudInstance(MeetingHud? meetingHud)
	{
		var mockMeetingHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHelper.Setup(x => x.Invoke()).Returns(meetingHud!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHelper.Object;
	}

	private static void SetupDataManagerSettings(bool censorChat)
	{
		var mockMultiplayerSettings = new Mock<MultiplayerSettingsData>(IntPtr.Zero);
		mockMultiplayerSettings.SetupGet(m => m.CensorChat).Returns(censorChat);

		var mockSettings = new Mock<SettingsData>(IntPtr.Zero);
		mockSettings.SetupGet(s => s.Multiplayer).Returns(mockMultiplayerSettings.Object);

		var mockSettingsHelper = new Mock<MockDataManagerget_SettingsHelper>();
		mockSettingsHelper.Setup(x => x.Invoke()).Returns(mockSettings.Object);
		MockDataManagerget_SettingsHelper.Instance = mockSettingsHelper.Object;
	}

	private static void SetLocalPlayer(PlayerControl? localPlayer)
	{
		var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
		mockLocalHelper.Setup(h => h.Invoke()).Returns(localPlayer!);
		MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;
	}

	private static (Mock<PlayerControl> mockPlayer, Mock<NetworkedPlayerInfo> mockData) CreateMockPlayer(byte playerId, bool isDead = false)
	{
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		mockData.SetupGet(d => d.PlayerId).Returns(playerId);
		mockData.SetupGet(d => d.IsDead).Returns(isDead);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);

		PlayerCache.AddPlayerControl(mockPlayer.Object);

		return (mockPlayer, mockData);
	}

	private static (Mock<IGameProgress> mockProgress, Mock<IGameRuntime> mockRuntime, Mock<IGameContext> mockContext, Mock<INomalGameRoleContainer> mockRoles) SetupGameContext()
	{
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockContext = new Mock<IGameContext>();
		var mockRoles = new Mock<INomalGameRoleContainer>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		return (mockProgress, mockRuntime, mockContext, mockRoles);
	}

	private static void AddPlayerToTrash(MonikaTrashSystem system, byte playerId)
	{
		var mockReader = new Mock<MessageReader>();
		mockReader.Setup(r => r.ReadPackedInt32()).Returns(1);
		mockReader.Setup(r => r.ReadByte()).Returns(playerId);
		system.Deserialize(mockReader.Object, false);
	}

	private static (Mock<ChatController> mockChatController, Mock<ChatBubble> mockBubble) SetupChatControllerMocks()
	{
		var mockChatController = new Mock<ChatController>(IntPtr.Zero);
		var mockBubble = new Mock<ChatBubble>(IntPtr.Zero);
		var mockBubbleTransform = new Mock<Transform>(IntPtr.Zero);
		mockBubble.SetupGet(b => b.transform).Returns(mockBubbleTransform.Object);

		mockChatController.Setup(c => c.GetPooledBubble()).Returns(mockBubble.Object);

		var mockScroller = new Mock<Scroller>(IntPtr.Zero);
		var mockInnerTransform = new Mock<Transform>(IntPtr.Zero);
		mockScroller.SetupGet(s => s.Inner).Returns(mockInnerTransform.Object);
		mockChatController.SetupGet(c => c.scroller).Returns(mockScroller.Object);

		var mockChatBubblePool = new Mock<ObjectPoolBehavior>(IntPtr.Zero);
		mockChatController.SetupGet(c => c.chatBubblePool).Returns(mockChatBubblePool.Object);

		var mockNotification = new Mock<ChatNotification>(IntPtr.Zero);
		mockChatController.SetupGet(c => c.chatNotification).Returns(mockNotification.Object);

		var mockSoundManager = new Mock<SoundManager>(IntPtr.Zero);
		var mockSoundInstanceHelper = new Mock<MockSoundManagerget_InstanceHelper>();
		mockSoundInstanceHelper.Setup(x => x.Invoke()).Returns(mockSoundManager.Object);
		MockSoundManagerget_InstanceHelper.Instance = mockSoundInstanceHelper.Object;

		var mockAudioSource = new Mock<AudioSource>(IntPtr.Zero);
		var mockAudioClip = new Mock<AudioClip>(IntPtr.Zero);
		mockChatController.SetupGet(c => c.messageSound).Returns(mockAudioClip.Object);

		mockSoundManager.Setup(s => s.PlaySound(It.IsAny<AudioClip>(), false, 1f)).Returns(mockAudioSource.Object);

		return (mockChatController, mockBubble);
	}

	[Fact]
	public void Prefix_WhenIsNotGameNow_ReturnsTrue()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockLogger = new Mock<IModLogger>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(false);

		var patchBody = new ChatControllerAddChatPatchBody(mockLogger.Object, mockProgress.Object, mockRuntime.Object);
		var mockChatController = new Mock<ChatController>(IntPtr.Zero);
		var (sourcePlayer, _) = CreateMockPlayer(1);

		// Act
		bool result = patchBody.Prefix(mockChatController.Object, sourcePlayer.Object, "hello", censor: false);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenTryGetGameContextFails_ReturnsTrue()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockLogger = new Mock<IModLogger>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);
		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var patchBody = new ChatControllerAddChatPatchBody(mockLogger.Object, mockProgress.Object, mockRuntime.Object);
		var mockChatController = new Mock<ChatController>(IntPtr.Zero);
		var (sourcePlayer, _) = CreateMockPlayer(1);

		// Act
		bool result = patchBody.Prefix(mockChatController.Object, sourcePlayer.Object, "hello", censor: false);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(true, false, true, false)]  // sourcePlayer is null
	[InlineData(false, true, true, false)]  // sourcePlayer.Data is null
	[InlineData(false, false, false, false)] // LocalPlayer is null
	[InlineData(false, false, true, true)]  // LocalPlayer.Data is null
	public void Prefix_WhenAnyPlayerOrDataIsNull_ReturnsFalse(bool sourceNull, bool sourceDataNull, bool localExists, bool localDataNull)
	{
		// Arrange
		var (mockProgress, mockRuntime, _, _) = SetupGameContext();
		var mockLogger = new Mock<IModLogger>();

		PlayerControl? sourcePlayer = null;
		if (!sourceNull)
		{
			var (src, srcData) = CreateMockPlayer(1);
			sourcePlayer = src.Object;
			if (sourceDataNull)
			{
				src.SetupGet(p => p.Data).Returns((NetworkedPlayerInfo)null!);
			}
		}

		if (localExists)
		{
			var (local, localData) = CreateMockPlayer(0);
			if (localDataNull)
			{
				local.SetupGet(p => p.Data).Returns((NetworkedPlayerInfo)null!);
			}
			SetLocalPlayer(local.Object);
		}
		else
		{
			SetLocalPlayer(null);
		}

		var patchBody = new ChatControllerAddChatPatchBody(mockLogger.Object, mockProgress.Object, mockRuntime.Object);
		var mockChatController = new Mock<ChatController>(IntPtr.Zero);

		// Act
		bool result = patchBody.Prefix(mockChatController.Object, sourcePlayer!, "hello", censor: false);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenLocalPlayerRoleNotFound_ReturnsTrue()
	{
		// Arrange
		var (mockProgress, mockRuntime, _, mockRoles) = SetupGameContext();
		var mockLogger = new Mock<IModLogger>();

		var (localPlayer, _) = CreateMockPlayer(0);
		var (sourcePlayer, _) = CreateMockPlayer(1);
		SetLocalPlayer(localPlayer.Object);

		SingleRoleBase? role = null;
		mockRoles.Setup(r => r.TryGetRole(0, out role)).Returns(false);
		var dummyRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white), Color.white, Color.white);
		SingleRoleBase? srcRole = dummyRole;
		mockRoles.Setup(r => r.TryGetRole(1, out srcRole)).Returns(true);

		var patchBody = new ChatControllerAddChatPatchBody(mockLogger.Object, mockProgress.Object, mockRuntime.Object);
		var mockChatController = new Mock<ChatController>(IntPtr.Zero);

		// Act
		bool result = patchBody.Prefix(mockChatController.Object, sourcePlayer.Object, "hello", censor: false);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenSourcePlayerRoleNotFound_ReturnsTrue()
	{
		// Arrange
		var (mockProgress, mockRuntime, _, mockRoles) = SetupGameContext();
		var mockLogger = new Mock<IModLogger>();

		var (localPlayer, _) = CreateMockPlayer(0);
		var (sourcePlayer, _) = CreateMockPlayer(1);
		SetLocalPlayer(localPlayer.Object);

		var dummyRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white), Color.white, Color.white);
		SingleRoleBase? localRole = dummyRole;
		mockRoles.Setup(r => r.TryGetRole(0, out localRole)).Returns(true);
		SingleRoleBase? srcRole = null;
		mockRoles.Setup(r => r.TryGetRole(1, out srcRole)).Returns(false);

		var patchBody = new ChatControllerAddChatPatchBody(mockLogger.Object, mockProgress.Object, mockRuntime.Object);
		var mockChatController = new Mock<ChatController>(IntPtr.Zero);

		// Act
		bool result = patchBody.Prefix(mockChatController.Object, sourcePlayer.Object, "hello", censor: false);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenSourceIsDeadAndLocalIsAliveAndNotOneMan_ReturnsFalse()
	{
		// Arrange
		var (mockProgress, mockRuntime, _, mockRoles) = SetupGameContext();
		var mockLogger = new Mock<IModLogger>();

		var (localPlayer, _) = CreateMockPlayer(0, isDead: false);
		var (sourcePlayer, _) = CreateMockPlayer(1, isDead: true);
		SetLocalPlayer(localPlayer.Object);

		var dummyRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white), Color.white, Color.white);
		SingleRoleBase? roleOut = dummyRole;
		mockRoles.Setup(r => r.TryGetRole(It.IsAny<byte>(), out roleOut)).Returns(true);

		var patchBody = new ChatControllerAddChatPatchBody(mockLogger.Object, mockProgress.Object, mockRuntime.Object);
		var mockChatController = new Mock<ChatController>(IntPtr.Zero);

		// Act
		bool result = patchBody.Prefix(mockChatController.Object, sourcePlayer.Object, "hello", censor: false);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenOneManActiveAndNotValidShowChatPlayerAndMonikaOff_ReturnsFalse()
	{
		// Arrange
		var (mockProgress, mockRuntime, _, mockRoles) = SetupGameContext();
		var mockLogger = new Mock<IModLogger>();

		var (localPlayer, _) = CreateMockPlayer(0, isDead: false);
		var (sourcePlayer, _) = CreateMockPlayer(1, isDead: false);
		var (callerPlayer, _) = CreateMockPlayer(2, isDead: false);
		SetLocalPlayer(localPlayer.Object);

		var dummyRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white), Color.white, Color.white);
		SingleRoleBase? roleOut = dummyRole;
		mockRoles.Setup(r => r.TryGetRole(It.IsAny<byte>(), out roleOut)).Returns(true);

		var mockOnemanMeeting = new Mock<IOnemanMeeting>();
		mockOnemanMeeting.Setup(m => m.IsValidShowChatPlayer(sourcePlayer.Object)).Returns(false);

		var onemanManager = OnemanMeetingSystemManager.CreateOrGet();
		var meetingField = typeof(OnemanMeetingSystemManager).GetField("meeting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		meetingField?.SetValue(onemanManager, mockOnemanMeeting.Object);

		var patchBody = new ChatControllerAddChatPatchBody(mockLogger.Object, mockProgress.Object, mockRuntime.Object);
		var mockChatController = new Mock<ChatController>(IntPtr.Zero);

		// Act
		bool result = patchBody.Prefix(mockChatController.Object, sourcePlayer.Object, "hello", censor: false);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenOneManActiveAndNotValidShowChatPlayerAndMonikaCannotChat_ReturnsFalse()
	{
		// Arrange
		var (mockProgress, mockRuntime, _, mockRoles) = SetupGameContext();
		var mockLogger = new Mock<IModLogger>();

		var (localPlayer, _) = CreateMockPlayer(0, isDead: false);
		var (sourcePlayer, _) = CreateMockPlayer(1, isDead: false);
		var (callerPlayer, _) = CreateMockPlayer(2, isDead: false);
		SetLocalPlayer(localPlayer.Object);

		var dummyRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white), Color.white, Color.white);
		SingleRoleBase? roleOut = dummyRole;
		mockRoles.Setup(r => r.TryGetRole(It.IsAny<byte>(), out roleOut)).Returns(true);

		var mockOnemanMeeting = new Mock<IOnemanMeeting>();
		mockOnemanMeeting.Setup(m => m.IsValidShowChatPlayer(sourcePlayer.Object)).Returns(false);

		var onemanManager = OnemanMeetingSystemManager.CreateOrGet();
		var meetingField = typeof(OnemanMeetingSystemManager).GetField("meeting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		meetingField?.SetValue(onemanManager, mockOnemanMeeting.Object);

		var monikaSystem = new MonikaTrashSystem(false);
		ExtremeSystemTypeManager.Instance.TryAdd(ExtremeSystemType.MonikaTrashSystem, monikaSystem);
		AddPlayerToTrash(monikaSystem, 1);

		var patchBody = new ChatControllerAddChatPatchBody(mockLogger.Object, mockProgress.Object, mockRuntime.Object);
		var mockChatController = new Mock<ChatController>(IntPtr.Zero);

		// Act
		bool result = patchBody.Prefix(mockChatController.Object, sourcePlayer.Object, "hello", censor: false);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenNotOneManAndMonikaActiveAndCannotChat_ReturnsFalse()
	{
		// Arrange
		var (mockProgress, mockRuntime, _, mockRoles) = SetupGameContext();
		var mockLogger = new Mock<IModLogger>();

		var (localPlayer, _) = CreateMockPlayer(0, isDead: false);
		var (sourcePlayer, _) = CreateMockPlayer(1, isDead: false);
		SetLocalPlayer(localPlayer.Object);

		var dummyRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white), Color.white, Color.white);
		SingleRoleBase? roleOut = dummyRole;
		mockRoles.Setup(r => r.TryGetRole(It.IsAny<byte>(), out roleOut)).Returns(true);

		var monikaSystem = new MonikaTrashSystem(false);
		ExtremeSystemTypeManager.Instance.TryAdd(ExtremeSystemType.MonikaTrashSystem, monikaSystem);
		AddPlayerToTrash(monikaSystem, 1);

		var patchBody = new ChatControllerAddChatPatchBody(mockLogger.Object, mockProgress.Object, mockRuntime.Object);
		var mockChatController = new Mock<ChatController>(IntPtr.Zero);

		// Act
		bool result = patchBody.Prefix(mockChatController.Object, sourcePlayer.Object, "hello", censor: false);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenExceptionInForceAddToChatBubble_LogsErrorAndReclaimsBubbleAndReturnsFalse()
	{
		// Arrange
		var (mockProgress, mockRuntime, _, mockRoles) = SetupGameContext();
		var mockLogger = new Mock<IModLogger>();

		var (localPlayer, _) = CreateMockPlayer(0, isDead: false);
		var (sourcePlayer, _) = CreateMockPlayer(1, isDead: false);
		SetLocalPlayer(localPlayer.Object);

		var dummyRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white), Color.white, Color.white);
		SingleRoleBase? roleOut = dummyRole;
		mockRoles.Setup(r => r.TryGetRole(It.IsAny<byte>(), out roleOut)).Returns(true);

		var (mockChatController, mockBubble) = SetupChatControllerMocks();
		mockBubble.Setup(b => b.SetRight()).Throws(new InvalidOperationException("Test Exception"));

		var patchBody = new ChatControllerAddChatPatchBody(mockLogger.Object, mockProgress.Object, mockRuntime.Object);

		// Act
		bool result = patchBody.Prefix(mockChatController.Object, localPlayer.Object, "hello", censor: false);

		// Assert
		Assert.False(result);
		mockLogger.Verify(l => l.LogError(It.IsAny<InvalidOperationException>()), Times.Once);
		Mock.Get(mockChatController.Object.chatBubblePool).Verify(p => p.Reclaim(mockBubble.Object), Times.Once);
	}

	[Fact]
	public void Prefix_WhenSamePlayer_ConfiguresRightBubbleAndReturnsFalse()
	{
		// Arrange
		var (mockProgress, mockRuntime, _, mockRoles) = SetupGameContext();
		var mockLogger = new Mock<IModLogger>();

		var (localPlayer, localData) = CreateMockPlayer(0, isDead: false);
		SetLocalPlayer(localPlayer.Object);

		Color roleNameColor = Color.red;
		var dummyRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white), roleNameColor, Color.blue);
		SingleRoleBase? roleOut = dummyRole;
		mockRoles.Setup(r => r.TryGetRole(0, out roleOut)).Returns(true);

		var mockMeetingHud = new Mock<MeetingHud>(IntPtr.Zero);
		mockMeetingHud.Setup(m => m.DidVote(0)).Returns(true);
		SetMeetingHudInstance(mockMeetingHud.Object);

		var (mockChatController, mockBubble) = SetupChatControllerMocks();
		mockChatController.SetupGet(c => c.IsOpenOrOpening).Returns(true);

		var patchBody = new ChatControllerAddChatPatchBody(mockLogger.Object, mockProgress.Object, mockRuntime.Object);

		// Act
		bool result = patchBody.Prefix(mockChatController.Object, localPlayer.Object, "my text", censor: false);

		// Assert
		Assert.False(result);
		mockBubble.Verify(b => b.SetRight(), Times.Once);
		mockBubble.Verify(b => b.SetLeft(), Times.Never);
		mockBubble.Verify(b => b.SetCosmetics(localData.Object), Times.Once);
		mockChatController.Verify(c => c.SetChatBubbleName(mockBubble.Object, localData.Object, false, true, roleNameColor, null), Times.Once);
		mockBubble.Verify(b => b.SetText("my text"), Times.Once);
		mockBubble.Verify(b => b.AlignChildren(), Times.Once);
		mockChatController.Verify(c => c.AlignAllBubbles(), Times.Once);
	}

	[Fact]
	public void Prefix_WhenDifferentPlayerAndNotOpen_ConfiguresLeftBubbleAndSoundAndNotificationAndReturnsFalse()
	{
		// Arrange
		var (mockProgress, mockRuntime, _, mockRoles) = SetupGameContext();
		var mockLogger = new Mock<IModLogger>();

		var (localPlayer, localData) = CreateMockPlayer(0, isDead: false);
		var (sourcePlayer, sourceData) = CreateMockPlayer(3, isDead: false);
		SetLocalPlayer(localPlayer.Object);

		Color seeColor = Color.green;
		var dummyRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white), Color.white, seeColor);
		SingleRoleBase? roleOut = dummyRole;
		mockRoles.Setup(r => r.TryGetRole(It.IsAny<byte>(), out roleOut)).Returns(true);

		var mockMeetingHud = new Mock<MeetingHud>(IntPtr.Zero);
		mockMeetingHud.Setup(m => m.DidVote(3)).Returns(false);
		SetMeetingHudInstance(mockMeetingHud.Object);

		var (mockChatController, mockBubble) = SetupChatControllerMocks();
		mockChatController.SetupGet(c => c.IsOpenOrOpening).Returns(false);
		mockChatController.SetupGet(c => c.notificationRoutine).Returns((UnityEngine.Coroutine)null!);

		var patchBody = new ChatControllerAddChatPatchBody(mockLogger.Object, mockProgress.Object, mockRuntime.Object);

		// Act
		bool result = patchBody.Prefix(mockChatController.Object, sourcePlayer.Object, "other text", censor: false);

		// Assert
		Assert.False(result);
		mockBubble.Verify(b => b.SetLeft(), Times.Once);
		mockBubble.Verify(b => b.SetRight(), Times.Never);
		mockBubble.Verify(b => b.SetCosmetics(sourceData.Object), Times.Once);
		mockChatController.Verify(c => c.SetChatBubbleName(mockBubble.Object, sourceData.Object, false, false, seeColor, null), Times.Once);
		mockBubble.Verify(b => b.SetText("other text"), Times.Once);
		Mock.Get(mockChatController.Object.chatNotification).Verify(n => n.SetUp(sourcePlayer.Object, "other text"), Times.Once);
	}

	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	[InlineData(false, false)]
	public void Prefix_CensorChatSetting_ConfiguresBubbleText(bool censorArg, bool censorSetting)
	{
		// Arrange
		SetupDataManagerSettings(censorChat: censorSetting);

		var (mockProgress, mockRuntime, _, mockRoles) = SetupGameContext();
		var mockLogger = new Mock<IModLogger>();

		var (localPlayer, _) = CreateMockPlayer(0, isDead: false);
		SetLocalPlayer(localPlayer.Object);

		var dummyRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white), Color.white, Color.white);
		SingleRoleBase? roleOut = dummyRole;
		mockRoles.Setup(r => r.TryGetRole(0, out roleOut)).Returns(true);

		var (mockChatController, mockBubble) = SetupChatControllerMocks();
		mockChatController.SetupGet(c => c.IsOpenOrOpening).Returns(true);

		var patchBody = new ChatControllerAddChatPatchBody(mockLogger.Object, mockProgress.Object, mockRuntime.Object);

		string inputChatText = "hello";

		// Act
		bool result = patchBody.Prefix(mockChatController.Object, localPlayer.Object, inputChatText, censor: censorArg);

		// Assert
		Assert.False(result);
		mockBubble.Verify(b => b.SetText(It.IsAny<string>()), Times.Once);
	}
}
