using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class WispTorchSystemTests
{
	public WispTorchSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void HasTorch_MarkClean_Reset()
	{
		var system = new WispTorchSystem(3, 2.0f, 10.0f, 5.0f);
		Assert.False(system.IsDirty);
		Assert.False(system.HasTorch(1));

		system.MarkClean();
		system.Reset(ResetTiming.MeetingEnd, null);

		var writer = new Mock<MessageWriter>(System.IntPtr.Zero);
		system.Serialize(writer.Object, true);
	}
}
