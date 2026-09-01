using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class RaiderBombSystemTests
{
	public RaiderBombSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void MarkClean_Reset_Deteriorate()
	{
		var bombParam = new RaiderBomb.Parameter(2.0f, 10.0f, true);
		var param = new RaiderBombSystem.Parameter(RaiderBombSystem.BombType.SingleBombType, 1, 5.0f, bombParam);

		var system = new RaiderBombSystem(param);
		Assert.False(system.IsDirty);

		system.MarkClean();
		system.Reset(ResetTiming.MeetingStart, null);
		system.Deteriorate(1.0f);
	}
}
