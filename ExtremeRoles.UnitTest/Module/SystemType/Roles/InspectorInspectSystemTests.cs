using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class InspectorInspectSystemTests
{
	public InspectorInspectSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void EndInspect_And_Reset_DoNotThrow()
	{
		var system = new InspectorInspectSystem(InspectorInspectSystem.InspectMode.Sabotage | InspectorInspectSystem.InspectMode.Vent);
		Assert.False(system.IsDirty);

		system.EndInspect(1);
		system.Reset(ResetTiming.MeetingStart, null);
		system.MarkClean();
		system.Serialize(null!, false);
		system.Deserialize(null!, false);
	}

	[Fact]
	public void UpdateSystem_AllOps()
	{
		var system = new InspectorInspectSystem(InspectorInspectSystem.InspectMode.Ability);

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		// StartInspect
		var rStart = new Mock<MessageReader>();
		rStart.SetupSequence(r => r.ReadByte())
			.Returns((byte)InspectorInspectSystem.Ops.StartInspect);
		system.UpdateSystem(mockPlayer.Object, rStart.Object);

		// Add
		var rAdd = new Mock<MessageReader>();
		rAdd.SetupSequence(r => r.ReadByte())
			.Returns((byte)InspectorInspectSystem.Ops.Add)
			.Returns((byte)2);
		system.UpdateSystem(mockPlayer.Object, rAdd.Object);

		// EndInspect
		var rEnd = new Mock<MessageReader>();
		rEnd.SetupSequence(r => r.ReadByte())
			.Returns((byte)InspectorInspectSystem.Ops.EndInspect);
		system.UpdateSystem(mockPlayer.Object, rEnd.Object);
	}
}
