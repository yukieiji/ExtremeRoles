using System;
using ExtremeRoles.Patches;
using ExtremeRoles.Module.CustomMonoBehaviour;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Patches;

public sealed class LogicOptionsSyncOptionsPatchTests
{
	[Fact]
	public void Prefix_WhenAmongUsClientNull_ReturnsFalse()
	{
		// Arrange
		MockSetupHelper.SetupUnityCommonMocks();
		IMockAmongUsClientget_Instance.Instance = new Mock<IMockAmongUsClientget_Instance>().Object;

		// Act
		var result = LogicOptionsSyncOptionsPatch.Prefix();

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenNotAmHost_ReturnsFalse()
	{
		// Arrange
		MockSetupHelper.SetupUnityCommonMocks();
		var mockClient = new Mock<AmongUsClient>(IntPtr.Zero);
		mockClient.SetupGet(c => c.AmHost).Returns(false);

		var mockClientHelper = new Mock<IMockAmongUsClientget_Instance>();
		mockClientHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
		IMockAmongUsClientget_Instance.Instance = mockClientHelper.Object;

		// Act
		var result = LogicOptionsSyncOptionsPatch.Prefix();

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenGameManagerNull_ReturnsFalse()
	{
		// Arrange
		MockSetupHelper.SetupUnityCommonMocks();
		var mockClient = new Mock<AmongUsClient>(IntPtr.Zero);
		mockClient.SetupGet(c => c.AmHost).Returns(true);

		var mockClientHelper = new Mock<IMockAmongUsClientget_Instance>();
		mockClientHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
		IMockAmongUsClientget_Instance.Instance = mockClientHelper.Object;

		IMockGameManagerget_Instance.Instance = new Mock<IMockGameManagerget_Instance>().Object;

		// Act
		var result = LogicOptionsSyncOptionsPatch.Prefix();

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenLogicOptionsNull_ReturnsFalse()
	{
		// Arrange
		MockSetupHelper.SetupUnityCommonMocks();
		var mockClient = new Mock<AmongUsClient>(IntPtr.Zero);
		mockClient.SetupGet(c => c.AmHost).Returns(true);

		var mockClientHelper = new Mock<IMockAmongUsClientget_Instance>();
		mockClientHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
		IMockAmongUsClientget_Instance.Instance = mockClientHelper.Object;

		var mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns((LogicOptions)null!);

		var mockGameManagerHelper = new Mock<IMockGameManagerget_Instance>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		IMockGameManagerget_Instance.Instance = mockGameManagerHelper.Object;

		// Act
		var result = LogicOptionsSyncOptionsPatch.Prefix();

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenValidHostAndLogicOptions_CallsSyncOptionAndReturnsFalse()
	{
		// Arrange
		MockSetupHelper.SetupUnityCommonMocks();
		var mockClient = new Mock<AmongUsClient>(IntPtr.Zero);
		mockClient.SetupGet(c => c.AmHost).Returns(true);

		var mockClientHelper = new Mock<IMockAmongUsClientget_Instance>();
		mockClientHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
		IMockAmongUsClientget_Instance.Instance = mockClientHelper.Object;

		var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
		var mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);

		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		var syncer = new LazyOptionSyncer(IntPtr.Zero);

		mockGameObject.Setup(g => g.TryGetComponent<LazyOptionSyncer>(out syncer)).Returns(true);
		mockGameManager.SetupGet(g => g.gameObject).Returns(mockGameObject.Object);

		var mockGameManagerHelper = new Mock<IMockGameManagerget_Instance>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		IMockGameManagerget_Instance.Instance = mockGameManagerHelper.Object;

		// Act
		var result = LogicOptionsSyncOptionsPatch.Prefix();

		// Assert
		Assert.False(result);
	}
}
