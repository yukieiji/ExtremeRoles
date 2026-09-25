using System;
using System.Reflection;
using AmongUs.GameOptions;
using InnerNet;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Bait;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class BaitRoleTests
{
	private readonly Mock<AmongUsClient> clientMock;

	public BaitRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupPlayerControlMocks();
		SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		var mockActionImplicit = new Mock<Il2CppSystem.MockActionop_ImplicitHelper<float>>();
		mockActionImplicit.Setup(x => x.Invoke(It.IsAny<Action<float>>()))
			.Returns((Action<float> act) => act != null ? new Il2CppSystem.Action<float>(IntPtr.Zero) : null!);
		Il2CppSystem.MockActionop_ImplicitHelper<float>.Instance = mockActionImplicit.Object;

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		var mockDestroyableMeetingHelper = new Mock<MockDestroyableSingletonget_InstanceHelper<MeetingHud>>();
		mockDestroyableMeetingHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockDestroyableSingletonget_InstanceHelper<MeetingHud>.Instance = mockDestroyableMeetingHelper.Object;

		SetLobbyMode(false);
	}

	private static void SetupGameOptionsManagerMock()
	{
		if (MockGameOptionsManagerget_InstanceHelper.Instance == null)
		{
			var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
			var mockGameOptionsManager = new Mock<GameOptionsManager>(IntPtr.Zero);
			mockGameOptionsManager.SetupGet(g => g.CurrentGameOptions).Returns(mockGameOptions.Object);

			var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
			mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockGameOptionsManager.Object);
			MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
		}
	}

	private void SetLobbyMode(bool isLobby)
	{
		if (isLobby)
		{
			clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.NotJoined);
			var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
		else
		{
			clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns((LobbyBehaviour)null!);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
	}

	private static void InitializeBaitRole(ExtremeRoles.Roles.Solo.Crewmate.Bait role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[playerId] = role;
	}

	private static void SetupFullScreenFlasherMocks(Mock<HudManager> hudMock)
	{
		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		var mockRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);

		mockRenderer.SetupGet(r => r.transform).Returns(mockTransform.Object);
		mockRenderer.SetupGet(r => r.gameObject).Returns(mockGameObject.Object);
		mockRenderer.SetupProperty(r => r.enabled);

		hudMock.SetupGet(h => h.FullScreen).Returns(mockRenderer.Object);
		hudMock.SetupGet(h => h.transform).Returns(mockTransform.Object);

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		var mockLerpHelper = new Mock<MockEffectsLerpHelper>();
		mockLerpHelper.Setup(x => x.Invoke(It.IsAny<float>(), It.IsAny<Il2CppSystem.Action<float>>()))
			.Returns((Il2CppSystem.Collections.IEnumerator)null!);
		MockEffectsLerpHelper.Instance = mockLerpHelper.Object;
	}

	[Fact]
	public void Constructor_InitializesCorrectly()
	{
		// Act
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();

		// Assert
		Assert.NotNull(role);
		Assert.Equal(ExtremeRoleId.Bait, role.Core.Id);
		Assert.Equal(RoleTypes.Crewmate, role.NoneAwakeRole);
	}

	[Theory]
	[InlineData(true, false, true)]
	[InlineData(false, true, true)]
	[InlineData(false, false, false)]
	public void IsAwake_ReturnsExpectedValue(bool isLobby, bool awakeRoleState, bool expectedIsAwake)
	{
		// Arrange
		SetLobbyMode(isLobby);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = awakeRoleState ? 0 : 7; // 0% -> awakeRole = true, 70% -> awakeRole = false
		}

		role.Initialize();

		// Act
		bool isAwake = role.IsAwake;

		// Assert
		Assert.Equal(expectedIsAwake, isAwake);
	}

	[Fact]
	public void Awake_WhenRoleExists_SetsAwakeRoleToTrue()
	{
		// Arrange
		byte playerId = 1;
		SetLobbyMode(false);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		InitializeBaitRole(role, playerId);

		Assert.False(role.IsAwake);

		// Act
		ExtremeRoles.Roles.Solo.Crewmate.Bait.Awake(playerId);

		// Assert
		Assert.True(role.IsAwake);
	}

	[Fact]
	public void RoleSpecificInit_WhenAwakeTaskGageIsZeroOrLess_SetsAwakeRoleTrue()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 0; // 0%
		}

		// Act
		role.Initialize();

		// Assert
		Assert.True(role.IsAwake);
	}

	[Fact]
	public void RoleSpecificInit_WhenAwakeTaskGageIsGreaterThanZero_SetsAwakeRoleFalse()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		// Act
		role.Initialize();

		// Assert
		Assert.False(role.IsAwake);
	}

	[Fact]
	public void Update_WhenAlreadyAwake_DoesNotSendRpc()
	{
		// Arrange
		SetLobbyMode(false);
		clientMock.Invocations.Clear();

		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 0; // 0% -> awakeRole = true
		}

		role.Initialize();

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		// Act
		role.Update(mockPlayer.Object);

		// Assert
		clientMock.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.Never());
	}

	[Fact]
	public void Update_WhenTaskGageNotReached_DoesNotAwake()
	{
		// Arrange
		SetLobbyMode(false);
		clientMock.Invocations.Clear();

		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

		var task1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		task1.SetupGet(t => t.Complete).Returns(true);

		var task2 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		task2.SetupGet(t => t.Complete).Returns(false);

		mockTasksList.SetupGet(l => l.Count).Returns(2);
		mockTasksList.Setup(l => l[0]).Returns(task1.Object);
		mockTasksList.Setup(l => l[1]).Returns(task2.Object);

		mockPlayerInfo.SetupGet(p => p.Tasks).Returns(mockTasksList.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockPlayerInfo.Object);

		// Task gage = 1/2 = 50% < 70%

		// Act
		role.Update(mockPlayer.Object);

		// Assert
		Assert.False(role.IsAwake);
		clientMock.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.Never());
	}

	[Fact]
	public void Update_WhenTaskGageReached_AwakesAndSendsRpc()
	{
		// Arrange
		SetLobbyMode(false);
		clientMock.Invocations.Clear();

		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 5; // 50%
		}

		role.Initialize();

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

		var task1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		task1.SetupGet(t => t.Complete).Returns(true);

		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(task1.Object);

		mockPlayerInfo.SetupGet(p => p.Tasks).Returns(mockTasksList.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockPlayerInfo.Object);

		// Task gage = 1/1 = 100% >= 50%

		// Act
		role.Update(mockPlayer.Object);

		// Assert
		Assert.True(role.IsAwake);
		clientMock.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), (byte)RPCOperator.Command.BaitAwakeRole, It.IsAny<SendOption>(), It.IsAny<int>()), Times.Once());
	}

	[Fact]
	public void GetFakeOptionString_ReturnsEmptyString()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();

		// Act
		string result = role.GetFakeOptionString();

		// Assert
		Assert.Equal("", result);
	}

	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	public void GetColoredRoleName_WhenAwakeOrTruthColor_ReturnsColoredRoleName(bool isTruthColor, bool isAwake)
	{
		// Arrange
		SetLobbyMode(isAwake);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();

		// Act
		string roleName = role.GetColoredRoleName(isTruthColor);

		// Assert
		Assert.NotNull(roleName);
		Assert.Contains("Bait", roleName);
	}

	[Fact]
	public void GetColoredRoleName_WhenNotAwakeAndNotTruthColor_ReturnsWhiteCrewmateName()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70% -> false
		}

		role.Initialize();

		// Act
		string roleName = role.GetColoredRoleName(false);

		// Assert
		Assert.NotNull(roleName);
		Assert.Contains("Crewmate", roleName);
	}

	[Fact]
	public void GetFullDescription_WhenAwake_ReturnsBaitFullDescription()
	{
		// Arrange
		SetLobbyMode(true);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();

		// Act
		string description = role.GetFullDescription();

		// Assert
		Assert.NotNull(description);
		Assert.Equal("BaitFullDescription", description);
	}

	[Fact]
	public void GetFullDescription_WhenNotAwake_ReturnsCrewmateFullDescription()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		string description = role.GetFullDescription();

		// Assert
		Assert.NotNull(description);
		Assert.Equal("CrewmateFullDescription", description);
	}

	[Fact]
	public void GetImportantText_WhenAwake_ReturnsBaseImportantText()
	{
		// Arrange
		SetLobbyMode(true);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();

		// Act
		string text = role.GetImportantText(true);

		// Assert
		Assert.NotNull(text);
		Assert.Contains("Bait", text);
	}

	[Fact]
	public void GetImportantText_WhenNotAwake_ReturnsCrewmateImportantText()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		string text = role.GetImportantText(true);

		// Assert
		Assert.NotNull(text);
		Assert.Contains("crewImportantText", text);
	}

	[Fact]
	public void GetIntroDescription_WhenAwake_ReturnsBaseIntroDescription()
	{
		// Arrange
		SetLobbyMode(true);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();

		// Act
		string desc = role.GetIntroDescription();

		// Assert
		Assert.NotNull(desc);
		Assert.Contains("BaitIntroDescription", desc);
	}

	[Fact]
	public void GetIntroDescription_WhenNotAwake_ReturnsCrewmateIntroDescription()
	{
		// Arrange
		SetLobbyMode(false);
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();

		var mockRole = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRole.SetupGet(r => r.Blurb).Returns("Crewmate Intro Blurb");

		var mockInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockInfo.SetupGet(i => i.Role).Returns(mockRole.Object);
		localPlayerMock.SetupGet(p => p.Data).Returns(mockInfo.Object);

		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		string desc = role.GetIntroDescription();

		// Assert
		Assert.NotNull(desc);
		Assert.Contains("Crewmate Intro Blurb", desc);
	}

	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	public void GetNameColor_WhenAwakeOrTruthColor_ReturnsRoleColor(bool isTruthColor, bool isAwake)
	{
		// Arrange
		SetLobbyMode(isAwake);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();

		// Act
		Color color = role.GetNameColor(isTruthColor);

		// Assert
		Assert.Equal(ColorPalette.BaitCyan, color);
	}

	[Fact]
	public void GetNameColor_WhenNotAwakeAndNotTruthColor_ReturnsWhite()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		Color color = role.GetNameColor(false);

		// Assert
		Assert.Equal(Palette.White, color);
	}

	[Fact]
	public void RolePlayerKilledAction_WhenNotAwake_DoesNothing()
	{
		// Arrange
		SetLobbyMode(false);

		var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

		var rolePlayerMock = new Mock<PlayerControl>(IntPtr.Zero);
		var killerPlayerMock = new Mock<PlayerControl>(IntPtr.Zero);

		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70% -> false
		}

		role.Initialize();

		// Act
		role.RolePlayerKilledAction(rolePlayerMock.Object, killerPlayerMock.Object);

		// Assert
		hudMock.VerifyGet(h => h.gameObject, Times.Never());
	}

	[Fact]
	public void RolePlayerKilledAction_WhenMeetingHudIsNotNull_DoesNothing()
	{
		// Arrange
		SetLobbyMode(true);

		var mockMeeting = new Mock<MeetingHud>(IntPtr.Zero);
		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns(mockMeeting.Object);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

		var rolePlayerMock = new Mock<PlayerControl>(IntPtr.Zero);
		var killerPlayerMock = new Mock<PlayerControl>(IntPtr.Zero);

		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		role.CreateRoleAllOption();
		role.Initialize();

		// Act
		role.RolePlayerKilledAction(rolePlayerMock.Object, killerPlayerMock.Object);

		// Assert
		hudMock.VerifyGet(h => h.gameObject, Times.Never());
	}

	[Fact]
	public void RolePlayerKilledAction_WhenAwakeAndLocalPlayerIsKiller_AddsBaitDelayReporterAndReducer()
	{
		// Arrange
		SetLobbyMode(true);

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		byte killerId = 1;
		byte rolePlayerId = 2;

		var reporter = new BaitDelayReporter(IntPtr.Zero);
		var reducer = new BaitKillCoolReducer(IntPtr.Zero);

		var mockLocalGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockLocalGameObject.Setup(g => g.AddComponent<BaitKillCoolReducer>()).Returns(reducer);

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(killerId);
		mockLocalPlayer.SetupGet(p => p.gameObject).Returns(mockLocalGameObject.Object);

		var mockHudGameObject = new Mock<GameObject>(IntPtr.Zero);
		var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		hudMock.SetupGet(h => h.gameObject).Returns(mockHudGameObject.Object);

		SetupFullScreenFlasherMocks(hudMock);

		mockHudGameObject.Setup(g => g.AddComponent<BaitDelayReporter>()).Returns(reporter);

		var rolePlayerMock = new Mock<PlayerControl>(IntPtr.Zero);
		rolePlayerMock.SetupGet(p => p.PlayerId).Returns(rolePlayerId);

		var killerPlayerMock = new Mock<PlayerControl>(IntPtr.Zero);
		killerPlayerMock.SetupGet(p => p.PlayerId).Returns(killerId);

		var baitRole = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		baitRole.CreateRoleAllOption();

		if (baitRole.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.DelayUntilForceReport, out var delayOption) && delayOption != null)
		{
			delayOption.Selection = 0; // 0.0s so it does not start coroutine
		}

		baitRole.Initialize();

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[rolePlayerId] = baitRole;

		var killerRole = new SpecialImpostor();
		killerRole.CreateRoleAllOption();
		killerRole.Initialize();
		ExtremeRoleManager.GameRole[killerId] = killerRole;

		// Act
		baitRole.RolePlayerKilledAction(rolePlayerMock.Object, killerPlayerMock.Object);

		// Assert
		mockHudGameObject.Verify(g => g.AddComponent<BaitDelayReporter>(), Times.Once());
		mockLocalGameObject.Verify(g => g.AddComponent<BaitKillCoolReducer>(), Times.Once());
	}

	[Fact]
	public void RolePlayerKilledAction_WhenLocalPlayerIsNotKiller_DoesNotAddBaitDelayReporter()
	{
		// Arrange
		SetLobbyMode(true);

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		byte localId = 1;
		byte killerId = 2;
		byte rolePlayerId = 3;

		var mockLocalGameObject = new Mock<GameObject>(IntPtr.Zero);

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(localId);
		mockLocalPlayer.SetupGet(p => p.gameObject).Returns(mockLocalGameObject.Object);

		var mockHudGameObject = new Mock<GameObject>(IntPtr.Zero);
		var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		hudMock.SetupGet(h => h.gameObject).Returns(mockHudGameObject.Object);

		var rolePlayerMock = new Mock<PlayerControl>(IntPtr.Zero);
		rolePlayerMock.SetupGet(p => p.PlayerId).Returns(rolePlayerId);

		var killerPlayerMock = new Mock<PlayerControl>(IntPtr.Zero);
		killerPlayerMock.SetupGet(p => p.PlayerId).Returns(killerId);

		var baitRole = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		InitializeBaitRole(baitRole, rolePlayerId);

		var localRole = new SpecialCrew();
		localRole.CreateRoleAllOption();
		localRole.Initialize();
		ExtremeRoleManager.GameRole[localId] = localRole;

		// Act
		baitRole.RolePlayerKilledAction(rolePlayerMock.Object, killerPlayerMock.Object);

		// Assert
		mockHudGameObject.Verify(g => g.AddComponent<BaitDelayReporter>(), Times.Never());
		mockLocalGameObject.Verify(g => g.AddComponent<BaitKillCoolReducer>(), Times.Never());
	}

	[Fact]
	public void RolePlayerKilledAction_WhenBenefitDisabled_DoesNotAddKillCoolReducer()
	{
		// Arrange
		SetLobbyMode(true);

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		byte killerId = 1;
		byte rolePlayerId = 2;

		var reporter = new BaitDelayReporter(IntPtr.Zero);

		var mockLocalGameObject = new Mock<GameObject>(IntPtr.Zero);

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(killerId);
		mockLocalPlayer.SetupGet(p => p.gameObject).Returns(mockLocalGameObject.Object);

		var mockHudGameObject = new Mock<GameObject>(IntPtr.Zero);
		var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		hudMock.SetupGet(h => h.gameObject).Returns(mockHudGameObject.Object);

		SetupFullScreenFlasherMocks(hudMock);

		mockHudGameObject.Setup(g => g.AddComponent<BaitDelayReporter>()).Returns(reporter);

		var rolePlayerMock = new Mock<PlayerControl>(IntPtr.Zero);
		rolePlayerMock.SetupGet(p => p.PlayerId).Returns(rolePlayerId);

		var killerPlayerMock = new Mock<PlayerControl>(IntPtr.Zero);
		killerPlayerMock.SetupGet(p => p.PlayerId).Returns(killerId);

		var baitRole = new ExtremeRoles.Roles.Solo.Crewmate.Bait();
		baitRole.CreateRoleAllOption();

		if (baitRole.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.EnableBaitBenefit, out var option) && option != null)
		{
			option.Selection = 0; // false
		}
		if (baitRole.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.DelayUntilForceReport, out var delayOption) && delayOption != null)
		{
			delayOption.Selection = 0; // 0.0s so it does not start coroutine
		}

		baitRole.Initialize();

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[rolePlayerId] = baitRole;

		var killerRole = new SpecialImpostor();
		killerRole.CreateRoleAllOption();
		killerRole.Initialize();
		ExtremeRoleManager.GameRole[killerId] = killerRole;

		// Act
		baitRole.RolePlayerKilledAction(rolePlayerMock.Object, killerPlayerMock.Object);

		// Assert
		mockLocalGameObject.Verify(g => g.AddComponent<BaitKillCoolReducer>(), Times.Never());
	}

	[Fact]
	public void CreateRoleAllOption_CreatesExpectedOptions()
	{
		// Arrange
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Bait);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bait();

		// Act
		role.CreateRoleAllOption();

		// Assert
		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);
	}
}
