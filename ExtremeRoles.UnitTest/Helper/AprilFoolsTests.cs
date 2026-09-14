using System;
using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Helper;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Helper;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class AprilFoolsTests
{
	public AprilFoolsTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
	}

	[Fact]
	public void UpdateApilSkinToggle_WhenServerManagerInstanceIsNull_SetsToggleInactive()
	{
		// Arrange
		var mockHelper = new Mock<MockDestroyableSingletonget_InstanceHelper<ServerManager>>();
		mockHelper.Setup(x => x.Invoke()).Returns((ServerManager)null!);
		MockDestroyableSingletonget_InstanceHelper<ServerManager>.Instance = mockHelper.Object;

		var mockCreateGameOptions = new Mock<CreateGameOptions>(IntPtr.Zero);
		var mockToggle = new Mock<GameObject>(IntPtr.Zero);
		mockCreateGameOptions.SetupGet(c => c.AprilFoolsToggle).Returns(mockToggle.Object);

		// Act
		AprilFools.UpdateApilSkinToggle(mockCreateGameOptions.Object);

		// Assert
		mockToggle.Verify(g => g.SetActive(false), Times.Once);
	}

	[Fact]
	public void UpdateApilSkinToggle_WhenServerManagerIsNotCustomServer_SetsToggleInactive()
	{
		// Arrange
		var mockRegion = new Mock<IRegionInfo>(IntPtr.Zero);
		mockRegion.SetupGet(r => r.Name).Returns("North America");

		var mockServerManager = MockSetupHelper.SetupDestroyableSingletonMock<ServerManager>();
		mockServerManager.SetupGet(m => m.CurrentRegion).Returns(mockRegion.Object);

		var mockCreateGameOptions = new Mock<CreateGameOptions>(IntPtr.Zero);
		var mockToggle = new Mock<GameObject>(IntPtr.Zero);
		mockCreateGameOptions.SetupGet(c => c.AprilFoolsToggle).Returns(mockToggle.Object);

		// Act
		AprilFools.UpdateApilSkinToggle(mockCreateGameOptions.Object);

		// Assert
		mockToggle.Verify(g => g.SetActive(false), Times.Once);
	}

	[Theory]
	[InlineData(IRegionInfoExtension.FullCustomServerName)]
	[InlineData(IRegionInfoExtension.ExROfficialServerTokyoManinName)]
	public void UpdateApilSkinToggle_WhenServerManagerIsCustomServer_SetsToggleActive(string regionName)
	{
		// Arrange
		var mockRegion = new Mock<IRegionInfo>(IntPtr.Zero);
		mockRegion.SetupGet(r => r.Name).Returns(regionName);

		var mockServerManager = MockSetupHelper.SetupDestroyableSingletonMock<ServerManager>();
		mockServerManager.SetupGet(m => m.CurrentRegion).Returns(mockRegion.Object);

		var mockCreateGameOptions = new Mock<CreateGameOptions>(IntPtr.Zero);
		var mockToggle = new Mock<GameObject>(IntPtr.Zero);
		mockCreateGameOptions.SetupGet(c => c.AprilFoolsToggle).Returns(mockToggle.Object);

		// Act
		AprilFools.UpdateApilSkinToggle(mockCreateGameOptions.Object);

		// Assert
		mockToggle.Verify(g => g.SetActive(true), Times.Once);
	}
}
