using Moq;
using UnityEngine;

namespace ExtremeRoles.UnitTest.Mocks;

public class PaletteMock : ISerialMockSetup
{
	public void Setup()
	{
		var mockCrewmateBlue = new Mock<MockPaletteget_CrewmateBlueHelper>();
		mockCrewmateBlue.Setup(x => x.Invoke()).Returns(new Color(0.5f, 0.5f, 1f, 1f));
		MockPaletteget_CrewmateBlueHelper.Instance = mockCrewmateBlue.Object;

		var mockImpostorRed = new Mock<MockPaletteget_ImpostorRedHelper>();
		mockImpostorRed.Setup(x => x.Invoke()).Returns(new Color(1f, 0.2f, 0.2f, 1f));
		MockPaletteget_ImpostorRedHelper.Instance = mockImpostorRed.Object;

		var mockWhite = new Mock<MockPaletteget_WhiteHelper>();
		mockWhite.Setup(x => x.Invoke()).Returns(new Color(1f, 1f, 1f, 1f));
		MockPaletteget_WhiteHelper.Instance = mockWhite.Object;

		var mockClearWhite = new Mock<MockPaletteget_ClearWhiteHelper>();
		mockClearWhite.Setup(x => x.Invoke()).Returns(new Color(1f, 1f, 1f, 0f));
		MockPaletteget_ClearWhiteHelper.Instance = mockClearWhite.Object;

		var mockBlack = new Mock<MockPaletteget_BlackHelper>();
		mockBlack.Setup(x => x.Invoke()).Returns(new Color(0f, 0f, 0f, 1f));
		MockPaletteget_BlackHelper.Instance = mockBlack.Object;

		var mockEnabledColor = new Mock<MockPaletteget_EnabledColorHelper>();
		mockEnabledColor.Setup(x => x.Invoke()).Returns(new Color(1f, 1f, 1f, 1f));
		MockPaletteget_EnabledColorHelper.Instance = mockEnabledColor.Object;

		var mockDisabledClear = new Mock<MockPaletteget_DisabledClearHelper>();
		mockDisabledClear.Setup(x => x.Invoke()).Returns(new Color(0f, 0f, 0f, 0f));
		MockPaletteget_DisabledClearHelper.Instance = mockDisabledClear.Object;

		var mockDisabledGrey = new Mock<MockPaletteget_DisabledGreyHelper>();
		mockDisabledGrey.Setup(x => x.Invoke()).Returns(new Color(0.5f, 0.5f, 0.5f, 1f));
		MockPaletteget_DisabledGreyHelper.Instance = mockDisabledGrey.Object;
	}
}
