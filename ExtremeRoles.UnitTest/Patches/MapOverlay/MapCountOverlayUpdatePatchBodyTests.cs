using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

	private static AdminDeviceOption CreateAdminDeviceOption(bool disable, bool enableLimit, float limitTime)
	{
		var adminOpt = (AdminDeviceOption)RuntimeHelpers.GetUninitializedObject(typeof(AdminDeviceOption));
		var disableField = typeof(AdminDeviceOption).GetField("<Disable>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		disableField?.SetValue(adminOpt, disable);

		var enableLimitField = typeof(AdminDeviceOption).GetField("<EnableLimit>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		enableLimitField?.SetValue(adminOpt, enableLimit);

		var limitTimeField = typeof(AdminDeviceOption).GetField("<LimitTime>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		limitTimeField?.SetValue(adminOpt, limitTime);

		return adminOpt;
	}

	[Fact]
	public void Initialize_WhenTryGetGameContextReturnsFalse_DoesNotThrow()
	{
		// Arrange
		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		// Act & Assert
		patchBody.Initialize();
	}

	[Fact]
	public void Initialize_WhenTryGetGameContextReturnsTrue_SetsTimerAndLogs()
	{
		// Arrange
		var adminOpt = CreateAdminDeviceOption(disable: false, enableLimit: true, limitTime: 30.0f);

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
		mockLogger.Verify(l => l.LogTrace(It.IsAny<string>()), Times.AtLeastOnce);
	}

	[Fact]
	public void Prefix_WhenTryGetGameContextReturnsFalse_ReturnsTrue()
	{
		// Arrange
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
	}

	[Fact]
	public void Postfix_WhenTryGetGameContextReturnsFalse_ReturnsEarly()
	{
		// Arrange
		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var overlay = new MapCountOverlay();

		// Act & Assert
		patchBody.Postfix(overlay);
	}

	[Fact]
	public void Postfix_WhenAdminDisabled_ReturnsEarly()
	{
		// Arrange
		var adminOpt = CreateAdminDeviceOption(disable: true, enableLimit: true, limitTime: 30.0f);

		var mockGlobalOption = new Mock<IShipGlobalOption>();
		mockGlobalOption.SetupGet(g => g.Admin).Returns(adminOpt);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		var dummyRole = (Supervisor)RuntimeHelpers.GetUninitializedObject(typeof(Supervisor));
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

		// Act & Assert
		patchBody.Postfix(overlay);
	}
}
