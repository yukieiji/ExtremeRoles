using Moq;
using UnityEngine;

namespace ExtremeRoles.UnitTest.Mocks;

public class UnityObjectInstantiateMock : ISerialMockSetup
{
	public void Setup()
	{
		var m5 = new Mock<MockObjectInstantiateHelper5>();
		m5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object orig, Transform parent) => orig);
		MockObjectInstantiateHelper5.Instance = m5.Object;

		var m10 = new Mock<MockObjectInstantiateHelper10>();
		m10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object orig, Transform parent) => orig);
		MockObjectInstantiateHelper10.Instance = m10.Object;

		var m7 = new Mock<MockObjectInstantiateHelper7>();
		m7.Setup(x => x.Invoke(It.IsAny<Material>()))
			.Returns((Material orig) => orig);
		MockObjectInstantiateHelper7.Instance = m7.Object;

		var m = new Mock<MockObjectInstantiateHelper>();
		m.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Vector3>(), It.IsAny<Quaternion>()))
			.Returns((UnityEngine.Object orig, Vector3 v, Quaternion q) => orig);
		MockObjectInstantiateHelper.Instance = m.Object;
	}
}
