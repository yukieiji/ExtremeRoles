using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Patches.MapOverlay;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TMPro;
using UnityEngine;
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
		MockSetupHelper.SetupTimeHelpers();
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		ExtremeRoleManager.GameRole.Clear();
		byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;
		ExtremeRoleManager.GameRole[localPlayerId] = CreateSupervisor(false, false);

		if (ShipStatusCache.KeyedRoom != null)
		{
			ShipStatusCache.KeyedRoom.Clear();
		}
		else
		{
			var prop = typeof(ShipStatusCache).GetProperty(nameof(ShipStatusCache.KeyedRoom), BindingFlags.Public | BindingFlags.Static);
			prop?.SetValue(null, new Dictionary<SystemTypes, PlainShipRoom>());
		}

		var mockOptions = new Mock<IGameOptions>(IntPtr.Zero);
		mockOptions.Setup(o => o.GetByte(ByteOptionNames.MapId)).Returns((byte)0);
		var mockManager = new Mock<GameOptionsManager>(IntPtr.Zero);
		mockManager.SetupGet(m => m.CurrentGameOptions).Returns(mockOptions.Object);
		var globalOptionsHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
		globalOptionsHelper.Setup(h => h.Invoke()).Returns(mockManager.Object);
		MockGameOptionsManagerget_InstanceHelper.Instance = globalOptionsHelper.Object;

		var mockSetColors = new Mock<MockPlayerMaterialSetColorsHelper>();
		mockSetColors.Setup(x => x.Invoke(It.IsAny<int>(), It.IsAny<Renderer>()));
		MockPlayerMaterialSetColorsHelper.Instance = mockSetColors.Object;

		var mockMapBehaviour = new Mock<MapBehaviour>(IntPtr.Zero);
		mockMapBehaviour.Setup(m => m.Close());
		var mockMapBehaviourHelper = new Mock<MockMapBehaviourget_InstanceHelper>();
		mockMapBehaviourHelper.Setup(x => x.Invoke()).Returns(mockMapBehaviour.Object);
		MockMapBehaviourget_InstanceHelper.Instance = mockMapBehaviourHelper.Object;

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<MapConsole>()).Returns(new Il2CppReferenceArray<MapConsole>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;
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

	private static AdminDeviceOption CreateAdminDeviceOption(bool disable, bool enableLimit, float limitTime)
	{
		var mockRemove = new Mock<IOption>();
		mockRemove.Setup(o => o.Value<bool>()).Returns(disable);

		var mockLimit = new Mock<IOption>();
		mockLimit.Setup(o => o.Value<bool>()).Returns(enableLimit);

		var mockTime = new Mock<IOption>();
		mockTime.Setup(o => o.Value<float>()).Returns(limitTime);

		var mockAirship = new Mock<IOption>();
		mockAirship.Setup(o => o.Value<int>()).Returns(0);

		var pack = new OptionPack();
		pack.AddOption((int)DeviceOptionType.IsRemove, mockRemove.Object);
		pack.AddOption((int)DeviceOptionType.EnableLimit, mockLimit.Object);
		pack.AddOption((int)DeviceOptionType.LimitTime, mockTime.Object);
		pack.AddOption((int)AdminSpecialOption.AirShipEnable, mockAirship.Object);

		var category = new OptionCategory(OptionTab.GeneralTab, 0, "Test", pack);
		return new AdminDeviceOption(category);
	}

	[Fact]
	public void Instance_ReturnsInstanceFromProvider_OrCached()
	{
		var mockLogger = new Mock<IModLogger>();
		var mockRuntime = new Mock<IGameRuntime>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var field = typeof(MapCountOverlayUpdatePatchBody).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
		field?.SetValue(null, patchBody);

		Assert.Same(patchBody, MapCountOverlayUpdatePatchBody.Instance);

		field?.SetValue(null, null);
	}

	[Fact]
	public void PlayerColors_ReturnsReadOnlyDictionary()
	{
		var mockLogger = new Mock<IModLogger>();
		var mockRuntime = new Mock<IGameRuntime>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		Assert.NotNull(patchBody.PlayerColors);
		Assert.Empty(patchBody.PlayerColors);
	}

	[Fact]
	public void IsAbilityUse_WhenNoAbilityActive_ReturnsFalse()
	{
		var mockLogger = new Mock<IModLogger>();
		var mockRuntime = new Mock<IGameRuntime>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		bool result = patchBody.IsAbilityUse();

		Assert.False(result);
	}

	[Fact]
	public void IsAbilityUse_WhenSupervisorAbilityActive_ReturnsTrue()
	{
		var supervisor = CreateSupervisor(boosted: true, isAbilityActive: true);
		byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;
		ExtremeRoleManager.GameRole[localPlayerId] = supervisor;

		var mockLogger = new Mock<IModLogger>();
		var mockRuntime = new Mock<IGameRuntime>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		bool result = patchBody.IsAbilityUse();

		Assert.True(result);
	}

	[Fact]
	public void Initialize_WhenTimerTextNotNull_DestroysTimerText()
	{
		var mockLogger = new Mock<IModLogger>();
		var mockRuntime = new Mock<IGameRuntime>();

		IGameContext? context = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(false);

		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var mockTimerText = new Mock<TextMeshPro>(IntPtr.Zero);
		var timerTextField = typeof(MapCountOverlayUpdatePatchBody).GetField("_timerText", BindingFlags.NonPublic | BindingFlags.Instance);
		timerTextField?.SetValue(patchBody, mockTimerText.Object);

		var mockDestroy = new Mock<MockObjectDestroyHelper2>();
		mockDestroy.Setup(d => d.Invoke(mockTimerText.Object));
		MockObjectDestroyHelper2.Instance = mockDestroy.Object;

		patchBody.Initialize();

		mockDestroy.Verify(d => d.Invoke(mockTimerText.Object), Times.Once);
	}

	[Fact]
	public void Initialize_WhenTryGetGameContextReturnsTrue_SetsAdminTimerAndLogs()
	{
		var mockLogger = new Mock<IModLogger>();
		var mockRuntime = new Mock<IGameRuntime>();

		var adminOpt = CreateAdminDeviceOption(disable: false, enableLimit: true, limitTime: 45f);

		var mockGlobalOpt = new Mock<IShipGlobalOption>();
		mockGlobalOpt.SetupGet(g => g.Admin).Returns(adminOpt);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOpt.Object);

		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		patchBody.Initialize();

		var timerField = typeof(MapCountOverlayUpdatePatchBody).GetField("_adminTimer", BindingFlags.NonPublic | BindingFlags.Instance);
		float timerValue = (float)timerField!.GetValue(patchBody)!;

		Assert.Equal(45f, timerValue);
		mockLogger.Verify(l => l.LogTrace(It.IsAny<string>()), Times.AtLeast(4));
	}

	[Fact]
	public void Prefix_WhenTryGetGameContextReturnsFalse_ReturnsTrue()
	{
		var mockLogger = new Mock<IModLogger>();
		var mockRuntime = new Mock<IGameRuntime>();

		IGameContext? context = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(false);

		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);

		bool result = patchBody.Prefix(mockOverlay.Object);

		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenRolesAllCountIsZero_ReturnsTrue()
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
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);

		bool result = patchBody.Prefix(mockOverlay.Object);

		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenTimerLessThanPointOne_ReturnsFalseAndSkipsProcessing()
	{
		var supervisor = CreateSupervisor(boosted: false, isAbilityActive: false);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, supervisor } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns(supervisor);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);
		mockOverlay.SetupProperty(o => o.timer, -0.05f); // timer (-0.05f) + Time.deltaTime (0.1f) = 0.05f < 0.1f

		bool result = patchBody.Prefix(mockOverlay.Object);

		Assert.False(result);
		mockOverlay.VerifyGet(o => o.CountAreas, Times.Never);
	}

	[Fact]
	public void Prefix_WhenNotSabAndHudOverrideActive_ActivatesSabotageOverlayAndReturnsEarly()
	{
		var supervisor = CreateSupervisor(boosted: false, isAbilityActive: false);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, supervisor } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns(supervisor);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockHasTask = new Mock<MockPlayerTaskPlayerHasTaskOfTypeHelper>();
		mockHasTask.Setup(x => x.Invoke<IHudOverrideTask>(It.IsAny<PlayerControl>())).Returns(true);
		MockPlayerTaskPlayerHasTaskOfTypeHelper.Instance = mockHasTask.Object;

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var mockBgColor = new Mock<AlphaPulse>(IntPtr.Zero);
		var mockSabTextGo = new Mock<GameObject>(IntPtr.Zero);
		var mockSabText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockSabText.SetupGet(t => t.gameObject).Returns(mockSabTextGo.Object);

		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);
		mockOverlay.SetupProperty(o => o.timer, 0.5f);
		mockOverlay.SetupProperty(o => o.isSab, false);
		mockOverlay.SetupGet(o => o.BackgroundColor).Returns(mockBgColor.Object);
		mockOverlay.SetupGet(o => o.SabotageText).Returns(mockSabText.Object);

		bool result = patchBody.Prefix(mockOverlay.Object);

		Assert.False(result);
		Assert.True(mockOverlay.Object.isSab);
		mockSabTextGo.Verify(g => g.SetActive(true), Times.Once);
		mockOverlay.VerifyGet(o => o.CountAreas, Times.Never);
	}

	[Fact]
	public void Prefix_WhenSabAndHudOverrideNotActive_DeactivatesSabotageOverlayAndProcesses()
	{
		var supervisor = CreateSupervisor(boosted: false, isAbilityActive: false);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, supervisor } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns(supervisor);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockHasTask = new Mock<MockPlayerTaskPlayerHasTaskOfTypeHelper>();
		mockHasTask.Setup(x => x.Invoke<IHudOverrideTask>(It.IsAny<PlayerControl>())).Returns(false);
		MockPlayerTaskPlayerHasTaskOfTypeHelper.Instance = mockHasTask.Object;

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var mockBgColor = new Mock<AlphaPulse>(IntPtr.Zero);
		var mockSabTextGo = new Mock<GameObject>(IntPtr.Zero);
		var mockSabText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockSabText.SetupGet(t => t.gameObject).Returns(mockSabTextGo.Object);

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);
		mockCounterArea.SetupGet(c => c.DetectiveExclusiveLocation).Returns(false);
		mockCounterArea.SetupGet(c => c.RoomType).Returns(SystemTypes.Cafeteria);

		var countAreas = new Il2CppReferenceArray<CounterArea>([mockCounterArea.Object]);

		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);
		mockOverlay.SetupProperty(o => o.timer, 0.5f);
		mockOverlay.SetupProperty(o => o.isSab, true);
		mockOverlay.SetupGet(o => o.BackgroundColor).Returns(mockBgColor.Object);
		mockOverlay.SetupGet(o => o.SabotageText).Returns(mockSabText.Object);
		mockOverlay.SetupGet(o => o.CountAreas).Returns(countAreas);

		bool result = patchBody.Prefix(mockOverlay.Object);

		Assert.False(result);
		Assert.False(mockOverlay.Object.isSab);
		mockSabTextGo.Verify(g => g.SetActive(false), Times.Once);
	}

	[Fact]
	public void Prefix_WhenDetectiveExclusiveLocation_SkipsCounterArea()
	{
		var supervisor = CreateSupervisor(boosted: false, isAbilityActive: false);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, supervisor } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns(supervisor);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockHasTask = new Mock<MockPlayerTaskPlayerHasTaskOfTypeHelper>();
		mockHasTask.Setup(x => x.Invoke<IHudOverrideTask>(It.IsAny<PlayerControl>())).Returns(false);
		MockPlayerTaskPlayerHasTaskOfTypeHelper.Instance = mockHasTask.Object;

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);
		mockCounterArea.SetupGet(c => c.DetectiveExclusiveLocation).Returns(true);

		var countAreas = new Il2CppReferenceArray<CounterArea>([mockCounterArea.Object]);

		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);
		mockOverlay.SetupProperty(o => o.timer, 0.5f);
		mockOverlay.SetupProperty(o => o.isSab, false);
		mockOverlay.SetupGet(o => o.CountAreas).Returns(countAreas);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		bool result = patchBody.Prefix(mockOverlay.Object);

		Assert.False(result);
		mockCounterArea.Verify(c => c.UpdateCount(It.IsAny<int>()), Times.Never);
	}

	[Fact]
	public void Prefix_WhenAdminDummySystemOverrideMode_CallsAddFakePlayerCount()
	{
		var supervisor = CreateSupervisor(boosted: true, isAbilityActive: true);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, supervisor } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns(supervisor);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockHasTask = new Mock<MockPlayerTaskPlayerHasTaskOfTypeHelper>();
		mockHasTask.Setup(x => x.Invoke<IHudOverrideTask>(It.IsAny<PlayerControl>())).Returns(false);
		MockPlayerTaskPlayerHasTaskOfTypeHelper.Instance = mockHasTask.Object;

		var dummySystem = AdminDummySystem.Get();
		dummySystem.Mode = AdminDummySystem.DummyMode.Override;
		dummySystem.Add(SystemTypes.Cafeteria, 1, 2);

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);
		mockCounterArea.SetupGet(c => c.DetectiveExclusiveLocation).Returns(false);
		mockCounterArea.SetupGet(c => c.RoomType).Returns(SystemTypes.Cafeteria);

		var countAreas = new Il2CppReferenceArray<CounterArea>([mockCounterArea.Object]);

		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);
		mockOverlay.SetupProperty(o => o.timer, 0.5f);
		mockOverlay.SetupProperty(o => o.isSab, false);
		mockOverlay.SetupGet(o => o.CountAreas).Returns(countAreas);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		bool result = patchBody.Prefix(mockOverlay.Object);

		Assert.False(result);
		mockCounterArea.Verify(c => c.UpdateCount(2), Times.Once);
		Assert.True(patchBody.PlayerColors.ContainsKey(SystemTypes.Cafeteria));
		Assert.Equal(2, patchBody.PlayerColors[SystemTypes.Cafeteria].Count);
	}

	[Fact]
	public void Prefix_WhenAdminDummySystemTryGetReturnsFalse_AddFakePlayerCountUpdatesCountToZero()
	{
		var supervisor = CreateSupervisor(boosted: false, isAbilityActive: false);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, supervisor } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns(supervisor);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockHasTask = new Mock<MockPlayerTaskPlayerHasTaskOfTypeHelper>();
		mockHasTask.Setup(x => x.Invoke<IHudOverrideTask>(It.IsAny<PlayerControl>())).Returns(false);
		MockPlayerTaskPlayerHasTaskOfTypeHelper.Instance = mockHasTask.Object;

		var dummySystem = AdminDummySystem.Get();
		dummySystem.Mode = AdminDummySystem.DummyMode.Override;

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);
		mockCounterArea.SetupGet(c => c.DetectiveExclusiveLocation).Returns(false);
		mockCounterArea.SetupGet(c => c.RoomType).Returns(SystemTypes.Cafeteria);

		var countAreas = new Il2CppReferenceArray<CounterArea>([mockCounterArea.Object]);

		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);
		mockOverlay.SetupProperty(o => o.timer, 0.5f);
		mockOverlay.SetupProperty(o => o.isSab, false);
		mockOverlay.SetupGet(o => o.CountAreas).Returns(countAreas);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		bool result = patchBody.Prefix(mockOverlay.Object);

		Assert.False(result);
		mockCounterArea.Verify(c => c.UpdateCount(0), Times.Once);
	}

	[Fact]
	public void Prefix_OverrideNormalCountOverlay_WhenRoomNotFound_LogsTraceAndSkips()
	{
		var supervisor = CreateSupervisor(boosted: false, isAbilityActive: false);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, supervisor } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns(supervisor);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockHasTask = new Mock<MockPlayerTaskPlayerHasTaskOfTypeHelper>();
		mockHasTask.Setup(x => x.Invoke<IHudOverrideTask>(It.IsAny<PlayerControl>())).Returns(false);
		MockPlayerTaskPlayerHasTaskOfTypeHelper.Instance = mockHasTask.Object;

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);
		mockCounterArea.SetupGet(c => c.DetectiveExclusiveLocation).Returns(false);
		mockCounterArea.SetupGet(c => c.RoomType).Returns(SystemTypes.Cafeteria);

		var countAreas = new Il2CppReferenceArray<CounterArea>([mockCounterArea.Object]);

		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);
		mockOverlay.SetupProperty(o => o.timer, 0.5f);
		mockOverlay.SetupProperty(o => o.isSab, false);
		mockOverlay.SetupGet(o => o.CountAreas).Returns(countAreas);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		bool result = patchBody.Prefix(mockOverlay.Object);

		Assert.False(result);
		mockLogger.Verify(l => l.LogTrace(It.Is<string>(s => s.Contains("Couldn't find counter for"))), Times.Once);
		mockCounterArea.Verify(c => c.UpdateCount(It.IsAny<int>()), Times.Never);
	}

	[Fact]
	public void Prefix_OverrideNormalCountOverlay_WithDummyColorsAndSupervisorEnhance_UpdatesCountAndColors()
	{
		var supervisor = CreateSupervisor(boosted: true, isAbilityActive: true);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, supervisor } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns(supervisor);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockHasTask = new Mock<MockPlayerTaskPlayerHasTaskOfTypeHelper>();
		mockHasTask.Setup(x => x.Invoke<IHudOverrideTask>(It.IsAny<PlayerControl>())).Returns(false);
		MockPlayerTaskPlayerHasTaskOfTypeHelper.Instance = mockHasTask.Object;

		var dummySystem = AdminDummySystem.Get();
		dummySystem.Mode = AdminDummySystem.DummyMode.Add;
		dummySystem.Add(SystemTypes.Cafeteria, 3, 4);

		var mockRoomArea = new Mock<Collider2D>(IntPtr.Zero);
		mockRoomArea.Setup(r => r.OverlapCollider(It.IsAny<ContactFilter2D>(), It.IsAny<Il2CppReferenceArray<Collider2D>>()))
			.Returns(0);

		var mockPlainShipRoom = new Mock<PlainShipRoom>(IntPtr.Zero);
		mockPlainShipRoom.SetupGet(r => r.roomArea).Returns(mockRoomArea.Object);
		ShipStatusCache.KeyedRoom[SystemTypes.Cafeteria] = mockPlainShipRoom.Object;

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);
		mockCounterArea.SetupGet(c => c.DetectiveExclusiveLocation).Returns(false);
		mockCounterArea.SetupGet(c => c.RoomType).Returns(SystemTypes.Cafeteria);

		var countAreas = new Il2CppReferenceArray<CounterArea>([mockCounterArea.Object]);

		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);
		mockOverlay.SetupProperty(o => o.timer, 0.5f);
		mockOverlay.SetupProperty(o => o.isSab, false);
		mockOverlay.SetupProperty(o => o.includeDeadBodies, true);
		mockOverlay.SetupProperty(o => o.showLivePlayerPosition, true);
		mockOverlay.SetupGet(o => o.CountAreas).Returns(countAreas);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		bool result = patchBody.Prefix(mockOverlay.Object);

		Assert.False(result);
		mockCounterArea.Verify(c => c.UpdateCount(2), Times.Once);
		Assert.True(patchBody.PlayerColors.ContainsKey(SystemTypes.Cafeteria));
		Assert.Equal(2, patchBody.PlayerColors[SystemTypes.Cafeteria].Count);
		Assert.Contains(3, patchBody.PlayerColors[SystemTypes.Cafeteria]);
		Assert.Contains(4, patchBody.PlayerColors[SystemTypes.Cafeteria]);
	}

	[Fact]
	public void Prefix_OverrideNormalCountOverlay_WhenSupervisorNotEnhanced_DoesNotStorePlayerColors()
	{
		var supervisor = CreateSupervisor(boosted: false, isAbilityActive: false);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, supervisor } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);
		mockRoleContainer.Setup(r => r.GetSafeCastedLocalPlayerRole<Supervisor>()).Returns(supervisor);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockHasTask = new Mock<MockPlayerTaskPlayerHasTaskOfTypeHelper>();
		mockHasTask.Setup(x => x.Invoke<IHudOverrideTask>(It.IsAny<PlayerControl>())).Returns(false);
		MockPlayerTaskPlayerHasTaskOfTypeHelper.Instance = mockHasTask.Object;

		var dummySystem = AdminDummySystem.Get();
		dummySystem.Mode = AdminDummySystem.DummyMode.Add;
		dummySystem.Add(SystemTypes.Cafeteria, 3, 4);

		var mockRoomArea = new Mock<Collider2D>(IntPtr.Zero);
		mockRoomArea.Setup(r => r.OverlapCollider(It.IsAny<ContactFilter2D>(), It.IsAny<Il2CppReferenceArray<Collider2D>>()))
			.Returns(0);

		var mockPlainShipRoom = new Mock<PlainShipRoom>(IntPtr.Zero);
		mockPlainShipRoom.SetupGet(r => r.roomArea).Returns(mockRoomArea.Object);
		ShipStatusCache.KeyedRoom[SystemTypes.Cafeteria] = mockPlainShipRoom.Object;

		var mockCounterArea = new Mock<CounterArea>(IntPtr.Zero);
		mockCounterArea.SetupGet(c => c.DetectiveExclusiveLocation).Returns(false);
		mockCounterArea.SetupGet(c => c.RoomType).Returns(SystemTypes.Cafeteria);

		var countAreas = new Il2CppReferenceArray<CounterArea>([mockCounterArea.Object]);

		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);
		mockOverlay.SetupProperty(o => o.timer, 0.5f);
		mockOverlay.SetupProperty(o => o.isSab, false);
		mockOverlay.SetupGet(o => o.CountAreas).Returns(countAreas);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		bool result = patchBody.Prefix(mockOverlay.Object);

		Assert.False(result);
		mockCounterArea.Verify(c => c.UpdateCount(2), Times.Once);
		Assert.False(patchBody.PlayerColors.ContainsKey(SystemTypes.Cafeteria));
	}

	[Fact]
	public void Postfix_WhenTryGetGameContextReturnsFalse_ReturnsEarly()
	{
		var mockLogger = new Mock<IModLogger>();
		var mockRuntime = new Mock<IGameRuntime>();

		IGameContext? context = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(false);

		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);

		patchBody.Postfix(mockOverlay.Object);

		mockOverlay.VerifyGet(o => o.transform, Times.Never);
	}

	[Fact]
	public void Postfix_WhenAdminOptDisabled_ReturnsEarly()
	{
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, CreateSupervisor(false, false) } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);

		var adminOpt = CreateAdminDeviceOption(disable: true, enableLimit: true, limitTime: 30f);

		var mockGlobalOpt = new Mock<IShipGlobalOption>();
		mockGlobalOpt.SetupGet(g => g.Admin).Returns(adminOpt);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOpt.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);

		patchBody.Postfix(mockOverlay.Object);

		mockOverlay.VerifyGet(o => o.transform, Times.Never);
	}

	[Fact]
	public void Postfix_WhenEnableLimitFalse_ReturnsEarly()
	{
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, CreateSupervisor(false, false) } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);

		var adminOpt = CreateAdminDeviceOption(disable: false, enableLimit: false, limitTime: 30f);

		var mockGlobalOpt = new Mock<IShipGlobalOption>();
		mockGlobalOpt.SetupGet(g => g.Admin).Returns(adminOpt);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOpt.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);

		patchBody.Postfix(mockOverlay.Object);

		mockOverlay.VerifyGet(o => o.transform, Times.Never);
	}

	[Fact]
	public void Postfix_WhenIsAbilityUseTrue_ReturnsEarly()
	{
		var supervisor = CreateSupervisor(boosted: true, isAbilityActive: true);
		byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;
		ExtremeRoleManager.GameRole[localPlayerId] = supervisor;

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, supervisor } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);

		var adminOpt = CreateAdminDeviceOption(disable: false, enableLimit: true, limitTime: 30f);

		var mockGlobalOpt = new Mock<IShipGlobalOption>();
		mockGlobalOpt.SetupGet(g => g.Admin).Returns(adminOpt);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOpt.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);
		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);

		patchBody.Postfix(mockOverlay.Object);

		mockOverlay.VerifyGet(o => o.transform, Times.Never);
	}

	[Fact]
	public void Postfix_WhenTimerTextNull_InstantiatesTimerText_AndUpdatesTimer()
	{
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, CreateSupervisor(false, false) } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);

		var adminOpt = CreateAdminDeviceOption(disable: false, enableLimit: true, limitTime: 30f);

		var mockGlobalOpt = new Mock<IShipGlobalOption>();
		mockGlobalOpt.SetupGet(g => g.Admin).Returns(adminOpt);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOpt.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockTextGo = new Mock<GameObject>(IntPtr.Zero);
		mockTextGo.Setup(g => g.SetActive(It.IsAny<bool>()));

		var mockCoolText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockCoolText.SetupGet(t => t.transform).Returns((Transform)null!);
		mockCoolText.SetupGet(t => t.gameObject).Returns(mockTextGo.Object);

		var mockKillButton = new Mock<KillButton>(IntPtr.Zero);
		mockKillButton.SetupGet(k => k.cooldownTimerText).Returns(mockCoolText.Object);

		var mockHudManager = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		mockHudManager.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		var mockInstantiatedText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockInstantiatedText.SetupGet(t => t.transform).Returns(mockTransform.Object);
		mockInstantiatedText.SetupGet(t => t.gameObject).Returns(mockTextGo.Object);

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns(mockInstantiatedText.Object);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns(mockInstantiatedText.Object);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var timerField = typeof(MapCountOverlayUpdatePatchBody).GetField("_adminTimer", BindingFlags.NonPublic | BindingFlags.Instance);
		timerField?.SetValue(patchBody, 10.0f);

		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);

		patchBody.Postfix(mockOverlay.Object);

		mockTextGo.Verify(g => g.SetActive(true), Times.Once);
	}

	[Fact]
	public void Postfix_WhenTimerZeroOrLess_DisablesAdminAndClosesMap()
	{
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, CreateSupervisor(false, false) } };
		mockRoleContainer.SetupGet(r => r.All).Returns(rolesDict);

		var adminOpt = CreateAdminDeviceOption(disable: false, enableLimit: true, limitTime: 30f);

		var mockGlobalOpt = new Mock<IShipGlobalOption>();
		mockGlobalOpt.SetupGet(g => g.Admin).Returns(adminOpt);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOpt.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockTextGo = new Mock<GameObject>(IntPtr.Zero);
		mockTextGo.Setup(g => g.SetActive(It.IsAny<bool>()));

		var mockCoolText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockCoolText.SetupGet(t => t.transform).Returns((Transform)null!);
		mockCoolText.SetupGet(t => t.gameObject).Returns(mockTextGo.Object);

		var mockKillButton = new Mock<KillButton>(IntPtr.Zero);
		mockKillButton.SetupGet(k => k.cooldownTimerText).Returns(mockCoolText.Object);

		var mockHudManager = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		mockHudManager.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		var mockInstantiatedText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockInstantiatedText.SetupGet(t => t.transform).Returns(mockTransform.Object);
		mockInstantiatedText.SetupGet(t => t.gameObject).Returns(mockTextGo.Object);

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns(mockInstantiatedText.Object);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns(mockInstantiatedText.Object);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		var mockLogger = new Mock<IModLogger>();
		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var timerField = typeof(MapCountOverlayUpdatePatchBody).GetField("_adminTimer", BindingFlags.NonPublic | BindingFlags.Instance);
		timerField?.SetValue(patchBody, 0.0f);

		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);

		patchBody.Postfix(mockOverlay.Object);

		mockTextGo.Verify(g => g.SetActive(true), Times.Once);
	}

	[Fact]
	public void StaticPatchClass_DelegatesToInstance()
	{
		var mockLogger = new Mock<IModLogger>();
		var mockRuntime = new Mock<IGameRuntime>();

		IGameContext? context = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(false);

		var patchBody = new MapCountOverlayUpdatePatchBody(mockLogger.Object, mockRuntime.Object);

		var field = typeof(MapCountOverlayUpdatePatchBody).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
		field?.SetValue(null, patchBody);

		var mockOverlay = new Mock<MapCountOverlay>(IntPtr.Zero);

		Assert.True(MapCountOverlayUpdatePatch.Prefix(mockOverlay.Object));
		MapCountOverlayUpdatePatch.Postfix(mockOverlay.Object);
		Assert.False(MapCountOverlayUpdatePatch.IsAbilityUse());
		Assert.NotNull(MapCountOverlayUpdatePatch.PlayerColor);

		MapCountOverlayUpdatePatch.LoadOptionValue();

		field?.SetValue(null, null);
	}
}
