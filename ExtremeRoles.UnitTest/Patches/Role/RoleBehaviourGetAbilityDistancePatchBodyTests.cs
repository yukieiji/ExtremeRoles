using System;
using AmongUs.GameOptions;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Patches.Role;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Patches.Role;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class RoleBehaviourGetAbilityDistancePatchBodyTests : IDisposable
{
	private static readonly Mock<MockGameManagerget_InstanceHelper> globalGameManagerHelper = new();

	public RoleBehaviourGetAbilityDistancePatchBodyTests()
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
	public void Prefix_WhenTryGetGameContextFails_ReturnsTrue()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(false);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(1);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var patchBody = new RoleBehaviourGetAbilityDistancePatchBody(mockProgress.Object, mockRuntime.Object);
		float resultDistance = 0f;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref resultDistance);

		// Assert
		Assert.True(shouldContinue);
		Assert.Equal(0f, resultDistance);
	}

	[Fact]
	public void Prefix_WhenIsNotGameNow_ReturnsTrue()
	{
		// Arrange
		var mockContext = new Mock<IGameContext>();
		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(false);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(1);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var patchBody = new RoleBehaviourGetAbilityDistancePatchBody(mockProgress.Object, mockRuntime.Object);
		float resultDistance = 0f;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref resultDistance);

		// Assert
		Assert.True(shouldContinue);
		Assert.Equal(0f, resultDistance);
	}

	[Fact]
	public void Prefix_WhenTryGetRoleFails_ReturnsTrue()
	{
		// Arrange
		byte playerId = 1;
		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = null;
		mockRoles.Setup(r => r.TryGetRole(playerId, out role)).Returns(false);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var patchBody = new RoleBehaviourGetAbilityDistancePatchBody(mockProgress.Object, mockRuntime.Object);
		float resultDistance = 0f;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref resultDistance);

		// Assert
		Assert.True(shouldContinue);
		Assert.Equal(0f, resultDistance);
	}

	[Fact]
	public void Prefix_WhenRoleCannotKill_ReturnsTrue()
	{
		// Arrange
		byte playerId = 1;
		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(false);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(playerId, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var patchBody = new RoleBehaviourGetAbilityDistancePatchBody(mockProgress.Object, mockRuntime.Object);
		float resultDistance = 0f;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref resultDistance);

		// Assert
		Assert.True(shouldContinue);
		Assert.Equal(0f, resultDistance);
	}

	[Fact]
	public void Prefix_WhenHasOtherKillRangeIsFalse_ReturnsTrue()
	{
		// Arrange
		byte playerId = 1;
		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(true);
		mockRole.Object.HasOtherKillRange = false;

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(playerId, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var patchBody = new RoleBehaviourGetAbilityDistancePatchBody(mockProgress.Object, mockRuntime.Object);
		float resultDistance = 0f;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref resultDistance);

		// Assert
		Assert.True(shouldContinue);
		Assert.Equal(0f, resultDistance);
	}

	[Fact]
	public void Prefix_WhenTryGetKillDistanceFails_ReturnsTrue()
	{
		// Arrange
		byte playerId = 1;
		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(true);
		mockRole.Object.HasOtherKillRange = true;
		mockRole.Object.KillRange = 0;

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(playerId, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		// GameSystem.TryGetKillDistance relies on GameManager.Instance
		globalGameManagerHelper.Setup(h => h.Invoke()).Returns((GameManager)null!);
		MockGameManagerget_InstanceHelper.Instance = globalGameManagerHelper.Object;

		var patchBody = new RoleBehaviourGetAbilityDistancePatchBody(mockProgress.Object, mockRuntime.Object);
		float resultDistance = 0f;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref resultDistance);

		// Assert
		Assert.True(shouldContinue);
		Assert.Equal(0f, resultDistance);
	}

	[Fact]
	public void Prefix_WhenTryGetKillDistanceSucceeds_ReturnsTrueWhenKillRangeNotGreaterThanRange()
	{
		// Arrange
		byte playerId = 1;
		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(true);
		mockRole.Object.HasOtherKillRange = true;
		mockRole.Object.KillRange = 0;

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(playerId, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var mockArray = new Mock<Il2CppStructArray<float>>(IntPtr.Zero);
		var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
		mockGameOptions.Setup(g => g.GetFloatArray(FloatArrayOptionNames.KillDistances)).Returns(mockArray.Object);

		var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
		mockLogicOptions.SetupGet(l => l.currentGameOptions).Returns(mockGameOptions.Object);

		var mockGameManager = new Mock<GameManager>();
		mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);
		globalGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		MockGameManagerget_InstanceHelper.Instance = globalGameManagerHelper.Object;

		var patchBody = new RoleBehaviourGetAbilityDistancePatchBody(mockProgress.Object, mockRuntime.Object);
		float resultDistance = 0f;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref resultDistance);

		// Assert
		Assert.True(shouldContinue);
		Assert.Equal(0f, resultDistance);
	}
}
