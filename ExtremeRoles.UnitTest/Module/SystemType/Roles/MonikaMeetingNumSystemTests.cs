using AmongUs.GameOptions;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class MonikaMeetingNumSystemTests
{
	public MonikaMeetingNumSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupGameDataMock();

		var mockNormalOptions = new Mock<NormalGameOptionsV11>(System.IntPtr.Zero);
		mockNormalOptions.SetupGet(o => o.NumEmergencyMeetings).Returns(1);

		var mockOptionsMgr = new Mock<GameOptionsManager>(System.IntPtr.Zero);
		mockOptionsMgr.SetupGet(m => m.currentNormalGameOptions).Returns(mockNormalOptions.Object);

		var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
		mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockOptionsMgr.Object);
		MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
	}

	[Fact]
	public void Properties_And_UpdateSystem()
	{
		var system = new MonikaMeetingNumSystem();
		Assert.False(system.IsDirty);

		system.Reset(ResetTiming.MeetingStart, null);

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var reader = new Mock<MessageReader>();
		reader.Setup(r => r.ReadByte()).Returns((byte)1);
		reader.Setup(r => r.ReadBoolean()).Returns(false);

		system.UpdateSystem(mockPlayer.Object, reader.Object);
	}
}
