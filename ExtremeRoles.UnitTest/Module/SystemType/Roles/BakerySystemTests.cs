using System;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

public class MockMultiAssignBakary : MultiAssignRoleBase
{
	public MockMultiAssignBakary(SingleRoleBase anotherRole) : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Bakary,
			ColorPalette.BakaryWheatColor))
	{
		this.AnotherRole = anotherRole;
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) { }
	protected override void RoleSpecificInit() { }
}

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class BakerySystemTests : IDisposable
{
	public BakerySystemTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupPaletteHelpers();
		MockSetupHelper.SetupAmongUsClientMock();
		MockSetupHelper.SetupPlayerControlMocks();
		SetupShipStatusMock();
		SetupMeetingAndExileMocks();
		SetupTranslationControllerMock();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupGameDataMock();
		ExtremeRoleManager.GameRole.Clear();
		PlayerCache.RemovePlayerControl(_ => true);
		MeetingReporter.Reset();
	}

	public void Dispose()
	{
		ExtremeRoleManager.GameRole.Clear();
		PlayerCache.RemovePlayerControl(_ => true);
		MeetingReporter.Reset();
	}

	private static void SetupTranslationControllerMock()
	{
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
	}

	private static void SetupMeetingAndExileMocks()
	{
		if (MockMeetingHudget_InstanceHelper.Instance == null)
		{
			var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
			mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
			MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;
		}

		if (MockDestroyableSingletonget_InstanceHelper<MeetingHud>.Instance == null)
		{
			var mockDestroyableMeetingHelper = new Mock<MockDestroyableSingletonget_InstanceHelper<MeetingHud>>();
			mockDestroyableMeetingHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
			MockDestroyableSingletonget_InstanceHelper<MeetingHud>.Instance = mockDestroyableMeetingHelper.Object;
		}

		if (MockExileControllerget_InstanceHelper.Instance == null)
		{
			var mockExileControllerHelper = new Mock<MockExileControllerget_InstanceHelper>();
			mockExileControllerHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
			MockExileControllerget_InstanceHelper.Instance = mockExileControllerHelper.Object;
		}

		if (MockDestroyableSingletonget_InstanceHelper<ExileController>.Instance == null)
		{
			var mockDestroyableExileHelper = new Mock<MockDestroyableSingletonget_InstanceHelper<ExileController>>();
			mockDestroyableExileHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
			MockDestroyableSingletonget_InstanceHelper<ExileController>.Instance = mockDestroyableExileHelper.Object;
		}
	}

	private static void SetupShipStatusMock()
	{
		if (MockShipStatusget_InstanceHelper.Instance == null)
		{
			var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
			mockShipStatus.SetupGet(s => s.enabled).Returns(true);

			var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
			mockShipHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
			MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;
		}
	}

	private static void SetTaskPhase()
	{
		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;
	}

	[Fact]
	public void MarkClean_And_Serialize_Deserialize()
	{
		var system = new BakerySystem(10.0f, 20.0f, true);
		system.MarkClean();
		Assert.False(system.IsDirty);

		var reader = new Mock<MessageReader>();
		reader.Setup(r => r.ReadSingle()).Returns(15.0f);
		system.Deserialize(reader.Object, false);

		var writer = new Mock<MessageWriter>(System.IntPtr.Zero);
		system.Serialize(writer.Object, true);
		writer.Verify(w => w.Write(15.0f), Times.Once);
		Assert.True(system.IsDirty);
	}

	[Fact]
	public void Reset_And_UpdateSystem()
	{
		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(true);

		var system = new BakerySystem(10.0f, 20.0f, false);
		system.Reset(ResetTiming.MeetingEnd, null);
		system.Reset(ResetTiming.MeetingStart, null);
		Assert.True(system.IsDirty);

		system.UpdateSystem(null!, null!);
	}

	[Fact]
	public void Deteriorate_WhenNotInTaskPhase_DoesNotIncrementTimer()
	{
		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
		GameProgressSystem.Current = GameProgressSystem.Progress.Meeting;

		byte playerId = 1;
		var bakary = new ExtremeRoles.Roles.Solo.Crewmate.Bakary();
		ExtremeRoleManager.GameRole[playerId] = bakary;

		var system = new BakerySystem(60.0f, 120.0f, true);
		system.Deteriorate(10.0f);

		var writer = new Mock<MessageWriter>(IntPtr.Zero);
		system.Serialize(writer.Object, false);
		writer.Verify(w => w.Write(0.0f), Times.Once);
	}

	[Fact]
	public void Deteriorate_WhenInTaskPhaseWithBakary_IncrementsTimer()
	{
		SetTaskPhase();

		byte playerId = 1;
		var bakary = new ExtremeRoles.Roles.Solo.Crewmate.Bakary();
		ExtremeRoleManager.GameRole[playerId] = bakary;

		var system = new BakerySystem(60.0f, 120.0f, true);
		system.Deteriorate(30.0f);

		var writer = new Mock<MessageWriter>(IntPtr.Zero);
		system.Serialize(writer.Object, false);
		writer.Verify(w => w.Write(30.0f), Times.Once);
	}

	[Fact]
	public void Deteriorate_WhenInTaskPhaseWithoutBakary_DoesNotIncrementTimer()
	{
		SetTaskPhase();

		ExtremeRoleManager.GameRole.Clear();

		var system = new BakerySystem(60.0f, 120.0f, true);
		system.Deteriorate(30.0f);

		var writer = new Mock<MessageWriter>(IntPtr.Zero);
		system.Serialize(writer.Object, false);
		writer.Verify(w => w.Write(0.0f), Times.Once);
	}

	[Fact]
	public void Deteriorate_WithMultiAssignRoleBakary_IncrementsTimer()
	{
		SetTaskPhase();

		byte playerId = 1;
		var bakary = new ExtremeRoles.Roles.Solo.Crewmate.Bakary();
		var multiRole = new MockMultiAssignBakary(bakary);
		ExtremeRoleManager.GameRole[playerId] = multiRole;

		var system = new BakerySystem(60.0f, 120.0f, true);
		system.Deteriorate(15.0f);

		var writer = new Mock<MessageWriter>(IntPtr.Zero);
		system.Serialize(writer.Object, false);
		writer.Verify(w => w.Write(15.0f), Times.Once);
	}

	[Fact]
	public void Reset_MeetingEnd_WhenChangeCookingDisabled_AlwaysReportsGoodBread()
	{
		SetTaskPhase();

		byte playerId = 1;
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		PlayerCache.AddPlayerControl(mockPlayer.Object);

		var bakary = new ExtremeRoles.Roles.Solo.Crewmate.Bakary();
		ExtremeRoleManager.GameRole[playerId] = bakary;

		var system = new BakerySystem(60.0f, 120.0f, false);
		system.Deteriorate(10.0f); // timer = 10s < 60s

		MeetingReporter.Reset();
		system.Reset(ResetTiming.MeetingEnd);

		string report = MeetingReporter.Instance.GetMeetingEndReport();
		Assert.Contains("goodBread", report);

		var writer = new Mock<MessageWriter>(IntPtr.Zero);
		system.Serialize(writer.Object, false);
		writer.Verify(w => w.Write(0.0f), Times.Once);
	}

	[Theory]
	[InlineData(30.0f, "rawBread")]
	[InlineData(60.0f, "goodBread")]
	[InlineData(90.0f, "goodBread")]
	[InlineData(120.0f, "badBread")]
	[InlineData(150.0f, "badBread")]
	public void Reset_MeetingEnd_WhenChangeCookingEnabled_ReportsBreadConditionBasedOnTimer(float cookTime, string expectedCondition)
	{
		SetTaskPhase();

		byte playerId = 1;
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		PlayerCache.AddPlayerControl(mockPlayer.Object);

		var bakary = new ExtremeRoles.Roles.Solo.Crewmate.Bakary();
		ExtremeRoleManager.GameRole[playerId] = bakary;

		var system = new BakerySystem(60.0f, 120.0f, true);
		system.Deteriorate(cookTime);

		MeetingReporter.Reset();
		system.Reset(ResetTiming.MeetingEnd);

		string report = MeetingReporter.Instance.GetMeetingEndReport();
		Assert.Contains(expectedCondition, report);

		// Timer should be reset to 0 after meeting end
		var writer = new Mock<MessageWriter>(IntPtr.Zero);
		system.Serialize(writer.Object, false);
		writer.Verify(w => w.Write(0.0f), Times.Once);
	}

	[Fact]
	public void Reset_MeetingEnd_WhenBakaryIsDead_DoesNotAddReport()
	{
		SetTaskPhase();

		byte playerId = 1;
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(true); // Bakary is dead!
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		PlayerCache.AddPlayerControl(mockPlayer.Object);

		var bakary = new ExtremeRoles.Roles.Solo.Crewmate.Bakary();
		ExtremeRoleManager.GameRole[playerId] = bakary;

		var system = new BakerySystem(60.0f, 120.0f, true);
		system.Deteriorate(60.0f);

		MeetingReporter.Reset();
		system.Reset(ResetTiming.MeetingEnd);

		string report = MeetingReporter.Instance.GetMeetingEndReport();
		Assert.Empty(report);
	}
}
