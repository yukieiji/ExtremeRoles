using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ForestallerSystemTests
{
	public ForestallerSystemTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void Reset_ClearsActiveState()
	{
		var system = new ForestallerSystem();
		var playerMock = MockSetupHelper.SetupPlayerControlMocks();
		playerMock.SetupGet(p => p.PlayerId).Returns((byte)1);

		var startReader = new Mock<MessageReader>();
		startReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)ForestallerSystem.Ops.StartForestall);

		system.UpdateSystem(playerMock.Object, startReader.Object);
		Assert.True(system.IsForestallActive);

		system.Reset(ResetTiming.MeetingStart, null);
		Assert.False(system.IsForestallActive);
	}

	[Fact]
	public void MultiplePlayers_ForestallActiveUntilAllEnd()
	{
		var system = new ForestallerSystem();

		var p1 = new Mock<PlayerControl>(System.IntPtr.Zero);
		p1.SetupGet(p => p.PlayerId).Returns((byte)1);

		var p2 = new Mock<PlayerControl>(System.IntPtr.Zero);
		p2.SetupGet(p => p.PlayerId).Returns((byte)2);

		var startReader1 = new Mock<MessageReader>();
		startReader1.SetupSequence(r => r.ReadByte())
			.Returns((byte)ForestallerSystem.Ops.StartForestall);

		var startReader2 = new Mock<MessageReader>();
		startReader2.SetupSequence(r => r.ReadByte())
			.Returns((byte)ForestallerSystem.Ops.StartForestall);

		system.UpdateSystem(p1.Object, startReader1.Object);
		system.UpdateSystem(p2.Object, startReader2.Object);

		Assert.True(system.IsForestallActive);

		var endReader1 = new Mock<MessageReader>();
		endReader1.SetupSequence(r => r.ReadByte())
			.Returns((byte)ForestallerSystem.Ops.EndForestall);

		system.UpdateSystem(p1.Object, endReader1.Object);

		// Still active because player 2 is still active
		Assert.True(system.IsForestallActive);

		var endReader2 = new Mock<MessageReader>();
		endReader2.SetupSequence(r => r.ReadByte())
			.Returns((byte)ForestallerSystem.Ops.EndForestall);

		system.UpdateSystem(p2.Object, endReader2.Object);

		// Now inactive
		Assert.False(system.IsForestallActive);
	}
}
