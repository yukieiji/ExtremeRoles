using Moq;
using UnityEngine;

namespace ExtremeRoles.UnitTest.Mocks;

public class MathfMock : ISerialMockSetup
{
	public void Setup()
	{
		var mockClamp01 = new Mock<MockMathfClamp01Helper>();
		mockClamp01.Setup(h => h.Invoke(It.IsAny<float>())).Returns((float f) => Math.Clamp(f, 0f, 1f));
		MockMathfClamp01Helper.Instance = mockClamp01.Object;

		var mockClamp = new Mock<MockMathfClampHelper>();
		mockClamp.Setup(h => h.Invoke(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>())).Returns((float v, float min, float max) => Math.Clamp(v, min, max));
		MockMathfClampHelper.Instance = mockClamp.Object;

		var mockClamp2 = new Mock<MockMathfClampHelper2>();
		mockClamp2.Setup(h => h.Invoke(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns((int v, int min, int max) => Math.Clamp(v, min, max));
		MockMathfClampHelper2.Instance = mockClamp2.Object;

		var mockMax = new Mock<MockMathfMaxHelper>();
		mockMax.Setup(h => h.Invoke(It.IsAny<float>(), It.IsAny<float>())).Returns((float a, float b) => Math.Max(a, b));
		MockMathfMaxHelper.Instance = mockMax.Object;

		var mockMin = new Mock<MockMathfMinHelper>();
		mockMin.Setup(h => h.Invoke(It.IsAny<float>(), It.IsAny<float>())).Returns((float a, float b) => Math.Min(a, b));
		MockMathfMinHelper.Instance = mockMin.Object;

		var mockAbs = new Mock<MockMathfAbsHelper>();
		mockAbs.Setup(h => h.Invoke(It.IsAny<float>())).Returns((float f) => Math.Abs(f));
		MockMathfAbsHelper.Instance = mockAbs.Object;

		var mockCeilToInt = new Mock<MockMathfCeilToIntHelper>();
		mockCeilToInt.Setup(h => h.Invoke(It.IsAny<float>())).Returns((float f) => (int)Math.Ceiling(f));
		MockMathfCeilToIntHelper.Instance = mockCeilToInt.Object;
	}
}
