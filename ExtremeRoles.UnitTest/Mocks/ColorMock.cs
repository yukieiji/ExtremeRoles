using Moq;
using UnityEngine;

namespace ExtremeRoles.UnitTest.Mocks;

public class ColorMock : ISerialMockSetup
{
	public void Setup()
	{
		var mockColorEq = new Mock<MockColorop_EqualityHelper>();
		mockColorEq.Setup(x => x.Invoke(It.IsAny<Color>(), It.IsAny<Color>()))
			.Returns((Color a, Color b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a);
		MockColorop_EqualityHelper.Instance = mockColorEq.Object;

		var mockColorIneq = new Mock<MockColorop_InequalityHelper>();
		mockColorIneq.Setup(x => x.Invoke(It.IsAny<Color>(), It.IsAny<Color>()))
			.Returns((Color a, Color b) => a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a);
		MockColorop_InequalityHelper.Instance = mockColorIneq.Object;

		var mockRandomInitState = new Mock<MockRandomInitStateHelper>();
		MockRandomInitStateHelper.Instance = mockRandomInitState.Object;

		MockColor32op_ImplicitHelper.Instance = new Mock<MockColor32op_ImplicitHelper>().Object;
		MockColor32op_ImplicitHelper2.Instance = new Mock<MockColor32op_ImplicitHelper2>().Object;
		MockColorop_ImplicitHelper.Instance = new Mock<MockColorop_ImplicitHelper>().Object;
		MockColorop_ImplicitHelper2.Instance = new Mock<MockColorop_ImplicitHelper2>().Object;

		var mockWhite = new Mock<MockColorget_whiteHelper>();
		mockWhite.Setup(x => x.Invoke()).Returns(new Color(1f, 1f, 1f, 1f));
		MockColorget_whiteHelper.Instance = mockWhite.Object;

		var mockBlack = new Mock<MockColorget_blackHelper>();
		mockBlack.Setup(x => x.Invoke()).Returns(new Color(0f, 0f, 0f, 1f));
		MockColorget_blackHelper.Instance = mockBlack.Object;

		var mockBlue = new Mock<MockColorget_blueHelper>();
		mockBlue.Setup(x => x.Invoke()).Returns(new Color(0f, 0f, 1f, 1f));
		MockColorget_blueHelper.Instance = mockBlue.Object;

		var mockClear = new Mock<MockColorget_clearHelper>();
		mockClear.Setup(x => x.Invoke()).Returns(new Color(0f, 0f, 0f, 0f));
		MockColorget_clearHelper.Instance = mockClear.Object;

		var mockCyan = new Mock<MockColorget_cyanHelper>();
		mockCyan.Setup(x => x.Invoke()).Returns(new Color(0f, 1f, 1f, 1f));
		MockColorget_cyanHelper.Instance = mockCyan.Object;

		var mockGray = new Mock<MockColorget_grayHelper>();
		mockGray.Setup(x => x.Invoke()).Returns(new Color(0.5f, 0.5f, 0.5f, 1f));
		MockColorget_grayHelper.Instance = mockGray.Object;

		var mockGreen = new Mock<MockColorget_greenHelper>();
		mockGreen.Setup(x => x.Invoke()).Returns(new Color(0f, 1f, 0f, 1f));
		MockColorget_greenHelper.Instance = mockGreen.Object;

		var mockGrey = new Mock<MockColorget_greyHelper>();
		mockGrey.Setup(x => x.Invoke()).Returns(new Color(0.5f, 0.5f, 0.5f, 1f));
		MockColorget_greyHelper.Instance = mockGrey.Object;

		var mockMagenta = new Mock<MockColorget_magentaHelper>();
		mockMagenta.Setup(x => x.Invoke()).Returns(new Color(1f, 0f, 1f, 1f));
		MockColorget_magentaHelper.Instance = mockMagenta.Object;

		var mockRed = new Mock<MockColorget_redHelper>();
		mockRed.Setup(x => x.Invoke()).Returns(new Color(1f, 0f, 0f, 1f));
		MockColorget_redHelper.Instance = mockRed.Object;

		var mockYellow = new Mock<MockColorget_yellowHelper>();
		mockYellow.Setup(x => x.Invoke()).Returns(new Color(1f, 0.92f, 0.016f, 1f));
		MockColorget_yellowHelper.Instance = mockYellow.Object;
	}
}
