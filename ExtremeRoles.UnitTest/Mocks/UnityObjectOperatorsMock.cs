using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ExtremeRoles.UnitTest.Mocks;

public class UnityObjectOperatorsMock : ISerialMockSetup
{
	public void Setup()
	{
		var mockEq = new Mock<MockObjectop_EqualityHelper>();
		mockEq.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<UnityEngine.Object>()))
			.Returns((UnityEngine.Object x, UnityEngine.Object y) =>
			{
				if (ReferenceEquals(x, y))
				{
					return true;
				}
				if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
				{
					return false;
				}
				return ReferenceEquals(x, y);
			});
		MockObjectop_EqualityHelper.Instance = mockEq.Object;

		var mockIneq = new Mock<MockObjectop_InequalityHelper>();
		mockIneq.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<UnityEngine.Object>()))
			.Returns((UnityEngine.Object x, UnityEngine.Object y) =>
			{
				if (ReferenceEquals(x, y))
				{
					return false;
				}
				if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
				{
					return true;
				}
				return !ReferenceEquals(x, y);
			});
		MockObjectop_InequalityHelper.Instance = mockIneq.Object;

		var mockImplicit = new Mock<MockObjectop_ImplicitHelper>();
		mockImplicit.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>()))
			.Returns((UnityEngine.Object obj) => !ReferenceEquals(obj, null));
		MockObjectop_ImplicitHelper.Instance = mockImplicit.Object;

		var mockUnityActionImplicit = new Mock<MockUnityActionop_ImplicitHelper>();
		mockUnityActionImplicit.Setup(x => x.Invoke(It.IsAny<Action>()))
			.Returns((Action act) => act != null ? new UnityEngine.Events.UnityAction(IntPtr.Zero) : null!);
		MockUnityActionop_ImplicitHelper.Instance = mockUnityActionImplicit.Object;

		var mockDestroy = new Mock<MockObjectDestroyHelper>();
		mockDestroy.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<float>()));
		MockObjectDestroyHelper.Instance = mockDestroy.Object;

		var mockDestroy2 = new Mock<MockObjectDestroyHelper2>();
		mockDestroy2.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>()));
		MockObjectDestroyHelper2.Instance = mockDestroy2.Object;

		var mockMiscDestroy = new Mock<MockMiscDestroyHelper>();
		mockMiscDestroy.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>()));
		MockMiscDestroyHelper.Instance = mockMiscDestroy.Object;

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper>();
		mockFindObjects.Setup(x => x.Invoke(It.IsAny<Il2CppSystem.Type>())).Returns(new Il2CppReferenceArray<UnityEngine.Object>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper.Instance = mockFindObjects.Object;

		var mockFindObjects2 = new Mock<MockObjectFindObjectsOfTypeHelper2>();
		mockFindObjects2.Setup(x => x.Invoke(It.IsAny<Il2CppSystem.Type>(), It.IsAny<bool>())).Returns((Il2CppReferenceArray<UnityEngine.Object>)null!);
		MockObjectFindObjectsOfTypeHelper2.Instance = mockFindObjects2.Object;

		MockObjectFindObjectsOfTypeHelper3.Instance = new Mock<MockObjectFindObjectsOfTypeHelper3>().Object;
		MockObjectFindObjectsOfTypeHelper4.Instance = new Mock<MockObjectFindObjectsOfTypeHelper4>().Object;

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
