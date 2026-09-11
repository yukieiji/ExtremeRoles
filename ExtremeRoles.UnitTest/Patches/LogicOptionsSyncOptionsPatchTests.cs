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
		MockAmongUsClientget_InstanceHelper.Instance = new Mock<MockAmongUsClientget_InstanceHelper>().Object;

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
		MockSetupHelper.SetupPlayerControlMocks();
		var mockClient = new Mock<AmongUsClient>(IntPtr.Zero);
		mockClient.SetupGet(c => c.AmHost).Returns(false);

		var mockClientHelper = new Mock<MockAmongUsClientget_InstanceHelper>();
		mockClientHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
		MockAmongUsClientget_InstanceHelper.Instance = mockClientHelper.Object;

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
		MockSetupHelper.SetupPlayerControlMocks();

		var mockAprilFools = new Mock<MockAprilFoolsModeget_IsAprilFoolsModeToggledOnHelper>();
		mockAprilFools.Setup(x => x.Invoke()).Returns(false);
		MockAprilFoolsModeget_IsAprilFoolsModeToggledOnHelper.Instance = mockAprilFools.Object;

		var mockClient = new Mock<AmongUsClient>(IntPtr.Zero);
		mockClient.SetupGet(c => c.AmHost).Returns(true);

		var mockClientHelper = new Mock<MockAmongUsClientget_InstanceHelper>();
		mockClientHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
		MockAmongUsClientget_InstanceHelper.Instance = mockClientHelper.Object;

		MockGameManagerget_InstanceHelper.Instance = new Mock<MockGameManagerget_InstanceHelper>().Object;

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

		var mockClientHelper = new Mock<MockAmongUsClientget_InstanceHelper>();
		mockClientHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
		MockAmongUsClientget_InstanceHelper.Instance = mockClientHelper.Object;

		var mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns((LogicOptions)null!);

		var mockGameManagerHelper = new Mock<MockGameManagerget_InstanceHelper>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		MockGameManagerget_InstanceHelper.Instance = mockGameManagerHelper.Object;

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

		var mockClientHelper = new Mock<MockAmongUsClientget_InstanceHelper>();
		mockClientHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
		MockAmongUsClientget_InstanceHelper.Instance = mockClientHelper.Object;

		var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
		var mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);

		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		var syncer = new LazyOptionSyncer(IntPtr.Zero);

		mockGameObject.Setup(g => g.TryGetComponent<LazyOptionSyncer>(out syncer)).Returns(true);
		mockGameManager.SetupGet(g => g.gameObject).Returns(mockGameObject.Object);

		var mockGameManagerHelper = new Mock<MockGameManagerget_InstanceHelper>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		MockGameManagerget_InstanceHelper.Instance = mockGameManagerHelper.Object;

		// Act
		var result = LogicOptionsSyncOptionsPatch.Prefix();

		// Assert
		Assert.False(result);
	}
}
