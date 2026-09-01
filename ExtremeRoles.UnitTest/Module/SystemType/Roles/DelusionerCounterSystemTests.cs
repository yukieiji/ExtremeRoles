using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class DelusionerCounterSystemTests
{
	public DelusionerCounterSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void TryGetCounter_WhenEmpty_ReturnsFalse()
	{
		var system = new DelusionerCounterSystem();
		bool found = system.TryGetCounter(1, out int count);
		Assert.False(found);
		Assert.Equal(0, count);
	}

	[Fact]
	public void Reset_ClearsCounts()
	{
		var system = new DelusionerCounterSystem();

		var playerMock = MockSetupHelper.SetupPlayerControlMocks();
		playerMock.SetupGet(p => p.PlayerId).Returns((byte)1);

		var readerAdd = new Mock<MessageReader>();
		readerAdd.SetupSequence(r => r.ReadByte())
			.Returns((byte)DelusionerCounterSystem.Ops.Ready);
		readerAdd.Setup(r => r.ReadPackedInt32()).Returns(5);

		system.UpdateSystem(playerMock.Object, readerAdd.Object);
		Assert.True(system.TryGetCounter(1, out int count));
		Assert.Equal(5, count);

		system.Reset(ResetTiming.MeetingStart, null);
		Assert.False(system.TryGetCounter(1, out _));
	}

	[Fact]
	public void UpdateSystem_AllOps()
	{
		var system = new DelusionerCounterSystem();
		var playerMock = MockSetupHelper.SetupPlayerControlMocks();
		playerMock.SetupGet(p => p.PlayerId).Returns((byte)1);

		// Ready
		var rReady = new Mock<MessageReader>();
		rReady.SetupSequence(r => r.ReadByte())
			.Returns((byte)DelusionerCounterSystem.Ops.Ready);
		rReady.Setup(r => r.ReadPackedInt32()).Returns(3);
		system.UpdateSystem(playerMock.Object, rReady.Object);
		Assert.True(system.TryGetCounter(1, out _));

		// Remove
		var rRemove = new Mock<MessageReader>();
		rRemove.SetupSequence(r => r.ReadByte())
			.Returns((byte)DelusionerCounterSystem.Ops.Remove);
		system.UpdateSystem(playerMock.Object, rRemove.Object);
		Assert.False(system.TryGetCounter(1, out _));

		// ForceUse for non-local player
		var rForceUse = new Mock<MessageReader>();
		rForceUse.SetupSequence(r => r.ReadByte())
			.Returns((byte)DelusionerCounterSystem.Ops.ForceUse)
			.Returns((byte)99); // target player 99
		rForceUse.Setup(r => r.ReadPackedInt32()).Returns(1);
		system.UpdateSystem(playerMock.Object, rForceUse.Object);

		// Default/Invalid op
		var rDefault = new Mock<MessageReader>();
		rDefault.SetupSequence(r => r.ReadByte())
			.Returns((byte)255);
		system.UpdateSystem(playerMock.Object, rDefault.Object);
	}
}
