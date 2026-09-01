using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class TimeBreakerTimeBreakSystemTests
{
	public TimeBreakerTimeBreakSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void Deteriorate_MarkClean_Reset()
	{
		var system = new TimeBreakerTimeBreakSystem(5.0f, false, false, false);
		Assert.False(system.Active);
		Assert.False(system.IsDirty);

		system.Deteriorate(1.0f);
		system.MarkClean();
		system.Reset(ResetTiming.MeetingStart, null);
		system.Serialize(null!, false);
		system.Deserialize(null!, false);
	}
}
