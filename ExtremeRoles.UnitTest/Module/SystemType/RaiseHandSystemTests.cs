using ExtremeRoles.Module.SystemType;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class RaiseHandSystemTests
{
	public RaiseHandSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void Get_ReturnsInstance()
	{
		var system = RaiseHandSystem.Get();
		Assert.NotNull(system);
		Assert.False(system.IsInit);
	}

	[Fact]
	public void SetActive_And_RaiseHandButtonSetActive_WhenNull_DoNotThrow()
	{
		var system = new RaiseHandSystem();
		system.SetActive(true);
		system.RaiseHandButtonSetActive(false);
	}

	[Fact]
	public void MarkClean_And_Deteriorate()
	{
		var system = new RaiseHandSystem();
		system.MarkClean();
		Assert.False(system.IsDirty);

		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(false);

		system.Deteriorate(1.0f);
	}

	[Fact]
	public void Serialize_And_Deserialize()
	{
		var system = new RaiseHandSystem();

		var reader = new Mock<MessageReader>();
		reader.Setup(r => r.ReadPackedInt32()).Returns(1);
		reader.Setup(r => r.ReadByte()).Returns((byte)5);

		system.Deserialize(reader.Object, false);

		var writer = new Mock<MessageWriter>(System.IntPtr.Zero);
		system.Serialize(writer.Object, true);

		writer.Verify(w => w.WritePacked(1), Times.Once);
		Assert.True(system.IsDirty);
	}

	[Fact]
	public void Reset_OnMeetingEnd_ClearsState()
	{
		var system = new RaiseHandSystem();
		system.Reset(ResetTiming.MeetingStart, null);
		system.Reset(ResetTiming.MeetingEnd, null);
		Assert.False(system.IsInit);
	}

	[Fact]
	public void UpdateSystem_RaisesHandForPlayer()
	{
		var system = new RaiseHandSystem();
		var reader = new Mock<MessageReader>();
		reader.Setup(r => r.ReadByte()).Returns((byte)1);

		system.UpdateSystem(null!, reader.Object);
	}
}
