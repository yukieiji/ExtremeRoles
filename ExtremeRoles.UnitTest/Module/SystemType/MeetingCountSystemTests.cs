using ExtremeRoles.Module.SystemType;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class MeetingCountSystemTests
{
	[Fact]
	public void Increase_IncrementsCounter()
	{
		var system = new MeetingCountSystem();
		Assert.Equal(0, system.Counter);

		system.Increse();
		Assert.Equal(1, system.Counter);

		system.Increse();
		Assert.Equal(2, system.Counter);
	}

	[Fact]
	public void Reset_And_UpdateSystem_DoNotThrow()
	{
		var system = new MeetingCountSystem();
		system.Reset(ResetTiming.MeetingStart, null);
		system.UpdateSystem(null!, null!);
	}
}
