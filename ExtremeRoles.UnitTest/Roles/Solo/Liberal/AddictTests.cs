using System;
using AmongUs.GameOptions;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Liberal;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Liberal;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class AddictTests
{
	public AddictTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupPaletteHelpers();
		MockSetupHelper.SetupAmongUsClientMock();
		MockSetupHelper.SetupGameDataMock();
		MockSetupHelper.SetupTimeHelpers();
		MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		SetupMeetingHudAndExileMocks();
		SetupMinigameMock(null);
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupLobbyMock();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupPlayerControlMocks();
		SetupGameOptionsManagerMock();
		SetupShipStatusMock();

		ExtremeRoleManager.GameRole.Clear();

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);
	}

	private static void SetupMinigameMock(Minigame? minigame)
	{
		var mockMinigameHelper = new Mock<MockMinigameget_InstanceHelper>();
		mockMinigameHelper.Setup(x => x.Invoke()).Returns(minigame!);
		MockMinigameget_InstanceHelper.Instance = mockMinigameHelper.Object;
	}

	private static void SetupMeetingHudAndExileMocks()
	{
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
	}

	private static void SetupShipStatusMock()
	{
		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		mockShip.SetupGet(s => s.enabled).Returns(true);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;
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

	[Fact]
	public void Initialize_AndOptions_InitializesStatusModelWithDefaultOptions()
	{
		// Arrange
		var addict = new Addict();
		addict.CreateRoleAllOption();

		// Act
		addict.Initialize();

		// Assert
		Assert.Equal(ExtremeRoleId.Addict, addict.Core.Id);
		Assert.NotNull(addict.Status);

		var status = addict.Status as AddictStatusModel;
		Assert.NotNull(status);

		Assert.Equal(15.0f, status.MaxSelfKillTimer);
		Assert.Equal(5.0f, status.MovementTimeToRecover);
		Assert.Equal(15.0f, status.CurrentSelfKillTimer);
		Assert.Equal(0.0f, status.CurrentMovementTime);
		Assert.False(status.HasExploded);
	}

	[Fact]
	public void ResetOnMeetingStart_And_ResetOnMeetingEnd_ResetsStatusModel()
	{
		// Arrange
		var addict = new Addict();
		addict.CreateRoleAllOption();
		addict.Initialize();

		var status = addict.Status as AddictStatusModel;
		Assert.NotNull(status);

		status.CurrentSelfKillTimer = 5.0f;
		status.CurrentMovementTime = 2.0f;
		status.HasExploded = true;

		// Act
		addict.ResetOnMeetingStart();

		// Assert
		Assert.Equal(15.0f, status.CurrentSelfKillTimer);
		Assert.Equal(0.0f, status.CurrentMovementTime);
		Assert.False(status.HasExploded);

		status.CurrentSelfKillTimer = 3.0f;
		addict.ResetOnMeetingEnd(null);

		Assert.Equal(15.0f, status.CurrentSelfKillTimer);
	}

	[Fact]
	public void Update_WhenTaskPhase_UpdatesPositionAndTimer()
	{
		// Arrange
		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.CanMove).Returns(true);
		localPlayerMock.SetupGet(p => p.inVent).Returns(false);

		var infoMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		infoMock.SetupGet(i => i.Object).Returns(localPlayerMock.Object);
		infoMock.SetupGet(i => i.IsDead).Returns(false);
		infoMock.SetupGet(i => i.Disconnected).Returns(false);
		localPlayerMock.SetupGet(p => p.Data).Returns(infoMock.Object);

		localPlayerMock.Setup(p => p.GetTruePosition()).Returns(new Vector2(10f, 10f));

		var addict = new Addict();
		addict.CreateRoleAllOption();
		addict.Initialize();

		var status = addict.Status as AddictStatusModel;
		Assert.NotNull(status);

		// Act
		addict.Update(localPlayerMock.Object);

		// Assert
		Assert.Equal(new Vector2(10f, 10f), status.PrevPlayerPos);
	}

	[Fact]
	public void Update_WhenTimerReachesZero_SetsHasExplodedTrue()
	{
		// Arrange
		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.CanMove).Returns(true);
		localPlayerMock.SetupGet(p => p.inVent).Returns(false);

		var infoMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		infoMock.SetupGet(i => i.Object).Returns(localPlayerMock.Object);
		infoMock.SetupGet(i => i.IsDead).Returns(false);
		infoMock.SetupGet(i => i.Disconnected).Returns(false);
		localPlayerMock.SetupGet(p => p.Data).Returns(infoMock.Object);
		localPlayerMock.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));

		var addict = new Addict();
		addict.CreateRoleAllOption();
		addict.Initialize();

		var status = addict.Status as AddictStatusModel;
		Assert.NotNull(status);

		status.CurrentSelfKillTimer = -1.0f;

		// Act
		addict.Update(localPlayerMock.Object);

		// Assert
		Assert.True(status.HasExploded);
	}
}
