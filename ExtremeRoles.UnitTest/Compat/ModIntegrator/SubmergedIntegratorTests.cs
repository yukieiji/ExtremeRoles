using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using BepInEx;
using ExtremeRoles.Compat.Initializer;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Compat.ModIntegrator;

public sealed class CustomTaskTypeObj
{
	public object taskType = (TaskTypes)123;
}

public sealed class CustomTaskTypes
{
	public static object? RetrieveOxygenMask = new CustomTaskTypeObj();
}

public sealed class FloorHandler
{
	public static bool onUpper = true;
	public static object? HandlerInstance = null;

	public static bool RpcRequestChangeFloorCalled { get; set; }
	public static bool RpcRequestChangeFloorArg { get; set; }
	public static bool RegisterFloorOverrideCalled { get; set; }
	public static bool RegisterFloorOverrideArg { get; set; }

	public static object? GetFloorHandler(PlayerControl pc)
	{
		return HandlerInstance;
	}

	public void RpcRequestChangeFloor(bool upper)
	{
		RpcRequestChangeFloorCalled = true;
		RpcRequestChangeFloorArg = upper;
	}

	public void RegisterFloorOverride(bool upper)
	{
		RegisterFloorOverrideCalled = true;
		RegisterFloorOverrideArg = upper;
	}
}

public class SubmarineStatus : MonoBehaviour
{
	public object? referenceHolder = null;

	public SubmarineStatus(IntPtr ptr) : base(ptr) { }

	public float CalculateLightRadius(object? player, bool neutral, bool neutralImpostor)
	{
		return 2.5f;
	}
}

public sealed class VentPatchData
{
	public static bool InTransitionValue = false;
	public static bool InTransition => InTransitionValue;
}

public sealed class SubmarineOxygenSystem
{
	public static SubmarineOxygenSystem SystemInstance { get; } = new SubmarineOxygenSystem();
	public static SubmarineOxygenSystem Instance => SystemInstance;

	public static PlayerControl? RepairedPlayer { get; set; }
	public static byte RepairedAmount { get; set; }

	public void RepairDamage(PlayerControl player, byte amount)
	{
		RepairedPlayer = player;
		RepairedAmount = amount;
	}

	public void Deteriorate()
	{
	}
}

public sealed class SubmergedExileController
{
	public static void WrapUpAndSpawn()
	{
	}
}

public sealed class DisplayPrespawnStepPatches
{
	public static void CustomPrespawnStep()
	{
	}
}

public sealed class SubmarineSelectSpawn
{
	public static void OnDestroy()
	{

	}

	public static void CoSelectLevel()
	{
	}
}

public sealed class ChangeFloorButtonPatches
{
	public static void HudUpdatePatch()
	{
	}
}

public sealed class SubmarineSpawnInSystem
{
	public static void Deteriorate()
	{
	}
}

public sealed class SubmarineSurvillanceMinigame
{
	public static void Update()
	{
	}
}

public sealed class ElevatorMover { }

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class SubmergedIntegratorTests : IDisposable
{
	private static readonly IGameOptions globalGameOptions;
	private static readonly GameOptionsManager globalGameOptionsManager;
	private static readonly Mock<MockGameOptionsManagerget_InstanceHelper> globalOptionsHelper = new();

	private const TaskTypes CustomTaskTypeVal = (TaskTypes)123;

	static SubmergedIntegratorTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();

		var mockOptions = new Mock<IGameOptions>(IntPtr.Zero);
		mockOptions.Setup(o => o.GetFloat(FloatOptionNames.CrewLightMod)).Returns(1.0f);
		mockOptions.Setup(o => o.GetFloat(FloatOptionNames.ImpostorLightMod)).Returns(1.5f);
		globalGameOptions = mockOptions.Object;

		var mockManager = new Mock<GameOptionsManager>(IntPtr.Zero);
		mockManager.SetupGet(m => m.CurrentGameOptions).Returns(globalGameOptions);
		globalGameOptionsManager = mockManager.Object;
	}

	public SubmergedIntegratorTests()
	{
		ResetState();
	}

	public void Dispose()
	{
		ResetState();
	}

	private void ResetState()
	{
		globalOptionsHelper.Setup(h => h.Invoke()).Returns(globalGameOptionsManager);
		MockGameOptionsManagerget_InstanceHelper.Instance = globalOptionsHelper.Object;

		VentPatchData.InTransitionValue = false;
		FloorHandler.HandlerInstance = null;
		FloorHandler.RpcRequestChangeFloorCalled = false;
		FloorHandler.RpcRequestChangeFloorArg = false;
		FloorHandler.RegisterFloorOverrideCalled = false;
		FloorHandler.RegisterFloorOverrideArg = false;
		SubmarineOxygenSystem.RepairedPlayer = null;
		SubmarineOxygenSystem.RepairedAmount = 0;
	}

	private sealed class DummyHarmonyPatch : IHarmonyPatch
	{
		public void Patch(
			System.Reflection.MethodBase original,
			HarmonyLib.HarmonyMethod? prefix = null,
			HarmonyLib.HarmonyMethod? postfix = null,
			HarmonyLib.HarmonyMethod? transpiler = null,
			HarmonyLib.HarmonyMethod? finalizer = null,
			HarmonyLib.HarmonyMethod? ilmanipulator = null)
		{
		}
	}

	private static SubmergedIntegrator CreateSubmergedIntegrator()
	{
		var basePlugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		var metadata = new BepInEx.BepInPlugin("Submerged", "Submerged", "1.0.0");

		var pluginInfo = new BepInEx.PluginInfo();
		typeof(BepInEx.PluginInfo)
			.GetField("<Instance>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
			?.SetValue(pluginInfo, basePlugin);
		typeof(BepInEx.PluginInfo)
			.GetField("<Metadata>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
			?.SetValue(pluginInfo, metadata);

		var mockAccessTool = new Mock<IAccessTool>();
		mockAccessTool
			.Setup(a => a.GetTypesFromAssembly(It.IsAny<Assembly>()))
			.Returns(new[]
			{
				typeof(CustomTaskTypes),
				typeof(ShipStatus),
				typeof(FloorHandler),
				typeof(SubmarineStatus),
				typeof(VentPatchData),
				typeof(SubmarineOxygenSystem),
				typeof(ElevatorMover),
				typeof(SubmergedExileController),
				typeof(DisplayPrespawnStepPatches),
				typeof(SubmarineSelectSpawn),
				typeof(ChangeFloorButtonPatches),
				typeof(SubmarineSpawnInSystem),
				typeof(SubmarineSurvillanceMinigame)
			});

		mockAccessTool
			.Setup(a => a.GetField(It.IsAny<Type>(), It.IsAny<string>()))
			.Returns((Type t, string name) =>
				t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!);

		mockAccessTool
			.Setup(a => a.GetMethod(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<Type[]?>()))
			.Returns((Type t, string name, Type[]? p) =>
				t.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!);

		mockAccessTool
			.Setup(a => a.GetProperty(It.IsAny<Type>(), It.IsAny<string>()))
			.Returns((Type t, string name) =>
				t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!);

		var patch = new DummyHarmonyPatch();
		var initializer = new SubmergedInitializer(pluginInfo, mockAccessTool.Object, patch);

		var integrator = new SubmergedIntegrator(initializer);

		var getterField = typeof(SubmergedIntegrator).GetField("submarineOxygenSystemInstanceGetter", BindingFlags.NonPublic | BindingFlags.Instance);
		var methodField = typeof(SubmergedIntegrator).GetField("submarineOxygenSystemRepairDamageMethod", BindingFlags.NonPublic | BindingFlags.Instance);

		getterField?.SetValue(integrator, typeof(SubmarineOxygenSystem).GetProperty(nameof(SubmarineOxygenSystem.Instance)));
		methodField?.SetValue(integrator, typeof(SubmarineOxygenSystem).GetMethod(nameof(SubmarineOxygenSystem.RepairDamage)));

		var getFloorHandlerInfoField = typeof(SubmergedIntegrator).GetField("getFloorHandlerInfo", BindingFlags.NonPublic | BindingFlags.Instance);
		getFloorHandlerInfoField?.SetValue(integrator, typeof(FloorHandler).GetMethod(nameof(FloorHandler.GetFloorHandler)));

		return integrator;
	}

	[Fact]
	public void Constructor_Initialization_SetsPropertiesCorrectly()
	{
		// Arrange & Act
		var integrator = CreateSubmergedIntegrator();

		// Assert
		Assert.Equal((byte)6, integrator.MapId);
		Assert.Equal((ShipStatus.MapType)6, integrator.MapType);
		Assert.False(integrator.CanPlaceCamera);
		Assert.True(integrator.IsCustomCalculateLightRadius);
		Assert.Equal(CustomTaskTypeVal, integrator.RetrieveOxygenMask);
	}

	[Fact]
	public void CalculateLightRadius_WithSubmarineStatus_InvokesCalculateAndReturnsValue()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();
		var mockSubStatus = new Mock<SubmarineStatus>(IntPtr.Zero);
		typeof(SubmergedIntegrator)
			.GetField("submarineStatus", BindingFlags.NonPublic | BindingFlags.Instance)
			?.SetValue(integrator, mockSubStatus.Object);

		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		// Act
		float radius1 = integrator.CalculateLightRadius(mockPlayerInfo.Object, false, false);
		float radius2 = integrator.CalculateLightRadius(mockPlayerInfo.Object, 1.2f, true);

		// Assert
		Assert.Equal(2.5f, radius1);
		Assert.Equal(2.5f, radius2);
	}

	[Fact]
	public void GetFloor_Vector3Overload_ReturnsExpectedFloor()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();
		var pos = new Vector3(0f, 0f, 0f);

		// Act
		int floor = integrator.GetFloor(pos);

		// Assert
		Assert.Equal(1, floor);
	}

	[Fact]
	public void GetFloor_PlayerControlOverload_WhenFloorHandlerNull_ReturnsMaxValue()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();
		FloorHandler.HandlerInstance = null;
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		// Act
		int floor = integrator.GetFloor(mockPlayer.Object);

		// Assert
		Assert.Equal(int.MaxValue, floor);
	}

	[Fact]
	public void ChangeFloor_WhenFloorGreaterThan1_DoesNothing()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		// Act
		integrator.ChangeFloor(mockPlayer.Object, 2);

		// Assert
		Assert.False(FloorHandler.RpcRequestChangeFloorCalled);
		Assert.False(FloorHandler.RegisterFloorOverrideCalled);
	}

	[Fact]
	public void ChangeFloor_WhenFloorHandlerNull_DoesNothing()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();
		FloorHandler.HandlerInstance = null;
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		// Act
		integrator.ChangeFloor(mockPlayer.Object, 1);

		// Assert
		Assert.False(FloorHandler.RpcRequestChangeFloorCalled);
		Assert.False(FloorHandler.RegisterFloorOverrideCalled);
	}

	[Fact]
	public void GetConsole_FixLights_ReturnsMatchingConsole()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		var mockGo1 = new Mock<GameObject>(IntPtr.Zero);
		mockGo1.SetupGet(g => g.name).Returns("LightsConsole_Obj");
		var mockConsole1 = new Mock<Console>(IntPtr.Zero);
		mockConsole1.SetupGet(c => c.gameObject).Returns(mockGo1.Object);

		var mockGo2 = new Mock<GameObject>(IntPtr.Zero);
		mockGo2.SetupGet(g => g.name).Returns("OtherConsole");
		var mockConsole2 = new Mock<Console>(IntPtr.Zero);
		mockConsole2.SetupGet(c => c.gameObject).Returns(mockGo2.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<Console>()).Returns(
			new Il2CppReferenceArray<Console>([mockConsole2.Object, mockConsole1.Object]));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		// Act
		var result = integrator.GetConsole(TaskTypes.FixLights);

		// Assert
		Assert.Same(mockConsole1.Object, result);
	}

	[Fact]
	public void GetConsole_StopCharles_ReturnsBallastConsole()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		var mockGo1 = new Mock<GameObject>(IntPtr.Zero);
		mockGo1.SetupGet(g => g.name).Returns("BallastConsole_1");
		var mockConsole1 = new Mock<Console>(IntPtr.Zero);
		mockConsole1.SetupGet(c => c.gameObject).Returns(mockGo1.Object);

		var mockGo2 = new Mock<GameObject>(IntPtr.Zero);
		mockGo2.SetupGet(g => g.name).Returns("BallastConsole_2");
		var mockConsole2 = new Mock<Console>(IntPtr.Zero);
		mockConsole2.SetupGet(c => c.gameObject).Returns(mockGo2.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<Console>()).Returns(
			new Il2CppReferenceArray<Console>([mockConsole1.Object, mockConsole2.Object]));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		// Act
		var result = integrator.GetConsole(TaskTypes.StopCharles);

		// Assert
		Assert.NotNull(result);
		Assert.True(result == mockConsole1.Object || result == mockConsole2.Object);
	}

	[Fact]
	public void GetConsole_UnhandledTask_ReturnsNull()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<Console>()).Returns(
			new Il2CppReferenceArray<Console>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		// Act
		var result = integrator.GetConsole(TaskTypes.SwipeCard);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void GetSpawnPos_ReturnsTwoPositionsBasedOnPlayerId()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		mockShip.SetupGet(s => s.SpawnRadius).Returns(2.0f);
		mockShip.SetupGet(s => s.InitialSpawnCenter).Returns(new Vector2(0f, 0f));

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.SetupGet(g => g.PlayerCount).Returns(5);

		// Act
		List<Vector2> spawnPos = integrator.GetSpawnPos(1);

		// Assert
		Assert.Equal(2, spawnPos.Count);
		Assert.Equal(spawnPos[0] + new Vector2(0.0f, 48.119f), spawnPos[1]);
	}

	[Theory]
	[InlineData(SystemConsoleType.AdminModule, 2)]
	[InlineData(SystemConsoleType.VitalsLabel, 1)]
	[InlineData(SystemConsoleType.SecurityCamera, 1)]
	[InlineData(SystemConsoleType.EmergencyButton, 0)]
	public void GetSystemObjectName_ReturnsExpectedCount(SystemConsoleType consoleType, int expectedCount)
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		// Act
		HashSet<string> names = integrator.GetSystemObjectName(consoleType);

		// Assert
		Assert.Equal(expectedCount, names.Count);
	}

	[Fact]
	public void GetSystemConsole_ReturnsMatchingConsole()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		var mockGoCam = new Mock<GameObject>(IntPtr.Zero);
		mockGoCam.SetupGet(g => g.name).Returns("SecurityConsole");
		var mockConsoleCam = new Mock<SystemConsole>(IntPtr.Zero);
		mockConsoleCam.SetupGet(c => c.gameObject).Returns(mockGoCam.Object);

		var mockGoVit = new Mock<GameObject>(IntPtr.Zero);
		mockGoVit.SetupGet(g => g.name).Returns("panel_vitals(Clone)");
		var mockConsoleVit = new Mock<SystemConsole>(IntPtr.Zero);
		mockConsoleVit.SetupGet(c => c.gameObject).Returns(mockGoVit.Object);

		var mockGoEm = new Mock<GameObject>(IntPtr.Zero);
		mockGoEm.SetupGet(g => g.name).Returns("console-mr-callmeeting");
		var mockConsoleEm = new Mock<SystemConsole>(IntPtr.Zero);
		mockConsoleEm.SetupGet(c => c.gameObject).Returns(mockGoEm.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<SystemConsole>()).Returns(
			new Il2CppReferenceArray<SystemConsole>([
				mockConsoleCam.Object,
				mockConsoleVit.Object,
				mockConsoleEm.Object
			]));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		// Act
		var resCam = integrator.GetSystemConsole(SystemConsoleType.SecurityCamera);
		var resVit = integrator.GetSystemConsole(SystemConsoleType.VitalsLabel);
		var resEm = integrator.GetSystemConsole(SystemConsoleType.EmergencyButton);
		var resOther = integrator.GetSystemConsole(SystemConsoleType.AdminModule);

		// Assert
		Assert.Same(mockConsoleCam.Object, resCam);
		Assert.Same(mockConsoleVit.Object, resVit);
		Assert.Same(mockConsoleEm.Object, resEm);
		Assert.Null(resOther);
	}

	[Fact]
	public void IsCustomSabotageTask_ReturnsTrueOnlyForOxygenTask()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		// Act
		bool isCustom = integrator.IsCustomSabotageTask(CustomTaskTypeVal);
		bool isNotCustom = integrator.IsCustomSabotageTask(TaskTypes.FixLights);

		// Assert
		Assert.True(isCustom);
		Assert.False(isNotCustom);
	}

	[Fact]
	public void IsCustomSabotageNow_WhenLocalPlayerHasCustomTask_ReturnsTrue()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		var mockTask = new Mock<NormalPlayerTask>(IntPtr.Zero);
		mockTask.SetupGet(t => t.TaskType).Returns(CustomTaskTypeVal);

		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(mockTask.Object);

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

		// Act
		bool isSabotageNow = integrator.IsCustomSabotageNow();

		// Assert
		Assert.True(isSabotageNow);
	}

	[Fact]
	public void IsCustomSabotageNow_WhenLocalPlayerHasNoCustomTask_ReturnsFalse()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		var mockTask = new Mock<NormalPlayerTask>(IntPtr.Zero);
		mockTask.SetupGet(t => t.TaskType).Returns(TaskTypes.FixLights);

		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(mockTask.Object);

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

		// Act
		bool isSabotageNow = integrator.IsCustomSabotageNow();

		// Assert
		Assert.False(isSabotageNow);
	}

	[Theory]
	[InlineData(9, true, false)]
	[InlineData(9, false, true)]
	[InlineData(0, false, true)]
	[InlineData(14, false, true)]
	[InlineData(5, false, false)]
	public void IsCustomVentUse_ReturnsExpectedResult(int ventId, bool inVent, bool expectedResult)
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		var mockVent = new Mock<Vent>(IntPtr.Zero);
		mockVent.SetupGet(v => v.Id).Returns(ventId);

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockPlayer.SetupGet(p => p.inVent).Returns(inVent);

		// Act
		bool canUse = integrator.IsCustomVentUse(mockVent.Object);

		// Assert
		Assert.Equal(expectedResult, canUse);
	}

	[Fact]
	public void IsCustomVentUseResult_WhenInTransition_ReturnsMaxValue()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();
		VentPatchData.InTransitionValue = true;

		var mockVent = new Mock<Vent>(IntPtr.Zero);
		mockVent.SetupGet(v => v.Id).Returns(0);

		var playerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero).Object;

		// Act
		var (dist, canUse, couldUse) = integrator.IsCustomVentUseResult(mockVent.Object, playerInfo, true);

		// Assert
		Assert.Equal(float.MaxValue, dist);
		Assert.False(canUse);
		Assert.False(couldUse);
	}



	[Fact]
	public void IsCustomVentUseResult_UnhandledVent_ReturnsMaxValue()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();
		VentPatchData.InTransitionValue = false;

		var mockVent = new Mock<Vent>(IntPtr.Zero);
		mockVent.SetupGet(v => v.Id).Returns(5);

		var playerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero).Object;

		// Act
		var (dist, canUse, couldUse) = integrator.IsCustomVentUseResult(mockVent.Object, playerInfo, true);

		// Assert
		Assert.Equal(float.MaxValue, dist);
		Assert.False(canUse);
		Assert.False(couldUse);
	}

	[Fact]
	public void RepairCustomSabotage_MatchingTaskType_CallsRepairDamage()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		// Act
		integrator.RepairCustomSabotage(CustomTaskTypeVal);

		// Assert
		Assert.Same(mockPlayer.Object, SubmarineOxygenSystem.RepairedPlayer);
		Assert.Equal((byte)64, SubmarineOxygenSystem.RepairedAmount);
		mockShip.Verify(s => s.RpcUpdateSystem((SystemTypes)130, 64), Times.Once);
	}

	[Fact]
	public void RepairCustomSabotage_NonMatchingTaskType_DoesNotCallRepairDamage()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		// Act
		integrator.RepairCustomSabotage(TaskTypes.FixLights);

		// Assert
		Assert.Null(SubmarineOxygenSystem.RepairedPlayer);
		mockShip.Verify(s => s.RpcUpdateSystem(It.IsAny<SystemTypes>(), It.IsAny<byte>()), Times.Never);
	}

	[Fact]
	public void RepairCustomSabotage_NoArgsOverload_CallsRepairDamageForOxygenMask()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		// Act
		integrator.RepairCustomSabotage();

		// Assert
		Assert.Same(mockPlayer.Object, SubmarineOxygenSystem.RepairedPlayer);
		Assert.Equal((byte)64, SubmarineOxygenSystem.RepairedAmount);
		mockShip.Verify(s => s.RpcUpdateSystem((SystemTypes)130, 64), Times.Once);
	}

	/*
	[Fact]
	public void AddCustomComponent_MovableFloorBehaviour_ThrowsArgumentException()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);

		// Act & Assert
		Assert.Throws<ArgumentException>(() => integrator.AddCustomComponent(mockGameObject.Object, CustomMonoBehaviourType.MovableFloorBehaviour));
	}
	*/

	[Fact]
	public void SetUpNewCamera_WhenFixConsoleNull_SearchesFixConsoleChild()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		var mockCameraTransform = new Mock<Transform>(IntPtr.Zero);
		mockCameraTransform.Setup(t => t.FindChild("FixConsole")).Returns((Transform)null!);

		var mockCamera = new Mock<SurvCamera>(IntPtr.Zero);
		mockCamera.SetupGet(c => c.transform).Returns(mockCameraTransform.Object);

		// Act
		integrator.SetUpNewCamera(mockCamera.Object);

		// Assert
		mockCameraTransform.Verify(t => t.FindChild("FixConsole"), Times.Once);
	}

	[Fact]
	public void SetUpNewCamera_WhenFixConsoleNotNull_SearchesFixConsoleChild()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		var mockFixConsole = new Mock<Transform>(IntPtr.Zero);

		var mockCameraTransform = new Mock<Transform>(IntPtr.Zero);
		mockCameraTransform.Setup(t => t.FindChild("FixConsole")).Returns(mockFixConsole.Object);

		var mockCamera = new Mock<SurvCamera>(IntPtr.Zero);
		mockCamera.SetupGet(c => c.transform).Returns(mockCameraTransform.Object);

		// Act
		integrator.SetUpNewCamera(mockCamera.Object);

		// Assert
		mockCameraTransform.Verify(t => t.FindChild("FixConsole"), Times.Once);
	}

	[Fact]
	public void CreateIntegrateOption_CreatesAndRegistersOptions()
	{
		// Arrange
		var integrator = CreateSubmergedIntegrator();

		OptionCategory? registeredCategory = null;

		using (var factory = new SequentialOptionCategoryFactory(
			"SubmergedCategory",
			2000,
			(p, c) => { },
			(t, c) => registeredCategory = c
		))
		{
			if (!OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)ShipGlobalOptionCategory.RandomSpawnOption, out _))
			{
				OptionPack pack = new OptionPack();
				var cate = new OptionCategory(OptionTab.GeneralTab, (int)ShipGlobalOptionCategory.RandomSpawnOption, "RandomSpawnCategory", pack);
				OptionManager.Instance.RegisterOptionGroup(OptionTab.GeneralTab, cate);
			}

			integrator.CreateIntegrateOption(factory);
		}

		// Assert
		var elevatorField = typeof(SubmergedIntegrator).GetField("elevatorOption", BindingFlags.NonPublic | BindingFlags.Instance);
		var doorField = typeof(SubmergedIntegrator).GetField("replaceDoorMinigameOption", BindingFlags.NonPublic | BindingFlags.Instance);

		var elevatorOpt = elevatorField?.GetValue(integrator) as IOption;
		var doorOpt = doorField?.GetValue(integrator) as IOption;

		Assert.NotNull(elevatorOpt);
		Assert.NotNull(doorOpt);
	}
}
