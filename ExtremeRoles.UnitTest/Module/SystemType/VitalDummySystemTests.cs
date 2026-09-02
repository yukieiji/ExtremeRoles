using ExtremeRoles.Module.SystemType;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class VitalDummySystemTests
{
	public VitalDummySystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void Get_And_TryGet_ReturnsInstance()
	{
		var system = VitalDummySystem.Get();
		Assert.NotNull(system);

		bool found = VitalDummySystem.TryGet(out var trySys);
		Assert.True(found);
		Assert.Same(system, trySys);
	}

	[Fact]
	public void Properties_AddRemove_And_Reset()
	{
		var system = new VitalDummySystem();
		Assert.False(system.IsActive);
		system.IsActive = true;
		Assert.True(system.IsActive);

		Assert.Equal(VitalDummySystem.DummyMode.No, system.Mode);
		system.Mode = VitalDummySystem.DummyMode.Random;
		Assert.Equal(VitalDummySystem.DummyMode.Random, system.Mode);

		system.AddAlive(1, 2);
		system.AddDead(3, 4);
		system.AddDisconnect(5, 6);

		system.RemoveAlive(1);
		system.RemoveDead(3);
		system.RemoveDisconnect(5);

		system.VitalBeginPostfix();

		system.Reset(ResetTiming.MeetingStart, null);
		system.Reset(ResetTiming.OnPlayer, null);
	}

	[Fact]
	public void UpdateSystem_AllOptions()
	{
		var system = new VitalDummySystem();

		// AddAlive
		var r1 = new Mock<MessageReader>();
		r1.SetupSequence(r => r.ReadByte())
			.Returns((byte)VitalDummySystem.Option.AddAlive)
			.Returns((byte)1);
		system.UpdateSystem(null!, r1.Object);

		// AddDead
		var r2 = new Mock<MessageReader>();
		r2.SetupSequence(r => r.ReadByte())
			.Returns((byte)VitalDummySystem.Option.AddDead)
			.Returns((byte)2);
		system.UpdateSystem(null!, r2.Object);

		// AddDisconnect
		var r3 = new Mock<MessageReader>();
		r3.SetupSequence(r => r.ReadByte())
			.Returns((byte)VitalDummySystem.Option.AddDisconnect)
			.Returns((byte)3);
		system.UpdateSystem(null!, r3.Object);

		// RemoveAlive
		var r4 = new Mock<MessageReader>();
		r4.SetupSequence(r => r.ReadByte())
			.Returns((byte)VitalDummySystem.Option.RemoveAlive)
			.Returns((byte)1);
		system.UpdateSystem(null!, r4.Object);

		// RemoveDead
		var r5 = new Mock<MessageReader>();
		r5.SetupSequence(r => r.ReadByte())
			.Returns((byte)VitalDummySystem.Option.RemoveDead)
			.Returns((byte)2);
		system.UpdateSystem(null!, r5.Object);

		// RemoveDisconnect
		var r6 = new Mock<MessageReader>();
		r6.SetupSequence(r => r.ReadByte())
			.Returns((byte)VitalDummySystem.Option.RemoveDisconnect)
			.Returns((byte)3);
		system.UpdateSystem(null!, r6.Object);

		// Default/Invalid
		var r7 = new Mock<MessageReader>();
		r7.SetupSequence(r => r.ReadByte())
			.Returns((byte)255)
			.Returns((byte)0);
		system.UpdateSystem(null!, r7.Object);
	}
}
