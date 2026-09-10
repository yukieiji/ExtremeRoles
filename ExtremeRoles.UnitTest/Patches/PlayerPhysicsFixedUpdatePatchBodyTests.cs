using System;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Patches;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Roles.Solo.Crewmate;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Patches;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class PlayerPhysicsFixedUpdatePatchBodyTests : IDisposable
{
	public PlayerPhysicsFixedUpdatePatchBodyTests()
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
		MockSetupHelper.SetupTimeHelpers();
		MockSetupHelper.SetupPlayerControlMocks();
	}

	private static Mock<PlayerPhysics> CreateMockPhysics(out Mock<Rigidbody2D> mockBody, out Mock<PlayerControl> mockPlayer)
	{
		mockBody = new Mock<Rigidbody2D>(IntPtr.Zero);
		mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		var mockPhysics = new Mock<PlayerPhysics>(IntPtr.Zero);
		mockPhysics.SetupGet(p => p.body).Returns(mockBody.Object);
		mockPhysics.SetupGet(p => p.myPlayer).Returns(mockPlayer.Object);

		return mockPhysics;
	}

	[Fact]
	public void Prefix_WhenIsNotTaskPhase_ReturnsTrue()
	{
		// Arrange
		var mockSystem = new Mock<IExtremeSystemTypeManager>();
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsTaskPhase).Returns(false);

		var patchBody = new PlayerPhysicsFixedUpdatePatchBody(mockSystem.Object, mockProgress.Object, mockRuntime.Object);
		var mockPhysics = CreateMockPhysics(out _, out _);

		// Act
		bool result = patchBody.Prefix(mockPhysics.Object);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenTryGetGameContextReturnsFalse_ReturnsTrue()
	{
		// Arrange
		var mockSystem = new Mock<IExtremeSystemTypeManager>();
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsTaskPhase).Returns(true);
		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var patchBody = new PlayerPhysicsFixedUpdatePatchBody(mockSystem.Object, mockProgress.Object, mockRuntime.Object);
		var mockPhysics = CreateMockPhysics(out _, out _);

		// Act
		bool result = patchBody.Prefix(mockPhysics.Object);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenIsTimeBreakNow_SetsVelocityZeroAndReturnsFalse()
	{
		// Arrange
		var mockSystem = new Mock<IExtremeSystemTypeManager>();
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsTaskPhase).Returns(true);

		var mockContext = new Mock<IGameContext>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var timeBreakSystem = new TimeBreakerTimeBreakSystem(5.0f, false, false, false);
		typeof(TimeBreakerTimeBreakSystem)
			.GetProperty(nameof(TimeBreakerTimeBreakSystem.Active))?
			.GetSetMethod(true)?
			.Invoke(timeBreakSystem, [true]);

		TimeBreakerTimeBreakSystem? systemOut = timeBreakSystem;
		mockSystem.Setup(s => s.TryGet<TimeBreakerTimeBreakSystem>(ExtremeSystemType.TimeBreakerTimeBreakSystem, out systemOut))
			.Returns(true);

		var patchBody = new PlayerPhysicsFixedUpdatePatchBody(mockSystem.Object, mockProgress.Object, mockRuntime.Object);
		var mockPhysics = CreateMockPhysics(out var mockBody, out _);

		// Act
		bool result = patchBody.Prefix(mockPhysics.Object);

		// Assert
		Assert.False(result);
		mockBody.VerifySet(b => b.velocity = Vector2.zero, Times.Once);
	}

	[Fact]
	public void Prefix_WhenAmOwnerAndCannotMove_SetsVelocityZeroAndReturnsFalse()
	{
		// Arrange
		var mockSystem = new Mock<IExtremeSystemTypeManager>();
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsTaskPhase).Returns(true);

		TimeBreakerTimeBreakSystem? systemOut = null;
		mockSystem.Setup(s => s.TryGet<TimeBreakerTimeBreakSystem>(ExtremeSystemType.TimeBreakerTimeBreakSystem, out systemOut))
			.Returns(false);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();

		// Setup GetLocalRoleCastedStatusFlag to return false for IStatusMovable.CanMove
		mockRoleContainer.Setup(r => r.GetLocalRoleCastedStatusFlag<IStatusMovable>(It.IsAny<Func<IStatusMovable, bool>>()))
			.Returns(false);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerPhysicsFixedUpdatePatchBody(mockSystem.Object, mockProgress.Object, mockRuntime.Object);
		var mockPhysics = CreateMockPhysics(out var mockBody, out _);
		mockPhysics.SetupGet(p => p.AmOwner).Returns(true);

		// Act
		bool result = patchBody.Prefix(mockPhysics.Object);

		// Assert
		Assert.False(result);
		mockBody.VerifySet(b => b.velocity = Vector2.zero, Times.Once);
	}

	[Fact]
	public void Prefix_WhenNotAmOwner_SetsVelocityZeroAndReturnsTrue()
	{
		// Arrange
		var mockSystem = new Mock<IExtremeSystemTypeManager>();
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsTaskPhase).Returns(true);

		TimeBreakerTimeBreakSystem? systemOut = null;
		mockSystem.Setup(s => s.TryGet<TimeBreakerTimeBreakSystem>(ExtremeSystemType.TimeBreakerTimeBreakSystem, out systemOut))
			.Returns(false);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerPhysicsFixedUpdatePatchBody(mockSystem.Object, mockProgress.Object, mockRuntime.Object);
		var mockPhysics = CreateMockPhysics(out var mockBody, out _);
		mockPhysics.SetupGet(p => p.AmOwner).Returns(false);

		// Act
		bool result = patchBody.Prefix(mockPhysics.Object);

		// Assert
		Assert.True(result);
		mockBody.VerifySet(b => b.velocity = Vector2.zero, Times.Once);
	}

	[Fact]
	public void Prefix_WhenAmOwnerAndCanMove_SetsVelocityZeroAndReturnsTrue()
	{
		// Arrange
		var mockSystem = new Mock<IExtremeSystemTypeManager>();
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsTaskPhase).Returns(true);

		TimeBreakerTimeBreakSystem? systemOut = null;
		mockSystem.Setup(s => s.TryGet<TimeBreakerTimeBreakSystem>(ExtremeSystemType.TimeBreakerTimeBreakSystem, out systemOut))
			.Returns(false);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalRoleCastedStatusFlag<IStatusMovable>(It.IsAny<Func<IStatusMovable, bool>>()))
			.Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerPhysicsFixedUpdatePatchBody(mockSystem.Object, mockProgress.Object, mockRuntime.Object);
		var mockPhysics = CreateMockPhysics(out var mockBody, out _);
		mockPhysics.SetupGet(p => p.AmOwner).Returns(true);

		// Act
		bool result = patchBody.Prefix(mockPhysics.Object);

		// Assert
		Assert.True(result);
		mockBody.VerifySet(b => b.velocity = Vector2.zero, Times.Once);
	}

	[Fact]
	public void Postfix_WhenIsNotTaskPhase_ReturnsEarlyWithoutModifyingVelocity()
	{
		// Arrange
		var mockSystem = new Mock<IExtremeSystemTypeManager>();
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsTaskPhase).Returns(false);

		var patchBody = new PlayerPhysicsFixedUpdatePatchBody(mockSystem.Object, mockProgress.Object, mockRuntime.Object);
		var mockPhysics = CreateMockPhysics(out var mockBody, out _);

		// Act
		patchBody.Postfix(mockPhysics.Object);

		// Assert
		mockBody.VerifySet(b => b.velocity = It.IsAny<Vector2>(), Times.Never);
	}

	[Fact]
	public void Postfix_WhenNotAmOwner_ReturnsEarlyWithoutModifyingVelocity()
	{
		// Arrange
		var mockSystem = new Mock<IExtremeSystemTypeManager>();
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsTaskPhase).Returns(true);

		var patchBody = new PlayerPhysicsFixedUpdatePatchBody(mockSystem.Object, mockProgress.Object, mockRuntime.Object);
		var mockPhysics = CreateMockPhysics(out var mockBody, out _);
		mockPhysics.SetupGet(p => p.AmOwner).Returns(false);

		// Act
		patchBody.Postfix(mockPhysics.Object);

		// Assert
		mockBody.VerifySet(b => b.velocity = It.IsAny<Vector2>(), Times.Never);
	}

	[Fact]
	public void Postfix_WhenMyPlayerCannotMove_ReturnsEarlyWithoutModifyingVelocity()
	{
		// Arrange
		var mockSystem = new Mock<IExtremeSystemTypeManager>();
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsTaskPhase).Returns(true);

		var patchBody = new PlayerPhysicsFixedUpdatePatchBody(mockSystem.Object, mockProgress.Object, mockRuntime.Object);
		var mockPhysics = CreateMockPhysics(out var mockBody, out var mockPlayer);
		mockPhysics.SetupGet(p => p.AmOwner).Returns(true);
		mockPlayer.SetupGet(p => p.CanMove).Returns(false);

		// Act
		patchBody.Postfix(mockPhysics.Object);

		// Assert
		mockBody.VerifySet(b => b.velocity = It.IsAny<Vector2>(), Times.Never);
	}

	[Fact]
	public void Postfix_WhenGameDataInstanceIsNull_ReturnsEarlyWithoutModifyingVelocity()
	{
		// Arrange
		var mockSystem = new Mock<IExtremeSystemTypeManager>();
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsTaskPhase).Returns(true);

		var mockGameDataHelper = new Mock<IMockGameDataget_Instance>();
		mockGameDataHelper.Setup(h => h.Invoke()).Returns((GameData)null!);
		IMockGameDataget_Instance.Instance = mockGameDataHelper.Object;

		var patchBody = new PlayerPhysicsFixedUpdatePatchBody(mockSystem.Object, mockProgress.Object, mockRuntime.Object);
		var mockPhysics = CreateMockPhysics(out var mockBody, out var mockPlayer);
		mockPhysics.SetupGet(p => p.AmOwner).Returns(true);
		mockPlayer.SetupGet(p => p.CanMove).Returns(true);

		// Act
		patchBody.Postfix(mockPhysics.Object);

		// Assert
		mockBody.VerifySet(b => b.velocity = It.IsAny<Vector2>(), Times.Never);
	}

	[Fact]
	public void Postfix_WhenTryGetGameContextReturnsFalse_ReturnsEarlyWithoutModifyingVelocity()
	{
		// Arrange
		var mockSystem = new Mock<IExtremeSystemTypeManager>();
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsTaskPhase).Returns(true);

		var mockGameData = new Mock<GameData>(IntPtr.Zero);
		var mockGameDataHelper = new Mock<IMockGameDataget_Instance>();
		mockGameDataHelper.Setup(h => h.Invoke()).Returns(mockGameData.Object);
		IMockGameDataget_Instance.Instance = mockGameDataHelper.Object;

		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var patchBody = new PlayerPhysicsFixedUpdatePatchBody(mockSystem.Object, mockProgress.Object, mockRuntime.Object);
		var mockPhysics = CreateMockPhysics(out var mockBody, out var mockPlayer);
		mockPhysics.SetupGet(p => p.AmOwner).Returns(true);
		mockPlayer.SetupGet(p => p.CanMove).Returns(true);

		// Act
		patchBody.Postfix(mockPhysics.Object);

		// Assert
		mockBody.VerifySet(b => b.velocity = It.IsAny<Vector2>(), Times.Never);
	}

	[Fact]
	public void Postfix_WhenAllConditionsMet_MultipliesVelocityByRoleSpeed()
	{
		// Arrange
		var mockSystem = new Mock<IExtremeSystemTypeManager>();
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsTaskPhase).Returns(true);

		var mockGameData = new Mock<GameData>(IntPtr.Zero);
		var mockGameDataHelper = new Mock<IMockGameDataget_Instance>();
		mockGameDataHelper.Setup(h => h.Invoke()).Returns(mockGameData.Object);
		IMockGameDataget_Instance.Instance = mockGameDataHelper.Object;

		var role = new SpecialCrew();
		role.IsBoost = true;
		role.MoveSpeed = 1.5f;

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(role);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerPhysicsFixedUpdatePatchBody(mockSystem.Object, mockProgress.Object, mockRuntime.Object);
		var mockPhysics = CreateMockPhysics(out var mockBody, out var mockPlayer);
		mockPhysics.SetupGet(p => p.AmOwner).Returns(true);
		mockPlayer.SetupGet(p => p.CanMove).Returns(true);

		var initialVelocity = new Vector2(2.0f, 4.0f);
		mockBody.SetupGet(b => b.velocity).Returns(initialVelocity);

		// Act
		patchBody.Postfix(mockPhysics.Object);

		// Assert
		var expectedVelocity = initialVelocity * 1.5f;
		mockBody.VerifySet(b => b.velocity = It.Is<Vector2>(v => v.x == expectedVelocity.x && v.y == expectedVelocity.y), Times.Once);
	}
}
