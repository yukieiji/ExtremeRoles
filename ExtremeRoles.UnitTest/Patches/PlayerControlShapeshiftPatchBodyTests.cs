using System;

using AmongUs.GameOptions;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Patches.Player;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo;
using Moq;
using UnityEngine;
using Xunit;

using Il2CppDictionary = Il2CppSystem.Collections.Generic.Dictionary<PlayerOutfitType, NetworkedPlayerInfo.PlayerOutfit>;

namespace ExtremeRoles.UnitTest.Patches;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class PlayerControlShapeshiftPatchBodyTests : IDisposable
{
	private sealed class DummySingleRole : SingleRoleBase
	{
		public DummySingleRole(RoleCore core)
		{
			var coreField = typeof(SingleRoleBase).GetField("<Core>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			coreField?.SetValue(this, core);
		}

		protected override void CreateSpecificOption(ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory factory) { }
		protected override void RoleSpecificInit() { }

		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
		public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => Color.white;
	}

	public PlayerControlShapeshiftPatchBodyTests()
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
		MockSetupHelper.SetupObjectImplicitHelpers();

		if (Il2CppSystem.MockActionop_ImplicitHelper.Instance == null)
		{
			var mockActionImplicit = new Mock<Il2CppSystem.MockActionop_ImplicitHelper>();
			mockActionImplicit.Setup(x => x.Invoke(It.IsAny<Action>()))
				.Returns((Action act) => act != null ? new Il2CppSystem.Action(IntPtr.Zero) : null!);
			Il2CppSystem.MockActionop_ImplicitHelper.Instance = mockActionImplicit.Object;
		}

		if (MockAprilFoolsModeShouldLongAroundHelper.Instance == null)
		{
			var mockLongAround = new Mock<MockAprilFoolsModeShouldLongAroundHelper>();
			mockLongAround.Setup(x => x.Invoke()).Returns(false);
			MockAprilFoolsModeShouldLongAroundHelper.Instance = mockLongAround.Object;
		}
	}

	private static (Mock<PlayerControl> mockPlayer, Mock<NetworkedPlayerInfo> mockData) CreateMockPlayer(byte playerId)
	{
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		mockData.SetupGet(d => d.PlayerId).Returns(playerId);

		var mockLogger = new Mock<Logger>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.logger).Returns(mockLogger.Object);

		return (mockPlayer, mockData);
	}

	[Fact]
	public void Prefix_WhenIsNotGameNow_ReturnsTrue()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(false);

		var patchBody = new PlayerControlShapeshiftPatchBody(mockProgress.Object, mockRuntime.Object);
		var (mockPlayer, _) = CreateMockPlayer(0);
		var (targetPlayer, _) = CreateMockPlayer(1);

		// Act
		bool result = patchBody.Prefix(mockPlayer.Object, targetPlayer.Object, animate: false);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenTryGetGameContextReturnsFalse_ReturnsTrue()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);
		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var patchBody = new PlayerControlShapeshiftPatchBody(mockProgress.Object, mockRuntime.Object);
		var (mockPlayer, _) = CreateMockPlayer(0);
		var (targetPlayer, _) = CreateMockPlayer(1);

		// Act
		bool result = patchBody.Prefix(mockPlayer.Object, targetPlayer.Object, animate: false);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenTryGetRoleReturnsFalse_ReturnsTrue()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = null;
		mockRoleContainer.Setup(r => r.TryGetRole(It.IsAny<byte>(), out role)).Returns(false);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerControlShapeshiftPatchBody(mockProgress.Object, mockRuntime.Object);
		var (mockPlayer, _) = CreateMockPlayer(0);
		var (targetPlayer, _) = CreateMockPlayer(1);

		// Act
		bool result = patchBody.Prefix(mockPlayer.Object, targetPlayer.Object, animate: false);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenRoleIsVanillaShapeshifter_ReturnsTrue()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var shapeshifterRole = new VanillaRoleWrapper(RoleTypes.Shapeshifter);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = shapeshifterRole;
		mockRoleContainer.Setup(r => r.TryGetRole(0, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerControlShapeshiftPatchBody(mockProgress.Object, mockRuntime.Object);
		var (mockPlayer, _) = CreateMockPlayer(0);
		var (targetPlayer, _) = CreateMockPlayer(1);

		// Act
		bool result = patchBody.Prefix(mockPlayer.Object, targetPlayer.Object, animate: false);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenCurrentOutfitTypeIsMushroomMixup_ReturnsFalseWithoutChangingOutfit()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var customRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = customRole;
		mockRoleContainer.Setup(r => r.TryGetRole(0, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerControlShapeshiftPatchBody(mockProgress.Object, mockRuntime.Object);
		var (mockPlayer, _) = CreateMockPlayer(0);
		mockPlayer.SetupGet(p => p.CurrentOutfitType).Returns(PlayerOutfitType.MushroomMixup);

		var (targetPlayer, _) = CreateMockPlayer(1);

		// Act
		bool result = patchBody.Prefix(mockPlayer.Object, targetPlayer.Object, animate: false);

		// Assert
		Assert.False(result);
		mockPlayer.Verify(p => p.RawSetOutfit(It.IsAny<NetworkedPlayerInfo.PlayerOutfit>(), It.IsAny<PlayerOutfitType>()), Times.Never);
	}

	[Fact]
	public void Prefix_WhenInstanceDefaultOutfitNotFound_ReturnsFalseWithoutChangingOutfit()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var customRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = customRole;
		mockRoleContainer.Setup(r => r.TryGetRole(0, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerControlShapeshiftPatchBody(mockProgress.Object, mockRuntime.Object);
		var (mockPlayer, mockData) = CreateMockPlayer(0);
		mockPlayer.SetupGet(p => p.CurrentOutfitType).Returns(PlayerOutfitType.Default);

		var emptyOutfits = new Mock<Il2CppDictionary>(IntPtr.Zero);
		NetworkedPlayerInfo.PlayerOutfit? defaultOutfit = null;
		emptyOutfits.Setup(d => d.TryGetValue(PlayerOutfitType.Default, out defaultOutfit)).Returns(false);
		mockData.SetupGet(d => d.Outfits).Returns(emptyOutfits.Object);

		var (targetPlayer, _) = CreateMockPlayer(1);

		// Act
		bool result = patchBody.Prefix(mockPlayer.Object, targetPlayer.Object, animate: false);

		// Assert
		Assert.False(result);
		mockPlayer.Verify(p => p.RawSetOutfit(It.IsAny<NetworkedPlayerInfo.PlayerOutfit>(), It.IsAny<PlayerOutfitType>()), Times.Never);
	}

	[Fact]
	public void Prefix_WhenDifferentTargetDefaultOutfitNotFound_ReturnsFalseWithoutChangingOutfit()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var customRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = customRole;
		mockRoleContainer.Setup(r => r.TryGetRole(0, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerControlShapeshiftPatchBody(mockProgress.Object, mockRuntime.Object);
		var (mockPlayer, mockData) = CreateMockPlayer(0);
		mockPlayer.SetupGet(p => p.CurrentOutfitType).Returns(PlayerOutfitType.Default);

		var instanceOutfit = new Mock<NetworkedPlayerInfo.PlayerOutfit>(IntPtr.Zero).Object;
		var instanceOutfits = new Mock<Il2CppDictionary>(IntPtr.Zero);
		NetworkedPlayerInfo.PlayerOutfit? instOutfitOut = instanceOutfit;
		instanceOutfits.Setup(d => d.TryGetValue(PlayerOutfitType.Default, out instOutfitOut)).Returns(true);
		mockData.SetupGet(d => d.Outfits).Returns(instanceOutfits.Object);

		var (targetPlayer, targetData) = CreateMockPlayer(1);
		var targetOutfits = new Mock<Il2CppDictionary>(IntPtr.Zero);
		NetworkedPlayerInfo.PlayerOutfit? targetOutfitOut = null;
		targetOutfits.Setup(d => d.TryGetValue(PlayerOutfitType.Default, out targetOutfitOut)).Returns(false);
		targetData.SetupGet(d => d.Outfits).Returns(targetOutfits.Object);

		// Act
		bool result = patchBody.Prefix(mockPlayer.Object, targetPlayer.Object, animate: false);

		// Assert
		Assert.False(result);
		mockPlayer.Verify(p => p.RawSetOutfit(It.IsAny<NetworkedPlayerInfo.PlayerOutfit>(), It.IsAny<PlayerOutfitType>()), Times.Never);
	}

	[Fact]
	public void Prefix_WhenAnimateFalseAndIsSame_RevertsShapeshiftAndReturnsFalse()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var customRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = customRole;
		mockRoleContainer.Setup(r => r.TryGetRole(0, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerControlShapeshiftPatchBody(mockProgress.Object, mockRuntime.Object);
		var (mockPlayer, mockData) = CreateMockPlayer(0);
		mockPlayer.SetupGet(p => p.CurrentOutfitType).Returns(PlayerOutfitType.Default);

		var instanceOutfit = new Mock<NetworkedPlayerInfo.PlayerOutfit>(IntPtr.Zero).Object;
		var instanceOutfits = new Mock<Il2CppDictionary>(IntPtr.Zero);
		NetworkedPlayerInfo.PlayerOutfit? instOutfitOut = instanceOutfit;
		instanceOutfits.Setup(d => d.TryGetValue(PlayerOutfitType.Default, out instOutfitOut)).Returns(true);
		mockData.SetupGet(d => d.Outfits).Returns(instanceOutfits.Object);

		// Target is same player
		var (targetPlayer, _) = CreateMockPlayer(0);

		// Act
		bool result = patchBody.Prefix(mockPlayer.Object, targetPlayer.Object, animate: false);

		// Assert
		Assert.False(result);
		mockPlayer.Verify(p => p.RawSetOutfit(instanceOutfit, PlayerOutfitType.Default), Times.Once);
		mockPlayer.VerifySet(p => p.shapeshiftTargetPlayerId = -1, Times.Once);
	}

	[Fact]
	public void Prefix_WhenAnimateFalseAndNotIsSame_ShapeshiftsToTargetAndReturnsFalse()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var customRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = customRole;
		mockRoleContainer.Setup(r => r.TryGetRole(0, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerControlShapeshiftPatchBody(mockProgress.Object, mockRuntime.Object);
		var (mockPlayer, mockData) = CreateMockPlayer(0);
		mockPlayer.SetupGet(p => p.CurrentOutfitType).Returns(PlayerOutfitType.Default);

		var instanceOutfit = new Mock<NetworkedPlayerInfo.PlayerOutfit>(IntPtr.Zero).Object;
		var instanceOutfits = new Mock<Il2CppDictionary>(IntPtr.Zero);
		NetworkedPlayerInfo.PlayerOutfit? instOutfitOut = instanceOutfit;
		instanceOutfits.Setup(d => d.TryGetValue(PlayerOutfitType.Default, out instOutfitOut)).Returns(true);
		mockData.SetupGet(d => d.Outfits).Returns(instanceOutfits.Object);

		var (targetPlayer, targetData) = CreateMockPlayer(1);
		var targetOutfit = new Mock<NetworkedPlayerInfo.PlayerOutfit>(IntPtr.Zero).Object;
		var targetOutfits = new Mock<Il2CppDictionary>(IntPtr.Zero);
		NetworkedPlayerInfo.PlayerOutfit? targetOutfitOut = targetOutfit;
		targetOutfits.Setup(d => d.TryGetValue(PlayerOutfitType.Default, out targetOutfitOut)).Returns(true);
		targetData.SetupGet(d => d.Outfits).Returns(targetOutfits.Object);

		// Act
		bool result = patchBody.Prefix(mockPlayer.Object, targetPlayer.Object, animate: false);

		// Assert
		Assert.False(result);
		mockPlayer.Verify(p => p.RawSetOutfit(targetOutfit, PlayerOutfitType.Shapeshifted), Times.Once);
		mockPlayer.VerifySet(p => p.shapeshiftTargetPlayerId = 1, Times.Once);
	}

	[Fact]
	public void Prefix_WhenAnimateTrue_PlaysShapeshiftAnimationAndReturnsFalse()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var customRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		SingleRoleBase? role = customRole;
		mockRoleContainer.Setup(r => r.TryGetRole(0, out role)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var localPlayer = MockSetupHelper.SetupPlayerControlMocks();

		var mockRoleManager = MockSetupHelper.SetupDestroyableSingletonMock<RoleManager>();
		var mockAnimPrefab = new Mock<RoleEffectAnimation>(IntPtr.Zero);
		mockRoleManager.SetupGet(rm => rm.shapeshiftAnim).Returns(mockAnimPrefab.Object);

		var mockInstantiatedAnim = new Mock<RoleEffectAnimation>(IntPtr.Zero);

		var m5 = new Mock<MockObjectInstantiateHelper5>();
		m5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object orig, Transform parent) => mockInstantiatedAnim.Object);
		MockObjectInstantiateHelper5.Instance = m5.Object;

		var m7 = new Mock<MockObjectInstantiateHelper7>();
		m7.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>()))
			.Returns((UnityEngine.Object orig) => mockInstantiatedAnim.Object);
		MockObjectInstantiateHelper7.Instance = m7.Object;

		var m10 = new Mock<MockObjectInstantiateHelper10>();
		m10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object orig, Transform parent) => mockInstantiatedAnim.Object);
		MockObjectInstantiateHelper10.Instance = m10.Object;

		var patchBody = new PlayerControlShapeshiftPatchBody(mockProgress.Object, mockRuntime.Object);
		var (mockPlayer, mockData) = CreateMockPlayer(0);
		mockPlayer.SetupGet(p => p.CurrentOutfitType).Returns(PlayerOutfitType.Default);

		var mockPhysics = new Mock<PlayerPhysics>(IntPtr.Zero);
		var mockPlayerAnim = new Mock<PlayerAnimations>(IntPtr.Zero);
		mockPhysics.SetupGet(p => p.Animations).Returns(mockPlayerAnim.Object);
		mockPlayer.SetupGet(p => p.MyPhysics).Returns(mockPhysics.Object);

		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockGameObject.SetupGet(g => g.transform).Returns(mockTransform.Object);
		mockPlayer.SetupGet(p => p.gameObject).Returns(mockGameObject.Object);

		var mockCosmetics = new Mock<CosmeticsLayer>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.cosmetics).Returns(mockCosmetics.Object);
		localPlayer.SetupGet(p => p.cosmetics).Returns(mockCosmetics.Object);

		var instanceOutfit = new Mock<NetworkedPlayerInfo.PlayerOutfit>(IntPtr.Zero).Object;
		var instanceOutfits = new Mock<Il2CppDictionary>(IntPtr.Zero);
		NetworkedPlayerInfo.PlayerOutfit? instOutfitOut = instanceOutfit;
		instanceOutfits.Setup(d => d.TryGetValue(PlayerOutfitType.Default, out instOutfitOut)).Returns(true);
		mockData.SetupGet(d => d.Outfits).Returns(instanceOutfits.Object);

		var (targetPlayer, targetData) = CreateMockPlayer(1);
		var targetOutfit = new Mock<NetworkedPlayerInfo.PlayerOutfit>(IntPtr.Zero).Object;
		var targetOutfits = new Mock<Il2CppDictionary>(IntPtr.Zero);
		NetworkedPlayerInfo.PlayerOutfit? targetOutfitOut = targetOutfit;
		targetOutfits.Setup(d => d.TryGetValue(PlayerOutfitType.Default, out targetOutfitOut)).Returns(true);
		targetData.SetupGet(d => d.Outfits).Returns(targetOutfits.Object);

		// Act
		bool result = patchBody.Prefix(mockPlayer.Object, targetPlayer.Object, animate: true);

		// Assert
		Assert.False(result);
		mockPlayer.VerifySet(p => p.shapeshifting = true, Times.Once);
		mockInstantiatedAnim.Verify(a => a.SetMaskLayerBasedOnWhoShouldSee(It.IsAny<bool>()), Times.Once);
		mockInstantiatedAnim.Verify(a => a.Play(
			mockPlayer.Object,
			It.IsAny<Il2CppSystem.Action>(),
			It.IsAny<bool>(),
			RoleEffectAnimation.SoundType.Local,
			0f), Times.Once);
	}
}
