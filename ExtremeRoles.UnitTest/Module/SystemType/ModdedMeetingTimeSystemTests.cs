using ExtremeRoles.Module.SystemType;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ModdedMeetingTimeSystemTests
{
	public ModdedMeetingTimeSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void Properties_And_MarkClean()
	{
		var system = new ModdedMeetingTimeSystem();
		Assert.True(system.IsShowTimer);
		Assert.Equal(0, system.PermOffset);
		Assert.Equal(0, system.TempOffset);
		Assert.Equal(0, system.ButtonTimeOffset);
		Assert.Equal(0, system.HudTimerStartOffset);

		system.IsDirty = true;
		Assert.True(system.IsDirty);
		system.MarkClean();
		Assert.False(system.IsDirty);
	}

	[Fact]
	public void Serialize_And_Deserialize()
	{
		var system = new ModdedMeetingTimeSystem();

		var reader = new Mock<MessageReader>();
		reader.SetupSequence(r => r.ReadPackedInt32())
			.Returns(10) // PermOffset
			.Returns(20) // TempOffset
			.Returns(30); // ButtonTimeOffset
		reader.Setup(r => r.ReadBoolean()).Returns(false);

		system.Deserialize(reader.Object, false);

		Assert.Equal(10, system.PermOffset);
		Assert.Equal(20, system.TempOffset);
		Assert.Equal(30, system.ButtonTimeOffset);
		Assert.False(system.IsShowTimer);
		Assert.Equal(30, system.HudTimerStartOffset);

		var writer = new Mock<MessageWriter>(System.IntPtr.Zero);
		system.Serialize(writer.Object, true);

		writer.Verify(w => w.WritePacked(10), Times.Once);
		writer.Verify(w => w.WritePacked(20), Times.Once);
		writer.Verify(w => w.WritePacked(30), Times.Once);
		writer.Verify(w => w.Write(false), Times.Once);
		Assert.True(system.IsDirty);
	}

	[Fact]
	public void Reset_WhenMeetingEnd_And_Host()
	{
		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(true);

		var system = new ModdedMeetingTimeSystem();
		var reader = new Mock<MessageReader>();
		reader.Setup(r => r.ReadByte()).Returns((byte)ModdedMeetingTimeSystem.Ops.ChangeMeetingHudTempOffset);
		reader.Setup(r => r.ReadPackedInt32()).Returns(15);
		system.UpdateSystem(null!, reader.Object);

		Assert.Equal(15, system.TempOffset);

		// Not MeetingEnd -> no change
		system.Reset(ResetTiming.MeetingStart, null);
		Assert.Equal(15, system.TempOffset);

		// MeetingEnd
		system.Reset(ResetTiming.MeetingEnd, null);
		Assert.Equal(0, system.TempOffset);
		Assert.True(system.IsShowTimer);
		Assert.True(system.IsDirty);
	}

	[Fact]
	public void UpdateSystem_AllOps()
	{
		var system = new ModdedMeetingTimeSystem();

		// ChangeMeetingHudPermOffset
		var r1 = new Mock<MessageReader>();
		r1.Setup(r => r.ReadByte()).Returns((byte)ModdedMeetingTimeSystem.Ops.ChangeMeetingHudPermOffset);
		r1.Setup(r => r.ReadPackedInt32()).Returns(5);
		system.UpdateSystem(null!, r1.Object);
		Assert.Equal(5, system.PermOffset);

		// ChangeMeetingHudTempOffset
		var r2 = new Mock<MessageReader>();
		r2.Setup(r => r.ReadByte()).Returns((byte)ModdedMeetingTimeSystem.Ops.ChangeMeetingHudTempOffset);
		r2.Setup(r => r.ReadPackedInt32()).Returns(10);
		system.UpdateSystem(null!, r2.Object);
		Assert.Equal(10, system.TempOffset);

		// ChangeButtonTime
		var r3 = new Mock<MessageReader>();
		r3.Setup(r => r.ReadByte()).Returns((byte)ModdedMeetingTimeSystem.Ops.ChangeButtonTime);
		r3.Setup(r => r.ReadPackedInt32()).Returns(15);
		system.UpdateSystem(null!, r3.Object);
		Assert.Equal(15, system.ButtonTimeOffset);

		// ChangeMeetingTimerShower
		var r4 = new Mock<MessageReader>();
		r4.Setup(r => r.ReadByte()).Returns((byte)ModdedMeetingTimeSystem.Ops.ChangeMeetingTimerShower);
		r4.Setup(r => r.ReadBoolean()).Returns(false);
		system.UpdateSystem(null!, r4.Object);
		Assert.False(system.IsShowTimer);

		// Reset
		var r5 = new Mock<MessageReader>();
		r5.Setup(r => r.ReadByte()).Returns((byte)ModdedMeetingTimeSystem.Ops.Reset);
		system.UpdateSystem(null!, r5.Object);
		Assert.Equal(0, system.PermOffset);
		Assert.Equal(0, system.TempOffset);
		Assert.Equal(0, system.ButtonTimeOffset);
		Assert.True(system.IsShowTimer);

		// Default/Invalid
		system.IsDirty = false;
		var r6 = new Mock<MessageReader>();
		r6.Setup(r => r.ReadByte()).Returns((byte)255);
		system.UpdateSystem(null!, r6.Object);
		Assert.False(system.IsDirty);
	}
}
