using Moq;
using UnityEngine;

namespace ExtremeRoles.UnitTest.Mocks;

public class Vector2Mock : ISerialMockSetup
{
	public void Setup()
	{
		var mockRight = new Mock<MockVector2get_rightHelper>();
		mockRight.Setup(x => x.Invoke()).Returns(new Vector2(1f, 0f));
		MockVector2get_rightHelper.Instance = mockRight.Object;
		var mockRightVec = new Mock<MockVector2get_rightVectorHelper>();
		mockRightVec.Setup(x => x.Invoke()).Returns(new Vector2(1f, 0f));
		MockVector2get_rightVectorHelper.Instance = mockRightVec.Object;

		var mockUp = new Mock<MockVector2get_upHelper>();
		mockUp.Setup(x => x.Invoke()).Returns(new Vector2(0f, 1f));
		MockVector2get_upHelper.Instance = mockUp.Object;
		var mockUpVec = new Mock<MockVector2get_upVectorHelper>();
		mockUpVec.Setup(x => x.Invoke()).Returns(new Vector2(0f, 1f));
		MockVector2get_upVectorHelper.Instance = mockUpVec.Object;

		var mockZero = new Mock<MockVector2get_zeroHelper>();
		mockZero.Setup(x => x.Invoke()).Returns(new Vector2(0f, 0f));
		MockVector2get_zeroHelper.Instance = mockZero.Object;
		var mockZeroVec = new Mock<MockVector2get_zeroVectorHelper>();
		mockZeroVec.Setup(x => x.Invoke()).Returns(new Vector2(0f, 0f));
		MockVector2get_zeroVectorHelper.Instance = mockZeroVec.Object;

		var mockDown = new Mock<MockVector2get_downHelper>();
		mockDown.Setup(x => x.Invoke()).Returns(new Vector2(0f, -1f));
		MockVector2get_downHelper.Instance = mockDown.Object;
		var mockDownVec = new Mock<MockVector2get_downVectorHelper>();
		mockDownVec.Setup(x => x.Invoke()).Returns(new Vector2(0f, -1f));
		MockVector2get_downVectorHelper.Instance = mockDownVec.Object;

		var mockOne = new Mock<MockVector2get_oneHelper>();
		mockOne.Setup(x => x.Invoke()).Returns(new Vector2(1f, 1f));
		MockVector2get_oneHelper.Instance = mockOne.Object;
		var mockOneVec = new Mock<MockVector2get_oneVectorHelper>();
		mockOneVec.Setup(x => x.Invoke()).Returns(new Vector2(1f, 1f));
		MockVector2get_oneVectorHelper.Instance = mockOneVec.Object;

		var mockMultiply = new Mock<MockVector2op_MultiplyHelper>();
		mockMultiply.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>()))
			.Returns((Vector2 a, Vector2 b) => new Vector2(a.x * b.x, a.y * b.y));
		MockVector2op_MultiplyHelper.Instance = mockMultiply.Object;

		var mockMultiply2 = new Mock<MockVector2op_MultiplyHelper2>();
		mockMultiply2.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<float>()))
			.Returns((Vector2 v, float f) => new Vector2(v.x * f, v.y * f));
		MockVector2op_MultiplyHelper2.Instance = mockMultiply2.Object;

		var mockMultiply3 = new Mock<MockVector2op_MultiplyHelper3>();
		mockMultiply3.Setup(x => x.Invoke(It.IsAny<float>(), It.IsAny<Vector2>()))
			.Returns((float f, Vector2 v) => new Vector2(v.x * f, v.y * f));
		MockVector2op_MultiplyHelper3.Instance = mockMultiply3.Object;

		var mockVec2Implicit = new Mock<MockVector2op_ImplicitHelper>();
		mockVec2Implicit.Setup(x => x.Invoke(It.IsAny<Vector3>()))
			.Returns((Vector3 v) => new Vector2(v.x, v.y));
		MockVector2op_ImplicitHelper.Instance = mockVec2Implicit.Object;

		var mockVec2Implicit2 = new Mock<MockVector2op_ImplicitHelper2>();
		mockVec2Implicit2.Setup(x => x.Invoke(It.IsAny<Vector2>()))
			.Returns((Vector2 v) => new Vector3(v.x, v.y, 0f));
		MockVector2op_ImplicitHelper2.Instance = mockVec2Implicit2.Object;
	}
}
