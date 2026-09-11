using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.Patches.Ship;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Patches;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ShipStatusOnEnablePatchBodyTests : IDisposable
{
	public ShipStatusOnEnablePatchBodyTests()
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

	private static EmergencyTaskOption CreateUninitializedEmergencyTaskOption()
	{
		return (EmergencyTaskOption)RuntimeHelpers.GetUninitializedObject(typeof(EmergencyTaskOption));
	}

	[Fact]
	public void StaticPostfix_WhenBodyIsNull_ResolvesServiceAndExecutesPostfix()
	{
		// Arrange
		var emergencyOption = CreateUninitializedEmergencyTaskOption();
		var mockGlobalOption = new Mock<IShipGlobalOption>();
		mockGlobalOption.SetupGet(g => g.Emergency).Returns(emergencyOption);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOption.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new ShipStatusOnEnablePatchBody(mockRuntime.Object);

		// Reset static field body to null via reflection
		var bodyField = typeof(ShipStatusOnEnablePatchBody).GetField("body", BindingFlags.NonPublic | BindingFlags.Static);
		bodyField?.SetValue(null, null);

		// Set ServiceProvider
		var mockServiceProvider = new Mock<IServiceProvider>();
		mockServiceProvider.Setup(p => p.GetService(typeof(ShipStatusOnEnablePatchBody))).Returns(patchBody);

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		var providerField = typeof(ExtremeRolesPlugin).GetField("<Provider>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
		providerField?.SetValue(plugin, mockServiceProvider.Object);

		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		mockShipStatus.SetupGet(s => s.Type).Returns((ShipStatus.MapType)(-1));

		// Act
		ShipStatusOnEnablePatchBody.StaticPostfix(mockShipStatus.Object);

		// Assert
		mockShipStatus.VerifyGet(s => s.Type, Times.Once);
	}

	[Fact]
	public void Postfix_WhenTryGetGameContextReturnsTrue_CallsChangeTime()
	{
		// Arrange
		var emergencyOption = CreateUninitializedEmergencyTaskOption();
		var mockGlobalOption = new Mock<IShipGlobalOption>();
		mockGlobalOption.SetupGet(g => g.Emergency).Returns(emergencyOption);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOption.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new ShipStatusOnEnablePatchBody(mockRuntime.Object);
		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		mockShipStatus.SetupGet(s => s.Type).Returns((ShipStatus.MapType)(-1));

		// Act
		typeof(ShipStatusOnEnablePatchBody)
			.GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Instance)?
			.Invoke(patchBody, [mockShipStatus.Object]);

		// Assert
		mockShipStatus.VerifyGet(s => s.Type, Times.Once);
	}

	[Fact]
	public void Postfix_WhenTryGetGameContextReturnsFalse_DoesNotCallChangeTime()
	{
		// Arrange
		var emergencyOption = CreateUninitializedEmergencyTaskOption();
		var mockGlobalOption = new Mock<IShipGlobalOption>();
		mockGlobalOption.SetupGet(g => g.Emergency).Returns(emergencyOption);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var patchBody = new ShipStatusOnEnablePatchBody(mockRuntime.Object);
		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);

		// Act
		typeof(ShipStatusOnEnablePatchBody)
			.GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Instance)?
			.Invoke(patchBody, [mockShipStatus.Object]);

		// Assert
		mockShipStatus.VerifyGet(s => s.Type, Times.Never);
	}
}
