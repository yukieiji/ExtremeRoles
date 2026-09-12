using System;
using System.Collections.Generic;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;
using ExtremeRoles.Patches.MapOverlay;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Patches.MapOverlay;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class MapCountOverlayUpdatePatchBodyTests : IDisposable
{
	public MapCountOverlayUpdatePatchBodyTests()
	{
		ResetState();
	}

	public void Dispose()
	{
		ResetState();
	}

	private static void ResetState()
	{
		MockSetupHelper.SetupUnityCommonMocks();
	}

	[Fact]
	public void PlayerColors_Initially_IsEmpty()
	{
		// Arrange
		var mockRuntime = new Mock<IGameRuntime>();
		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		// Act & Assert
		Assert.NotNull(patchBody.PlayerColors);
		Assert.Empty(patchBody.PlayerColors);
	}

	[Fact]
	public void IsAbilityUse_WhenNoLocalPlayerRole_ThrowsArgumentNullException()
	{
		// Arrange
		var mockRuntime = new Mock<IGameRuntime>();
		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => patchBody.IsAbilityUse());
	}

	[Fact]
	public void Initialize_WhenTryGetGameContextReturnsFalse_LogsNothing()
	{
		// Arrange
		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		// Act
		patchBody.Initialize();

		// Assert
		mockLogger.Verify(l => l.LogTrace(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void Initialize_WhenTryGetGameContextReturnsTrue_LogsAdminConditionDetails()
	{
		// Arrange
		var adminOpt = new AdminDeviceOption();

		var mockGlobalOption = new Mock<IShipGlobalOption>();
		mockGlobalOption.SetupGet(g => g.Admin).Returns(adminOpt);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOption.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		// Act
		patchBody.Initialize();

		// Assert
		mockLogger.Verify(l => l.LogTrace("---- AdminCondition ----"), Times.Once);
		mockLogger.Verify(l => l.LogTrace(It.Is<string>(s => s.StartsWith("IsRemoveAdmin:"))), Times.Once);
		mockLogger.Verify(l => l.LogTrace(It.Is<string>(s => s.StartsWith("EnableAdminLimit:"))), Times.Once);
		mockLogger.Verify(l => l.LogTrace(It.Is<string>(s => s.StartsWith("AdminTime:"))), Times.Once);
	}

	[Fact]
	public void Prefix_WhenTryGetGameContextReturnsFalse_ReturnsTrueAndDoesNotAccessRoles()
	{
		// Arrange
		var mockContext = new Mock<IGameContext>();
		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var overlay = new MapCountOverlay();

		// Act
		bool result = patchBody.Prefix(overlay);

		// Assert
		Assert.True(result);
		mockContext.VerifyGet(c => c.Roles, Times.Never);
	}

	[Fact]
	public void Prefix_WhenRolesAllIsEmpty_ReturnsTrue()
	{
		// Arrange
		var mockRoles = new Mock<INomalGameRoleContainer>();
		mockRoles.SetupGet(r => r.All).Returns(new Dictionary<byte, SingleRoleBase>());

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var overlay = new MapCountOverlay();

		// Act
		bool result = patchBody.Prefix(overlay);

		// Assert
		Assert.True(result);
		mockRoles.VerifyGet(r => r.All, Times.Once);
	}

	[Fact]
	public void Postfix_WhenTryGetGameContextReturnsFalse_DoesNotQueryGlobalOption()
	{
		// Arrange
		var mockGlobalOption = new Mock<IShipGlobalOption>();

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOption.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var overlay = new MapCountOverlay();

		// Act
		patchBody.Postfix(overlay);

		// Assert
		mockGlobalOption.VerifyGet(g => g.Admin, Times.Never);
	}

	[Fact]
	public void Postfix_WhenRolesAllIsEmpty_DoesNotQueryGlobalOption()
	{
		// Arrange
		var mockRoles = new Mock<INomalGameRoleContainer>();
		mockRoles.SetupGet(r => r.All).Returns(new Dictionary<byte, SingleRoleBase>());

		var mockGlobalOption = new Mock<IShipGlobalOption>();

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOption.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var overlay = new MapCountOverlay();

		// Act
		patchBody.Postfix(overlay);

		// Assert
		mockGlobalOption.VerifyGet(g => g.Admin, Times.Never);
	}

	[Fact]
	public void Postfix_WhenAdminDisabled_QueriesGlobalOptionOnce()
	{
		// Arrange
		var adminOpt = new AdminDeviceOption();

		var mockGlobalOption = new Mock<IShipGlobalOption>();
		mockGlobalOption.SetupGet(g => g.Admin).Returns(adminOpt);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		var dummyRole = new Supervisor();
		mockRoles.SetupGet(r => r.All).Returns(new Dictionary<byte, SingleRoleBase> { { 0, dummyRole } });

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOption.Object);
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var overlay = new MapCountOverlay();

		// Act
		patchBody.Postfix(overlay);

		// Assert
		mockGlobalOption.VerifyGet(g => g.Admin, Times.Once);
	}
}
