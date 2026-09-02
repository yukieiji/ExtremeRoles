using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class HostUpdateSystemTests
{
	public HostUpdateSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void BasicInterfaceMethods_And_Properties()
	{
		var system = new HostUpdateSystem();
		Assert.False(system.IsDirty);

		system.MarkClean();
		system.Deserialize(null!, false);
		system.Reset(ResetTiming.MeetingStart, null);
		system.Serialize(null!, false);
		system.UpdateSystem(null!, null!);
	}

	[Fact]
	public void Add_Get_Remove_WorkCorrectly()
	{
		var system = new HostUpdateSystem();
		var mockUpdatable1 = new Mock<IUpdatableObject>();
		var mockUpdatable2 = new Mock<IUpdatableObject>();

		system.Add(mockUpdatable1.Object);
		system.Add(mockUpdatable2.Object);

		Assert.Same(mockUpdatable1.Object, system.Get(0));
		Assert.Same(mockUpdatable2.Object, system.Get(1));

		system.Remove(0);
		mockUpdatable1.Verify(x => x.Clear(), Times.Once);
		Assert.Same(mockUpdatable2.Object, system.Get(0));

		system.Remove(mockUpdatable2.Object);
		mockUpdatable2.Verify(x => x.Clear(), Times.Once);
	}

	[Fact]
	public void Deteriorate_WhenNotHost_DoesNotUpdate()
	{
		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(false);

		var system = new HostUpdateSystem();
		var mockUpdatable = new Mock<IUpdatableObject>();
		system.Add(mockUpdatable.Object);

		system.Deteriorate(1.0f);
		mockUpdatable.Verify(x => x.Update(It.IsAny<int>()), Times.Never);
	}

	[Fact]
	public void Deteriorate_WhenHost_UpdatesObjects()
	{
		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(true);

		var system = new HostUpdateSystem();
		var mockUpdatable = new Mock<IUpdatableObject>();
		system.Add(mockUpdatable.Object);

		system.Deteriorate(1.0f);
		mockUpdatable.Verify(x => x.Update(0), Times.Once);
	}
}
