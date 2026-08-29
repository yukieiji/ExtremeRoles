using ExtremeRoles.Module.SystemType;
using Moq;

namespace ExtremeRoles.UnitTest.Mocks;

public class ShipStatusMock : ISerialMockSetup
{
	public Mock<ShipStatus> MockShipStatus { get; private set; } = null!;
	public Mock<Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, ISystemType>> MockSystems { get; private set; } = null!;

	public void Setup()
	{
		MockShipStatus = new Mock<ShipStatus>();
		MockSystems = new Mock<Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, ISystemType>>(System.IntPtr.Zero);
		MockShipStatus.SetupGet(s => s.Systems).Returns(MockSystems.Object);

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(MockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;
	}
}
