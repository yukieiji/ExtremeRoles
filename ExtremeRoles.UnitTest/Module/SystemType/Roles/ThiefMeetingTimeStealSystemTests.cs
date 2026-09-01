using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ThiefMeetingTimeStealSystemTests
{
	public ThiefMeetingTimeStealSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void MarkClean_And_Serialize()
	{
		var system = new ThiefMeetingTimeStealSystem(3, -10, 5);
		Assert.False(system.IsDirty);

		system.MarkClean();
		system.Reset(ResetTiming.MeetingStart, null);

		var writer = new Mock<MessageWriter>(System.IntPtr.Zero);
		system.Serialize(writer.Object, true);
		writer.Verify(w => w.WritePacked(0), Times.Once);
	}
}
