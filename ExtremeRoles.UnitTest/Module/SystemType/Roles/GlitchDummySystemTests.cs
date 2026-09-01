using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class GlitchDummySystemTests
{
	public GlitchDummySystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void Deteriorate_MarkClean_Reset_DoNotThrow()
	{
		var system = new GlitchDummySystem(false, false, 5.0f);
		Assert.False(system.IsDirty);

		system.MarkClean();
		system.Deteriorate(1.0f);
		system.Reset(ResetTiming.MeetingStart, null);
		system.Deserialize(null!, false);
		system.Serialize(null!, false);
	}
}
