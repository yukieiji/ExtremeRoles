using Moq;
using System;
using System.Collections.Generic;
using System.Text;
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

		MockColorget_blackHelper.Instance = new Mock<MockColorget_blackHelper>().Object;
		MockColorget_blueHelper.Instance = new Mock<MockColorget_blueHelper>().Object;
		MockColorget_clearHelper.Instance = new Mock<MockColorget_clearHelper>().Object;
		MockColorget_cyanHelper.Instance = new Mock<MockColorget_cyanHelper>().Object;
		MockColorget_grayHelper.Instance = new Mock<MockColorget_grayHelper>().Object;
		MockColorget_greenHelper.Instance = new Mock<MockColorget_greenHelper>().Object;
		MockColorget_greyHelper.Instance = new Mock<MockColorget_greyHelper>().Object;
		MockColorget_magentaHelper.Instance = new Mock<MockColorget_magentaHelper>().Object;
		MockColorget_redHelper.Instance = new Mock<MockColorget_redHelper>().Object;
		MockColorget_whiteHelper.Instance = new Mock<MockColorget_whiteHelper>().Object;
		MockColorget_yellowHelper.Instance = new Mock<MockColorget_yellowHelper>().Object;
	}
}
