using AmongUs.GameOptions;
using Moq;
using System;

namespace ExtremeRoles.UnitTest.Mocks;

public class GameOptionsManagerMock : ISerialMockSetup
{
	public void Setup()
	{
		var mockOptions = new Mock<IGameOptions>(IntPtr.Zero);
		mockOptions.SetupGet(o => o.MaxPlayers).Returns(15);
		mockOptions.SetupGet(o => o.NumImpostors).Returns(3);
		mockOptions.SetupGet(o => o.GameMode).Returns(GameModes.Normal);

		var mockOptionsMgr = new Mock<GameOptionsManager>(IntPtr.Zero);
		mockOptionsMgr.SetupGet(m => m.CurrentGameOptions).Returns(mockOptions.Object);
		mockOptionsMgr.SetupGet(m => m.currentGameOptions).Returns(mockOptions.Object);

		var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
		mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockOptionsMgr.Object);
		MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
	}
}
