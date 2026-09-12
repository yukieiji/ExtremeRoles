using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Patches.MapOverlay;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using Moq;
using UnityEngine;
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
		MockSetupHelper.SetupTimeHelpers();
		MockSetupHelper.SetupPlayerControlMocks();

		var mockSetColors = new Mock<MockPlayerMaterialSetColorsHelper>();
		mockSetColors.Setup(x => x.Invoke(It.IsAny<int>(), It.IsAny<Renderer>()));
		MockPlayerMaterialSetColorsHelper.Instance = mockSetColors.Object;
	}

	private static Supervisor CreateSupervisor(bool boosted, bool isAbilityActive)
	{
		var supervisor = new Supervisor
		{
			Boosted = boosted
		};
		if (isAbilityActive)
		{
			var button = (ExtremeAbilityButton)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeAbilityButton));
			typeof(ExtremeAbilityButton).GetProperty(nameof(ExtremeAbilityButton.State))?.SetValue(button, AbilityState.Activating);
			typeof(Supervisor).GetField("adminButton", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(supervisor, button);
		}
		return supervisor;
	}

	[Fact]
	public void Postfix_WhenTryGetGameContextReturnsFalse_ReturnsEarly()
	{
		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(false);

		var mockLogger = new Mock<IModLogger>();
		var countOverlayPatchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var patchBody = new CounterAreaUpdatePatchBody(mockRuntime.Object, countOverlayPatchBody);

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);

		patchBody.Postfix(mockCounterArea.Object);

		mockCounterArea.VerifyGet(c => c.pool, Times.Never);
	}

	[Fact]
	public void Postfix_WhenRolesAllCountIsZero_ReturnsEarly()
	{
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var emptyRoles = new Dictionary<byte, SingleRoleBase>();
		mockRoleContainer.SetupGet(r => r.All).Returns(emptyRoles);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockLogger = new Mock<IModLogger>();
		var countOverlayPatchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var patchBody = new CounterAreaUpdatePatchBody(mockRuntime.Object, countOverlayPatchBody);

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);

		patchBody.Postfix(mockCounterArea.Object);

		mockCounterArea.VerifyGet(c => c.pool, Times.Never);
	}

	[Fact]
	public void Postfix_WhenSupervisorIsNull_ReturnsEarly()
	{
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase>
		{
			{ 0, CreateSupervisor(false, false) }
		};
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns((Supervisor)null!);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockLogger = new Mock<IModLogger>();
		var countOverlayPatchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var patchBody = new CounterAreaUpdatePatchBody(mockRuntime.Object, countOverlayPatchBody);

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);

		patchBody.Postfix(mockCounterArea.Object);

		mockCounterArea.VerifyGet(c => c.pool, Times.Never);
	}

	[Fact]
	public void Postfix_WhenPoolBehaviorHasNoSpriteRenderer_ReturnsEarly()
	{
		var supervisor = CreateSupervisor(false, false);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase>
		{
			{ 0, supervisor }
		};
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns(supervisor);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPoolable = new Mock<PoolableBehavior>(IntPtr.Zero);
		SpriteRenderer? defaultRenderer = null;
		mockPoolable.Setup(p => p.TryGetComponent<SpriteRenderer>(out defaultRenderer)).Returns(false);

		var mockObjectPool = new Mock<ObjectPoolBehavior>(IntPtr.Zero);
		mockObjectPool.Setup(p => p.Get<PoolableBehavior>()).Returns(mockPoolable.Object);

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);
		mockCounterArea.SetupGet(c => c.pool).Returns(mockObjectPool.Object);

		var mockLogger = new Mock<IModLogger>();
		var countOverlayPatchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var patchBody = new CounterAreaUpdatePatchBody(mockRuntime.Object, countOverlayPatchBody);

		patchBody.Postfix(mockCounterArea.Object);

		mockPoolable.VerifyGet(p => p.OwnerPool, Times.Never);
	}

	[Fact]
	public void Postfix_WhenNotBoostedOrNotAbilityActive_SetsDefaultMaterialToIcons()
	{
		var supervisor = CreateSupervisor(boosted: false, isAbilityActive: true);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase>
		{
			{ 0, supervisor }
		};
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns(supervisor);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockMaterial = new Mock<Material>(IntPtr.Zero);
		var mockDefaultMaterial = new Mock<Material>(IntPtr.Zero);
		var mockDefaultRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockDefaultRenderer.SetupGet(r => r.material).Returns(mockMaterial.Object);

		var mockInstantiate7 = new Mock<MockObjectInstantiateHelper7>();
		mockInstantiate7.Setup(h => h.Invoke(mockMaterial.Object)).Returns(mockDefaultMaterial.Object);
		MockObjectInstantiateHelper7.Instance = mockInstantiate7.Object;

		var mockPoolable = new Mock<PoolableBehavior>(IntPtr.Zero);
		SpriteRenderer? defaultRenderer = mockDefaultRenderer.Object;
		mockPoolable.Setup(p => p.TryGetComponent<SpriteRenderer>(out defaultRenderer)).Returns(true);

		var mockOwnerPool = new Mock<ObjectPoolBehavior>(IntPtr.Zero);
		mockPoolable.SetupGet(p => p.OwnerPool).Returns(mockOwnerPool.Object);

		var mockObjectPool = new Mock<ObjectPoolBehavior>(IntPtr.Zero);
		mockObjectPool.Setup(p => p.Get<PoolableBehavior>()).Returns(mockPoolable.Object);

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);
		mockCounterArea.SetupGet(c => c.pool).Returns(mockObjectPool.Object);

		var mockIconRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		var mockIcon = new Mock<PoolableBehavior>(IntPtr.Zero);
		SpriteRenderer? iconRenderer = mockIconRenderer.Object;
		mockIcon.Setup(i => i.TryGetComponent<SpriteRenderer>(out iconRenderer)).Returns(true);

		var iconList = new Mock<Il2CppSystem.Collections.Generic.List<PoolableBehavior>>(IntPtr.Zero);
		iconList.SetupGet(l => l.Count).Returns(1);
		iconList.Setup(l => l[0]).Returns(mockIcon.Object);
		mockCounterArea.SetupGet(c => c.myIcons).Returns(iconList.Object);

		var mockLogger = new Mock<IModLogger>();
		var countOverlayPatchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var patchBody = new CounterAreaUpdatePatchBody(mockRuntime.Object, countOverlayPatchBody);

		patchBody.Postfix(mockCounterArea.Object);

		mockOwnerPool.Verify(o => o.Reclaim(mockPoolable.Object), Times.Once);
		mockIconRenderer.VerifySet(r => r.material = mockDefaultMaterial.Object, Times.Once);
	}

	[Fact]
	public void Postfix_WhenPlayerColorsNotFound_ReturnsEarly()
	{
		var supervisor = CreateSupervisor(boosted: true, isAbilityActive: true);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase>
		{
			{ 0, supervisor }
		};
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns(supervisor);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockMaterial = new Mock<Material>(IntPtr.Zero);
		var mockDefaultMaterial = new Mock<Material>(IntPtr.Zero);
		var mockDefaultRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockDefaultRenderer.SetupGet(r => r.material).Returns(mockMaterial.Object);

		var mockInstantiate7 = new Mock<MockObjectInstantiateHelper7>();
		mockInstantiate7.Setup(h => h.Invoke(mockMaterial.Object)).Returns(mockDefaultMaterial.Object);
		MockObjectInstantiateHelper7.Instance = mockInstantiate7.Object;

		var mockPoolable = new Mock<PoolableBehavior>(IntPtr.Zero);
		SpriteRenderer? defaultRenderer = mockDefaultRenderer.Object;
		mockPoolable.Setup(p => p.TryGetComponent<SpriteRenderer>(out defaultRenderer)).Returns(true);

		var mockOwnerPool = new Mock<ObjectPoolBehavior>(IntPtr.Zero);
		mockPoolable.SetupGet(p => p.OwnerPool).Returns(mockOwnerPool.Object);

		var mockObjectPool = new Mock<ObjectPoolBehavior>(IntPtr.Zero);
		mockObjectPool.Setup(p => p.Get<PoolableBehavior>()).Returns(mockPoolable.Object);

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);
		mockCounterArea.SetupGet(c => c.pool).Returns(mockObjectPool.Object);
		mockCounterArea.SetupGet(c => c.RoomType).Returns(SystemTypes.Cafeteria);

		var mockLogger = new Mock<IModLogger>();
		var countOverlayPatchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var patchBody = new CounterAreaUpdatePatchBody(mockRuntime.Object, countOverlayPatchBody);

		patchBody.Postfix(mockCounterArea.Object);

		mockOwnerPool.Verify(o => o.Reclaim(mockPoolable.Object), Times.Once);
		mockCounterArea.VerifyGet(c => c.myIcons, Times.Never);
	}

	[Fact]
	public void Postfix_WhenBoostedAndActiveAndPlayerColorsFound_SetsPlayerColorsToIcons()
	{
		var supervisor = CreateSupervisor(boosted: true, isAbilityActive: true);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase>
		{
			{ 0, supervisor }
		};
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns(supervisor);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockMaterial = new Mock<Material>(IntPtr.Zero);
		var mockDefaultMaterial = new Mock<Material>(IntPtr.Zero);
		var mockInstantiatedMaterial = new Mock<Material>(IntPtr.Zero);
		var mockDefaultRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockDefaultRenderer.SetupGet(r => r.material).Returns(mockMaterial.Object);

		var mockInstantiate7 = new Mock<MockObjectInstantiateHelper7>();
		mockInstantiate7.Setup(h => h.Invoke(mockMaterial.Object)).Returns(mockDefaultMaterial.Object);
		mockInstantiate7.Setup(h => h.Invoke(mockDefaultMaterial.Object)).Returns(mockInstantiatedMaterial.Object);
		MockObjectInstantiateHelper7.Instance = mockInstantiate7.Object;

		var mockPoolable = new Mock<PoolableBehavior>(IntPtr.Zero);
		SpriteRenderer? defaultRenderer = mockDefaultRenderer.Object;
		mockPoolable.Setup(p => p.TryGetComponent<SpriteRenderer>(out defaultRenderer)).Returns(true);

		var mockOwnerPool = new Mock<ObjectPoolBehavior>(IntPtr.Zero);
		mockPoolable.SetupGet(p => p.OwnerPool).Returns(mockOwnerPool.Object);

		var mockObjectPool = new Mock<ObjectPoolBehavior>(IntPtr.Zero);
		mockObjectPool.Setup(p => p.Get<PoolableBehavior>()).Returns(mockPoolable.Object);

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);
		mockCounterArea.SetupGet(c => c.pool).Returns(mockObjectPool.Object);
		mockCounterArea.SetupGet(c => c.RoomType).Returns(SystemTypes.Cafeteria);

		var mockIconRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockIconRenderer.SetupGet(r => r.material).Returns(mockInstantiatedMaterial.Object);

		var mockIcon = new Mock<PoolableBehavior>(IntPtr.Zero);
		SpriteRenderer? iconRenderer = mockIconRenderer.Object;
		mockIcon.Setup(i => i.TryGetComponent<SpriteRenderer>(out iconRenderer)).Returns(true);

		var iconList = new Mock<Il2CppSystem.Collections.Generic.List<PoolableBehavior>>(IntPtr.Zero);
		iconList.SetupGet(l => l.Count).Returns(1);
		iconList.Setup(l => l[0]).Returns(mockIcon.Object);
		mockCounterArea.SetupGet(c => c.myIcons).Returns(iconList.Object);

		var mockLogger = new Mock<IModLogger>();
		var countOverlayPatchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var fieldInfo = typeof(MapCountOverlayUpdatePatchBody).GetField("_playerColors", BindingFlags.NonPublic | BindingFlags.Instance);
		var playerColorsDict = (Dictionary<SystemTypes, IReadOnlyList<int>>)fieldInfo!.GetValue(countOverlayPatchBody)!;
		playerColorsDict[SystemTypes.Cafeteria] = new List<int> { 1 };

		var patchBody = new CounterAreaUpdatePatchBody(mockRuntime.Object, countOverlayPatchBody);

		patchBody.Postfix(mockCounterArea.Object);

		mockOwnerPool.Verify(o => o.Reclaim(mockPoolable.Object), Times.Once);
		mockIconRenderer.VerifySet(r => r.material = mockInstantiatedMaterial.Object, Times.Once);
	}
}
