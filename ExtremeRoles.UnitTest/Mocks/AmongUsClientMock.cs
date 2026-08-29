using Hazel;
using Moq;

namespace ExtremeRoles.UnitTest.Mocks;

public class AmongUsClientMock : ISerialMockSetup
{
	public Mock<AmongUsClient> MockAmongUsClient { get; private set; } = null!;

	public void Setup()
	{
		MockAmongUsClient = new Mock<AmongUsClient>();
		var mockWriter = new Mock<MessageWriter>(System.IntPtr.Zero);
		MockAmongUsClient.Setup(c => c.StartRpcImmediately(
			It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockHelper = new Mock<MockAmongUsClientget_InstanceHelper>();
		mockHelper.Setup(h => h.Invoke()).Returns(MockAmongUsClient.Object);
		MockAmongUsClientget_InstanceHelper.Instance = mockHelper.Object;
	}
}
