using ExtremeRoles.Module.SystemType;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ExtremeConsoleSystemTests
{
	public ExtremeConsoleSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void Create_ReturnsInstance()
	{
		var system = ExtremeConsoleSystem.Create();
		Assert.NotNull(system);
	}

	[Fact]
	public void Reset_And_UpdateSystem_DoNotThrow()
	{
		var system = new ExtremeConsoleSystem();
		system.Reset(ResetTiming.MeetingStart, null);
		system.UpdateSystem(null!, null!);
	}
}
