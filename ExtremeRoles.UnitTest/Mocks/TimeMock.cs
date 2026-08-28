using Moq;
using UnityEngine;

namespace ExtremeRoles.UnitTest.Mocks;

public class TimeMock : ISerialMockSetup
{
	public void Setup()
	{
		var mockDeltaTime = new Mock<MockTimeget_deltaTimeHelper>();
		mockDeltaTime.Setup(h => h.Invoke()).Returns(0.016f);
		MockTimeget_deltaTimeHelper.Instance = mockDeltaTime.Object;

		var mockTime = new Mock<MockTimeget_timeHelper>();
		mockTime.Setup(h => h.Invoke()).Returns(1.0f);
		MockTimeget_timeHelper.Instance = mockTime.Object;
	}
}
