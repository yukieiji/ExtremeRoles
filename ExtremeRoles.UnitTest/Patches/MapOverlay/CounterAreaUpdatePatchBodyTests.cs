using System;
using System.Collections.Generic;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Patches.MapOverlay;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Patches.MapOverlay;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class CounterAreaUpdatePatchBodyTests : IDisposable
{
	public CounterAreaUpdatePatchBodyTests()
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
	public void Postfix_WhenTryGetGameContextReturnsFalse_PlayerColorsUnchanged()
	{
		// Arrange
		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var mockLogger = new Mock<IModLogger>();
		var mapCountOverlayPatchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var patchBody = new CounterAreaUpdatePatchBody(mockRuntime.Object, mapCountOverlayPatchBody);

		var counterArea = new CounterArea();

		// Act
		patchBody.Postfix(counterArea);

		// Assert
		Assert.Empty(mapCountOverlayPatchBody.PlayerColors);
	}

	[Fact]
	public void Postfix_WhenRolesAllIsEmpty_PlayerColorsUnchanged()
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
		var mapCountOverlayPatchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var patchBody = new CounterAreaUpdatePatchBody(mockRuntime.Object, mapCountOverlayPatchBody);

		var counterArea = new CounterArea();

		// Act
		patchBody.Postfix(counterArea);

		// Assert
		Assert.Empty(mapCountOverlayPatchBody.PlayerColors);
	}

	[Fact]
	public void Postfix_WhenLocalPlayerIsNotSupervisor_PlayerColorsUnchanged()
	{
		// Arrange
		var mockRoles = new Mock<INomalGameRoleContainer>();
		var dummyRole = new Supervisor();
		mockRoles.SetupGet(r => r.All).Returns(new Dictionary<byte, SingleRoleBase> { { 0, dummyRole } });
		mockRoles.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns((Supervisor?)null);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var mockLogger = new Mock<IModLogger>();
		var mapCountOverlayPatchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var patchBody = new CounterAreaUpdatePatchBody(mockRuntime.Object, mapCountOverlayPatchBody);

		var counterArea = new CounterArea();

		// Act
		patchBody.Postfix(counterArea);

		// Assert
		Assert.Empty(mapCountOverlayPatchBody.PlayerColors);
	}
}
