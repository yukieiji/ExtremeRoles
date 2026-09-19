using System;
using AmongUs.GameOptions;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;
using ExtremeRoles.Patches.Role;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Ability;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Patches.Role;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class RoleBehaviourIsValidTargetPatchBodyTests : IDisposable
{
	public RoleBehaviourIsValidTargetPatchBodyTests()
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

	private static void SetAbilityClass(SingleRoleBase role, IAbility? ability)
	{
		typeof(SingleRoleBase)
			.GetProperty(nameof(SingleRoleBase.AbilityClass))?
			.SetValue(role, ability);
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

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = false;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.True(shouldContinue);
		Assert.False(result);
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

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = false;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.True(shouldContinue);
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenTryGetRoleFails_ReturnsTrue()
	{
		// Arrange
		byte instancePlayerId = 1;
		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = null;
		mockRoles.Setup(r => r.TryGetRole(instancePlayerId, out role)).Returns(false);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(instancePlayerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = false;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.True(shouldContinue);
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenRoleCannotKill_ReturnsTrue()
	{
		// Arrange
		byte instancePlayerId = 1;
		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(false);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(instancePlayerId, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(instancePlayerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = false;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.True(shouldContinue);
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenTargetDead_SetsResultFalseAndReturnsFalse()
	{
		// Arrange
		byte instancePlayerId = 1;
		byte targetPlayerId = 2;

		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(true);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(instancePlayerId, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(instancePlayerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var mockTargetObject = new Mock<PlayerControl>(IntPtr.Zero);
		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(t => t.PlayerId).Returns(targetPlayerId);
		mockTarget.SetupGet(t => t.IsDead).Returns(true); // Target is dead
		mockTarget.SetupGet(t => t.Disconnected).Returns(false);
		mockTarget.SetupGet(t => t.Object).Returns(mockTargetObject.Object);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.False(shouldContinue);
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenTargetIsSelf_SetsResultFalseAndReturnsFalse()
	{
		// Arrange
		byte instancePlayerId = 1;

		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(true);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(instancePlayerId, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(instancePlayerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var mockTargetObject = new Mock<PlayerControl>(IntPtr.Zero);
		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(t => t.PlayerId).Returns(instancePlayerId); // Same player ID
		mockTarget.SetupGet(t => t.IsDead).Returns(false);
		mockTarget.SetupGet(t => t.Disconnected).Returns(false);
		mockTarget.SetupGet(t => t.Object).Returns(mockTargetObject.Object);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.False(shouldContinue);
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenTargetRoleIsNull_SetsResultFalseAndReturnsFalse()
	{
		// Arrange
		byte instancePlayerId = 1;
		byte targetPlayerId = 2;

		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(true);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(instancePlayerId, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(instancePlayerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var mockTargetObject = new Mock<PlayerControl>(IntPtr.Zero);
		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(t => t.PlayerId).Returns(targetPlayerId);
		mockTarget.SetupGet(t => t.IsDead).Returns(false);
		mockTarget.SetupGet(t => t.Disconnected).Returns(false);
		mockTarget.SetupGet(t => t.Object).Returns(mockTargetObject.Object);
		mockTarget.SetupGet(t => t.Role).Returns((RoleBehaviour)null!); // Role is null

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.False(shouldContinue);
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenTargetInVentAndVentKillDisabled_SetsResultFalseAndReturnsFalse()
	{
		// Arrange
		byte instancePlayerId = 1;
		byte targetPlayerId = 2;

		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(true);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(instancePlayerId, out role)).Returns(true);

		var mockGlobalOption = new Mock<IShipGlobalOption>();
		mockGlobalOption.SetupGet(g => g.Vent).Returns(new VentConsoleOption(false, false, false, VentAnimationMode.VanillaAnimation)); // Vent kill disabled

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOption.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(instancePlayerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var mockTargetRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);

		var mockTargetObject = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetObject.SetupGet(o => o.inVent).Returns(true); // Target in vent

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(t => t.PlayerId).Returns(targetPlayerId);
		mockTarget.SetupGet(t => t.IsDead).Returns(false);
		mockTarget.SetupGet(t => t.Disconnected).Returns(false);
		mockTarget.SetupGet(t => t.Object).Returns(mockTargetObject.Object);
		mockTarget.SetupGet(t => t.Role).Returns(mockTargetRoleBehaviour.Object);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.False(shouldContinue);
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenTargetInVentAndVentKillEnabled_TargetInMovingPlat_SetsResultFalseAndReturnsFalse()
	{
		// Arrange
		byte instancePlayerId = 1;
		byte targetPlayerId = 2;

		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(true);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(instancePlayerId, out role)).Returns(true);

		var mockGlobalOption = new Mock<IShipGlobalOption>();
		mockGlobalOption.SetupGet(g => g.Vent).Returns(new VentConsoleOption(false, false, true, VentAnimationMode.VanillaAnimation)); // Vent kill enabled

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOption.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(instancePlayerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var mockTargetRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);

		var mockTargetObject = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetObject.SetupGet(o => o.inVent).Returns(true);
		mockTargetObject.SetupGet(o => o.inMovingPlat).Returns(true); // Target in moving plat

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(t => t.PlayerId).Returns(targetPlayerId);
		mockTarget.SetupGet(t => t.IsDead).Returns(false);
		mockTarget.SetupGet(t => t.Disconnected).Returns(false);
		mockTarget.SetupGet(t => t.Object).Returns(mockTargetObject.Object);
		mockTarget.SetupGet(t => t.Role).Returns(mockTargetRoleBehaviour.Object);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.False(shouldContinue);
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenTargetRoleNotFound_SetsResultFalseAndReturnsFalse()
	{
		// Arrange
		byte instancePlayerId = 1;
		byte targetPlayerId = 2;

		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(true);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(instancePlayerId, out role)).Returns(true);

		SingleRoleBase? targetRole = null;
		mockRoles.Setup(r => r.TryGetRole(targetPlayerId, out targetRole)).Returns(false); // Target role not found

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(instancePlayerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var mockTargetRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);

		var mockTargetObject = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetObject.SetupGet(o => o.inVent).Returns(false);
		mockTargetObject.SetupGet(o => o.inMovingPlat).Returns(false);

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(t => t.PlayerId).Returns(targetPlayerId);
		mockTarget.SetupGet(t => t.IsDead).Returns(false);
		mockTarget.SetupGet(t => t.Disconnected).Returns(false);
		mockTarget.SetupGet(t => t.Object).Returns(mockTargetObject.Object);
		mockTarget.SetupGet(t => t.Role).Returns(mockTargetRoleBehaviour.Object);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.False(shouldContinue);
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenTargetSameTeam_SetsResultFalseAndReturnsFalse()
	{
		// Arrange
		byte instancePlayerId = 1;
		byte targetPlayerId = 2;

		var mockTargetRole = new Mock<SingleRoleBase>();

		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(true);
		mockRole.Setup(r => r.IsSameTeam(mockTargetRole.Object)).Returns(true); // Same team

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(instancePlayerId, out role)).Returns(true);

		SingleRoleBase? targetRole = mockTargetRole.Object;
		mockRoles.Setup(r => r.TryGetRole(targetPlayerId, out targetRole)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(instancePlayerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var mockTargetRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);

		var mockTargetObject = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetObject.SetupGet(o => o.inVent).Returns(false);
		mockTargetObject.SetupGet(o => o.inMovingPlat).Returns(false);

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(t => t.PlayerId).Returns(targetPlayerId);
		mockTarget.SetupGet(t => t.IsDead).Returns(false);
		mockTarget.SetupGet(t => t.Disconnected).Returns(false);
		mockTarget.SetupGet(t => t.Object).Returns(mockTargetObject.Object);
		mockTarget.SetupGet(t => t.Role).Returns(mockTargetRoleBehaviour.Object);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.False(shouldContinue);
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenTargetInvincibleAndKillInvalid_SetsResultFalseAndReturnsFalse()
	{
		// Arrange
		byte instancePlayerId = 1;
		byte targetPlayerId = 2;

		var mockInvincibleAbility = new Mock<IAbility>();
		var mockInvincible = mockInvincibleAbility.As<IInvincible>();
		mockInvincible.Setup(i => i.IsValidKillFromSource(instancePlayerId)).Returns(false); // Invalid kill

		var mockTargetRole = new Mock<SingleRoleBase>();
		SetAbilityClass(mockTargetRole.Object, mockInvincibleAbility.Object);

		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(true);
		mockRole.Setup(r => r.IsSameTeam(mockTargetRole.Object)).Returns(false);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(instancePlayerId, out role)).Returns(true);

		SingleRoleBase? targetRole = mockTargetRole.Object;
		mockRoles.Setup(r => r.TryGetRole(targetPlayerId, out targetRole)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(instancePlayerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var mockTargetRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);

		var mockTargetObject = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetObject.SetupGet(o => o.inVent).Returns(false);
		mockTargetObject.SetupGet(o => o.inMovingPlat).Returns(false);

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(t => t.PlayerId).Returns(targetPlayerId);
		mockTarget.SetupGet(t => t.IsDead).Returns(false);
		mockTarget.SetupGet(t => t.Disconnected).Returns(false);
		mockTarget.SetupGet(t => t.Object).Returns(mockTargetObject.Object);
		mockTarget.SetupGet(t => t.Role).Returns(mockTargetRoleBehaviour.Object);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.False(shouldContinue);
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenTargetInvincibleAndKillValid_SetsResultTrueAndReturnsFalse()
	{
		// Arrange
		byte instancePlayerId = 1;
		byte targetPlayerId = 2;

		var mockInvincibleAbility = new Mock<IAbility>();
		var mockInvincible = mockInvincibleAbility.As<IInvincible>();
		mockInvincible.Setup(i => i.IsValidKillFromSource(instancePlayerId)).Returns(true); // Valid kill

		var mockTargetRole = new Mock<SingleRoleBase>();
		SetAbilityClass(mockTargetRole.Object, mockInvincibleAbility.Object);

		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(true);
		mockRole.Setup(r => r.IsSameTeam(mockTargetRole.Object)).Returns(false);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(instancePlayerId, out role)).Returns(true);

		SingleRoleBase? targetRole = mockTargetRole.Object;
		mockRoles.Setup(r => r.TryGetRole(targetPlayerId, out targetRole)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(instancePlayerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var mockTargetRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);

		var mockTargetObject = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetObject.SetupGet(o => o.inVent).Returns(false);
		mockTargetObject.SetupGet(o => o.inMovingPlat).Returns(false);

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(t => t.PlayerId).Returns(targetPlayerId);
		mockTarget.SetupGet(t => t.IsDead).Returns(false);
		mockTarget.SetupGet(t => t.Disconnected).Returns(false);
		mockTarget.SetupGet(t => t.Object).Returns(mockTargetObject.Object);
		mockTarget.SetupGet(t => t.Role).Returns(mockTargetRoleBehaviour.Object);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = false;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.False(shouldContinue);
		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenTargetNotInvincible_SetsResultTrueAndReturnsFalse()
	{
		// Arrange
		byte instancePlayerId = 1;
		byte targetPlayerId = 2;

		var mockTargetRole = new Mock<SingleRoleBase>();
		SetAbilityClass(mockTargetRole.Object, null); // Not invincible

		var mockRole = new Mock<SingleRoleBase>();
		mockRole.SetupGet(r => r.CanKill).Returns(true);
		mockRole.Setup(r => r.IsSameTeam(mockTargetRole.Object)).Returns(false);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = mockRole.Object;
		mockRoles.Setup(r => r.TryGetRole(instancePlayerId, out role)).Returns(true);

		SingleRoleBase? targetRole = mockTargetRole.Object;
		mockRoles.Setup(r => r.TryGetRole(targetPlayerId, out targetRole)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(instancePlayerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);

		var mockTargetRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);

		var mockTargetObject = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetObject.SetupGet(o => o.inVent).Returns(false);
		mockTargetObject.SetupGet(o => o.inMovingPlat).Returns(false);

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(t => t.PlayerId).Returns(targetPlayerId);
		mockTarget.SetupGet(t => t.IsDead).Returns(false);
		mockTarget.SetupGet(t => t.Disconnected).Returns(false);
		mockTarget.SetupGet(t => t.Object).Returns(mockTargetObject.Object);
		mockTarget.SetupGet(t => t.Role).Returns(mockTargetRoleBehaviour.Object);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = false;

		// Act
		bool shouldContinue = patchBody.Prefix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.False(shouldContinue);
		Assert.True(result);
	}

	[Fact]
	public void Postfix_WhenTryGetGameContextFails_DoesNotModifyResult()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(false);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		patchBody.Postfix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Postfix_WhenIsNotGameNow_DoesNotModifyResult()
	{
		// Arrange
		var mockContext = new Mock<IGameContext>();
		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(false);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		patchBody.Postfix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Postfix_WhenResultIsAlreadyFalse_DoesNotModifyResult()
	{
		// Arrange
		var mockContext = new Mock<IGameContext>();
		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = false; // Already false

		// Act
		patchBody.Postfix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Postfix_WhenInstanceRoleIsNotDetectiveOrTracker_DoesNotModifyResult()
	{
		// Arrange
		var mockContext = new Mock<IGameContext>();
		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Role).Returns(RoleTypes.Crewmate); // Neither Detective nor Tracker

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		patchBody.Postfix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Postfix_WhenTargetRoleNotFound_DoesNotModifyResult()
	{
		// Arrange
		byte targetPlayerId = 2;

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? targetRole = null;
		mockRoles.Setup(r => r.TryGetRole(targetPlayerId, out targetRole)).Returns(false); // Not found

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Role).Returns(RoleTypes.Detective);

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(t => t.PlayerId).Returns(targetPlayerId);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		patchBody.Postfix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Postfix_WhenTargetRoleAbilityClassIsNotInvincible_DoesNotModifyResult()
	{
		// Arrange
		byte targetPlayerId = 2;

		var mockTargetRole = new Mock<SingleRoleBase>();
		SetAbilityClass(mockTargetRole.Object, null); // Not invincible

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? targetRole = mockTargetRole.Object;
		mockRoles.Setup(r => r.TryGetRole(targetPlayerId, out targetRole)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Role).Returns(RoleTypes.Detective);

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(t => t.PlayerId).Returns(targetPlayerId);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		patchBody.Postfix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(RoleTypes.Detective, true, true)]
	[InlineData(RoleTypes.Detective, false, false)]
	[InlineData(RoleTypes.Tracker, true, true)]
	[InlineData(RoleTypes.Tracker, false, false)]
	public void Postfix_WhenDetectiveOrTrackerAndTargetInvincible_UpdatesResult(RoleTypes instanceRole, bool isValidAbilitySource, bool expectedResult)
	{
		// Arrange
		byte instancePlayerId = 1;
		byte targetPlayerId = 2;

		var mockInvincibleAbility = new Mock<IAbility>();
		var mockInvincible = mockInvincibleAbility.As<IInvincible>();
		mockInvincible.Setup(i => i.IsValidAbilitySource(instancePlayerId)).Returns(isValidAbilitySource);

		var mockTargetRole = new Mock<SingleRoleBase>();
		SetAbilityClass(mockTargetRole.Object, mockInvincibleAbility.Object);

		var mockRoles = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? targetRole = mockTargetRole.Object;
		mockRoles.Setup(r => r.TryGetRole(targetPlayerId, out targetRole)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(instancePlayerId);

		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Player).Returns(mockPlayer.Object);
		mockRoleBehaviour.SetupGet(r => r.Role).Returns(instanceRole);

		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(t => t.PlayerId).Returns(targetPlayerId);

		var patchBody = new RoleBehaviourIsValidTargetPatchBody(mockProgress.Object, mockRuntime.Object);
		bool result = true;

		// Act
		patchBody.Postfix(mockRoleBehaviour.Object, ref result, mockTarget.Object);

		// Assert
		Assert.Equal(expectedResult, result);
	}
}
