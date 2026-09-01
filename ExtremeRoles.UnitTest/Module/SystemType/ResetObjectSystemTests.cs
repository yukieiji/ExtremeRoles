using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ResetObjectSystemTests
{
	[Fact]
	public void Add_And_Reset_OnMeetingStart_ClearsObjects()
	{
		var system = new ResetObjectSystem();
		var mockResetObj1 = new Mock<IMeetingResetObject>();
		var mockResetObj2 = new Mock<IMeetingResetObject>();

		system.Add(mockResetObj1.Object);
		system.Add(mockResetObj2.Object);

		// Non-MeetingStart timing does not clear
		system.Reset(ResetTiming.MeetingEnd, null);
		mockResetObj1.Verify(x => x.Clear(), Times.Never);

		// MeetingStart timing clears
		system.Reset(ResetTiming.MeetingStart, null);
		mockResetObj1.Verify(x => x.Clear(), Times.Once);
		mockResetObj2.Verify(x => x.Clear(), Times.Once);

		// Second MeetingStart timing does nothing as list was cleared
		system.Reset(ResetTiming.MeetingStart, null);
		mockResetObj1.Verify(x => x.Clear(), Times.Once);
	}

	[Fact]
	public void UpdateSystem_DoesNotThrow()
	{
		var system = new ResetObjectSystem();
		system.UpdateSystem(null!, null!);
	}
}
