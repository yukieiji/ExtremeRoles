using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class SurrogatorGurdSystemTests
{
	public SurrogatorGurdSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void GuardNum_AddGuardNum_CanGuard()
	{
		var system = new SurrogatorGurdSystem(15.0f);
		Assert.Equal(0, system.GuardNum);
		Assert.Equal(15.0f, system.PreventKillTime);
		Assert.False(system.IsDirty);
		Assert.False(system.CanGuard(1));

		system.AddGuardNum(2);
		Assert.Equal(2, system.GuardNum);

		system.MarkClean();
		system.Reset(ResetTiming.MeetingStart, null);
		system.Serialize(null!, false);
		system.Deserialize(null!, false);
	}

	[Fact]
	public void UpdateSystem_Add_Reduce()
	{
		var system = new SurrogatorGurdSystem(10.0f);

		// Add
		var rAdd = new Mock<MessageReader>();
		rAdd.Setup(r => r.ReadByte()).Returns((byte)SurrogatorGurdSystem.Ops.Add);
		system.UpdateSystem(null!, rAdd.Object);
		Assert.Equal(1, system.GuardNum);

		// Reduce
		var rReduce = new Mock<MessageReader>();
		rReduce.Setup(r => r.ReadByte()).Returns((byte)SurrogatorGurdSystem.Ops.Reduce);
		system.UpdateSystem(null!, rReduce.Object);
		Assert.Equal(0, system.GuardNum);

		// Default
		var rDefault = new Mock<MessageReader>();
		rDefault.Setup(r => r.ReadByte()).Returns((byte)255);
		system.UpdateSystem(null!, rDefault.Object);
	}
}
