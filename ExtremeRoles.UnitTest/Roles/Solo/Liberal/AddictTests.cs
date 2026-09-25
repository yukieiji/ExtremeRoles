using System;
using AmongUs.GameOptions;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Liberal;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Liberal;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class AddictTests
{
	private static Harmony? harmony;

	public AddictTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupPaletteHelpers();
		MockSetupHelper.SetupAmongUsClientMock();
		MockSetupHelper.SetupGameDataMock();
		MockSetupHelper.SetupTimeHelpers();
		MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		MockSetupHelper.SetupOptionManager();
		SetupMeetingHudAndExileMocks();
		SetupMinigameMock(null);
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupLobbyMock();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupPlayerControlMocks();
		SetupGameOptionsManagerMock();
		SetupShipStatusMock();

		if (harmony == null)
		{
			harmony = new Harmony("AddictTestsPatch");
			var origSub = typeof(Vector2).GetMethod("op_Subtraction", new[] { typeof(Vector2), typeof(Vector2) });
			var prefixSub = typeof(Vector2TestPatch).GetMethod(nameof(Vector2TestPatch.OpSubtractionPrefix));
			if (origSub != null && prefixSub != null)
			{
				harmony.Patch(origSub, prefix: new HarmonyMethod(prefixSub));
			}

			var origSqrMag = typeof(Vector2).GetProperty("sqrMagnitude")?.GetGetMethod();
			var prefixSqrMag = typeof(Vector2TestPatch).GetMethod(nameof(Vector2TestPatch.SqrMagnitudePrefix));
			if (origSqrMag != null && prefixSqrMag != null)
			{
				harmony.Patch(origSqrMag, prefix: new HarmonyMethod(prefixSqrMag));
			}
		}

		ExtremeRoleManager.GameRole.Clear();

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);
	}

	public static class Vector2TestPatch
	{
		public static bool OpSubtractionPrefix(Vector2 a, Vector2 b, ref Vector2 __result)
		{
			__result = new Vector2(a.x - b.x, a.y - b.y);
			return false;
		}

		public static bool SqrMagnitudePrefix(Vector2 __instance, ref float __result)
		{
			__result = __instance.x * __instance.x + __instance.y * __instance.y;
			return false;
		}
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

		Assert.Equal(15.0f, status.CurrentSelfKillTimer);
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

		// Act
		addict.ResetOnMeetingStart();

		// Assert
		Assert.Equal(15.0f, status.CurrentSelfKillTimer);

		addict.ResetOnMeetingEnd(null);

		Assert.Equal(15.0f, status.CurrentSelfKillTimer);
	}

	[Fact]
	public void Update_WhenTaskPhase_UpdatesTimerAndTriggersExplosion()
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
		Assert.True(status.CurrentSelfKillTimer <= 15.0f);
	}

	[Fact]
	public void UpdateTimer_WhenStationaryAndMoving_UpdatesTimersAndTriggersExplosion()
	{
		// Arrange
		var status = new AddictStatusModel(15.0f, 5.0f);
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.CanMove).Returns(true);
		localPlayerMock.SetupGet(p => p.inVent).Returns(false);

		var infoMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		infoMock.SetupGet(i => i.Object).Returns(localPlayerMock.Object);
		infoMock.SetupGet(i => i.IsDead).Returns(false);
		infoMock.SetupGet(i => i.Disconnected).Returns(false);
		localPlayerMock.SetupGet(p => p.Data).Returns(infoMock.Object);

		Vector2 currentPos = new Vector2(0f, 0f);
		localPlayerMock.Setup(p => p.GetTruePosition()).Returns(() => currentPos);

		// Stationary update
		bool explode = status.UpdateTimer(localPlayerMock.Object, 1.0f);
		Assert.False(explode);
		Assert.Equal(14.0f, status.CurrentSelfKillTimer);

		// Trigger explosion
		explode = status.UpdateTimer(localPlayerMock.Object, 14.0f);
		Assert.True(explode);

		// Subsequent call after explosion returns false
		explode = status.UpdateTimer(localPlayerMock.Object, 1.0f);
		Assert.False(explode);
	}
}
