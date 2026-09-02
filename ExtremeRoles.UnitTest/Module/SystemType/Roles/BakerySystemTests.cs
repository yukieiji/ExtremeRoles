using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class BakerySystemTests
{
	public BakerySystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
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
}
