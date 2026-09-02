using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.Compat;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Helper;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class MapTests : IDisposable
{
	private static readonly IGameOptions globalGameOptions;
	private static readonly GameOptionsManager globalGameOptionsManager;
	private static readonly Mock<MockGameOptionsManagerget_InstanceHelper> globalOptionsHelper = new();

	private byte currentMapId = 0;

	static MapTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();

		var mockOptions = new Mock<IGameOptions>(IntPtr.Zero);
		globalGameOptions = mockOptions.Object;

		var mockManager = new Mock<GameOptionsManager>(IntPtr.Zero);
		mockManager.SetupGet(m => m.CurrentGameOptions).Returns(globalGameOptions);
		globalGameOptionsManager = mockManager.Object;
	}

	public MapTests()
	{
		ResetState();
	}

	public void Dispose()
	{
		ResetState();
	}

	private void ResetState()
	{
		currentMapId = 0;

		Mock.Get(globalGameOptions).Reset();
		Mock.Get(globalGameOptions)
			.Setup(o => o.GetByte(ByteOptionNames.MapId))
			.Returns(() => currentMapId);

		globalOptionsHelper.Setup(h => h.Invoke()).Returns(globalGameOptionsManager);
		MockGameOptionsManagerget_InstanceHelper.Instance = globalOptionsHelper.Object;

		CompatModManager.Instance.RemoveMap();
		PlayerCache.RemovePlayerControl(_ => true);
	}

	[Theory]
	[InlineData((byte)0, Map.SkeldKey)]
	[InlineData((byte)1, Map.MiraHqKey)]
	[InlineData((byte)2, Map.PolusKey)]
	[InlineData((byte)4, Map.AirShipKey)]
	[InlineData((byte)5, Map.FungleKey)]
	[InlineData((byte)3, "")]
	[InlineData((byte)99, "")]
	public void Name_ReturnsCorrectVanillaKey(byte mapId, string expectedKey)
	{
		currentMapId = mapId;

		Assert.Equal(expectedKey, Map.Name);
	}

	[Fact]
	public void Name_WhenSubmergedModMapActive_ReturnsSubmerged()
	{
		var compatManager = CompatModManager.Instance;
		var mapField = typeof(CompatModManager).GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

		var mockSubmergedIntegrator = (SubmergedIntegrator)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(SubmergedIntegrator));
		mapField?.SetValue(compatManager, mockSubmergedIntegrator);

		Assert.Equal("Submerged", Map.Name);
	}

	[Fact]
	public void GetAirShipRandomSpawn_ReturnsParsedVectors()
	{
		var list = Map.GetAirShipRandomSpawn();

		Assert.NotNull(list);
		Assert.NotEmpty(list);
	}

	[Fact]
	public void AddSpawnPoint_WhenModMapActive_CallsModMapGetSpawnPos()
	{
		byte playerId = 1;
		var expectedList = new List<Vector2> { new Vector2(10f, 20f) };

		var mockModMap = new Mock<IMapMod>();
		mockModMap.Setup(m => m.GetSpawnPos(playerId)).Returns(expectedList);

		var compatManager = CompatModManager.Instance;
		var mapField = typeof(CompatModManager).GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		mapField?.SetValue(compatManager, mockModMap.Object);

		var posList = new List<Vector2>();
		Map.AddSpawnPoint(posList, playerId);

		Assert.Contains(new Vector2(10f, 20f), posList);
		mockModMap.Verify(m => m.GetSpawnPos(playerId), Times.Once);
	}

	[Fact]
	public void AddSpawnPoint_VanillaAirShip_AddsRandomSpawn()
	{
		currentMapId = 4; // Airship
		byte playerId = 1;

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		var posList = new List<Vector2>();

		Map.AddSpawnPoint(posList, playerId);

		Assert.NotEmpty(posList);
	}

	[Fact]
	public void AddSpawnPoint_VanillaDefaultMap_AddsInitialAndMeetingSpawn()
	{
		currentMapId = 0; // Skeld
		byte playerId = 1;

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		mockShip.SetupGet(s => s.SpawnRadius).Returns(2.0f);
		mockShip.SetupGet(s => s.InitialSpawnCenter).Returns(new Vector2(0f, 0f));
		mockShip.SetupGet(s => s.MeetingSpawnCenter).Returns(new Vector2(5f, 5f));

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		var mockPc = new Mock<PlayerControl>(IntPtr.Zero);
		PlayerCache.AddPlayerControl(mockPc.Object);

		var posList = new List<Vector2>();

		Map.AddSpawnPoint(posList, playerId);

		Assert.Equal(2, posList.Count);
	}

	[Fact]
	public void AddSpawnPoint_IEnumerableOverload_AppendsPoints()
	{
		currentMapId = 4;
		byte playerId = 1;

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		var initialList = new List<Vector2> { new Vector2(1f, 1f) };

		Map.AddSpawnPoint(initialList, playerId);

		Assert.True(initialList.Count > 1);
	}

	[Fact]
	public void DisableSecurity_WhenNoConsoleFound_DoesNotThrow()
	{
		currentMapId = 0;

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<SystemConsole>()).Returns(new Il2CppReferenceArray<SystemConsole>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var exception = Record.Exception(() => Map.DisableSecurity());
		Assert.Null(exception);
	}

	[Fact]
	public void GetSecuritySystemConsole_WhenModMapActive_DelegatesToModMap()
	{
		var mockConsole = new Mock<SystemConsole>(IntPtr.Zero);
		var mockModMap = new Mock<IMapMod>();
		mockModMap.Setup(m => m.GetSystemConsole(SystemConsoleType.SecurityCamera)).Returns(mockConsole.Object);

		var compatManager = CompatModManager.Instance;
		var mapField = typeof(CompatModManager).GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		mapField?.SetValue(compatManager, mockModMap.Object);

		var result = Map.GetSecuritySystemConsole();

		Assert.Same(mockConsole.Object, result);
		mockModMap.Verify(m => m.GetSystemConsole(SystemConsoleType.SecurityCamera), Times.Once);
	}

	[Theory]
	[InlineData((byte)0, Map.SkeldSecurity)]
	[InlineData((byte)1, Map.MiraHqSecurity)]
	[InlineData((byte)2, Map.PolusSecurity)]
	[InlineData((byte)3, Map.SkeldSecurity)]
	[InlineData((byte)4, Map.AirShipSecurity)]
	[InlineData((byte)5, Map.FangleSecurity)]
	public void GetSecuritySystemConsole_VanillaMaps_ReturnsMatchingConsole(byte mapId, string expectedNameKey)
	{
		currentMapId = mapId;

		var matchedConsoleGo = new Mock<GameObject>(IntPtr.Zero);
		matchedConsoleGo.SetupGet(g => g.name).Returns($"Prefix_{expectedNameKey}_Suffix");

		var matchedConsole = new Mock<SystemConsole>(IntPtr.Zero);
		matchedConsole.SetupGet(c => c.gameObject).Returns(matchedConsoleGo.Object);

		var unmatchedConsoleGo = new Mock<GameObject>(IntPtr.Zero);
		unmatchedConsoleGo.SetupGet(g => g.name).Returns("UnmatchedConsoleName");

		var unmatchedConsole = new Mock<SystemConsole>(IntPtr.Zero);
		unmatchedConsole.SetupGet(c => c.gameObject).Returns(unmatchedConsoleGo.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<SystemConsole>()).Returns(
			new Il2CppReferenceArray<SystemConsole>([unmatchedConsole.Object, matchedConsole.Object]));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var result = Map.GetSecuritySystemConsole();

		Assert.Same(matchedConsole.Object, result);
	}

	[Fact]
	public void GetSecuritySystemConsole_WhenNoMatchingConsole_ReturnsNull()
	{
		currentMapId = 0; // Skeld

		var unmatchedConsoleGo = new Mock<GameObject>(IntPtr.Zero);
		unmatchedConsoleGo.SetupGet(g => g.name).Returns("UnmatchedConsoleName");

		var unmatchedConsole = new Mock<SystemConsole>(IntPtr.Zero);
		unmatchedConsole.SetupGet(c => c.gameObject).Returns(unmatchedConsoleGo.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<SystemConsole>()).Returns(
			new Il2CppReferenceArray<SystemConsole>([unmatchedConsole.Object]));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var result = Map.GetSecuritySystemConsole();

		Assert.Null(result);
	}

	[Fact]
	public void GetSecuritySystemConsole_WhenConsoleListIsEmpty_ReturnsNull()
	{
		currentMapId = 0; // Skeld

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<SystemConsole>()).Returns(
			new Il2CppReferenceArray<SystemConsole>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var result = Map.GetSecuritySystemConsole();

		Assert.Null(result);
	}

	[Fact]
	public void GetVitalSystemConsole_WhenModMapActive_DelegatesToModMap()
	{
		var mockConsole = new Mock<SystemConsole>(IntPtr.Zero);
		var mockModMap = new Mock<IMapMod>();
		mockModMap.Setup(m => m.GetSystemConsole(SystemConsoleType.VitalsLabel)).Returns(mockConsole.Object);

		var compatManager = CompatModManager.Instance;
		var mapField = typeof(CompatModManager).GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		mapField?.SetValue(compatManager, mockModMap.Object);

		var result = Map.GetVitalSystemConsole();

		Assert.Same(mockConsole.Object, result);
	}

	[Theory]
	[InlineData((byte)2, Map.PolusVital)]
	[InlineData((byte)4, Map.AirShipVital)]
	[InlineData((byte)5, Map.FangleVital)]
	public void GetVitalSystemConsole_VanillaMapsWithVitals_ReturnsMatchingConsole(byte mapId, string expectedNameKey)
	{
		currentMapId = mapId;

		var matchedConsoleGo = new Mock<GameObject>(IntPtr.Zero);
		matchedConsoleGo.SetupGet(g => g.name).Returns($"Prefix_{expectedNameKey}_Suffix");

		var matchedConsole = new Mock<SystemConsole>(IntPtr.Zero);
		matchedConsole.SetupGet(c => c.gameObject).Returns(matchedConsoleGo.Object);

		var unmatchedConsoleGo = new Mock<GameObject>(IntPtr.Zero);
		unmatchedConsoleGo.SetupGet(g => g.name).Returns("UnmatchedConsoleName");

		var unmatchedConsole = new Mock<SystemConsole>(IntPtr.Zero);
		unmatchedConsole.SetupGet(c => c.gameObject).Returns(unmatchedConsoleGo.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<SystemConsole>()).Returns(
			new Il2CppReferenceArray<SystemConsole>([unmatchedConsole.Object, matchedConsole.Object]));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var result = Map.GetVitalSystemConsole();

		Assert.Same(matchedConsole.Object, result);
	}

	[Theory]
	[InlineData((byte)0)] // Skeld (No vitals console)
	[InlineData((byte)1)] // MiraHQ (No vitals console)
	[InlineData((byte)99)] // Unknown Map
	public void GetVitalSystemConsole_VanillaMapsWithoutVitals_ReturnsNull(byte mapId)
	{
		currentMapId = mapId;

		var matchedConsoleGo = new Mock<GameObject>(IntPtr.Zero);
		matchedConsoleGo.SetupGet(g => g.name).Returns("panel_vitals");

		var matchedConsole = new Mock<SystemConsole>(IntPtr.Zero);
		matchedConsole.SetupGet(c => c.gameObject).Returns(matchedConsoleGo.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<SystemConsole>()).Returns(
			new Il2CppReferenceArray<SystemConsole>([matchedConsole.Object]));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var result = Map.GetVitalSystemConsole();

		Assert.Null(result);
	}

	[Fact]
	public void GetVitalSystemConsole_WhenNoMatchingConsole_ReturnsNull()
	{
		currentMapId = 2; // Polus

		var unmatchedConsoleGo = new Mock<GameObject>(IntPtr.Zero);
		unmatchedConsoleGo.SetupGet(g => g.name).Returns("UnmatchedConsoleName");

		var unmatchedConsole = new Mock<SystemConsole>(IntPtr.Zero);
		unmatchedConsole.SetupGet(c => c.gameObject).Returns(unmatchedConsoleGo.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<SystemConsole>()).Returns(
			new Il2CppReferenceArray<SystemConsole>([unmatchedConsole.Object]));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var result = Map.GetVitalSystemConsole();

		Assert.Null(result);
	}

	[Fact]
	public void DisableVital_WhenNoVitalConsole_DoesNotThrow()
	{
		currentMapId = 0; // Skeld has no vital

		var exception = Record.Exception(() => Map.DisableVital());
		Assert.Null(exception);
	}

	[Fact]
	public void DisableVital_WhenModMapActive_QueriesModMap()
	{
		var mockModMap = new Mock<IMapMod>();
		mockModMap.Setup(m => m.GetSystemObjectName(SystemConsoleType.VitalsLabel))
			.Returns(new HashSet<string> { "CustomVital" });

		var compatManager = CompatModManager.Instance;
		var mapField = typeof(CompatModManager).GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		mapField?.SetValue(compatManager, mockModMap.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<SystemConsole>()).Returns(new Il2CppReferenceArray<SystemConsole>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var exception = Record.Exception(() => Map.DisableVital());
		Assert.Null(exception);
		mockModMap.Verify(m => m.GetSystemObjectName(SystemConsoleType.VitalsLabel), Times.Once);
	}

	[Fact]
	public void DisableAdmin_WhenModMapActive_QueriesModMap()
	{
		var mockModMap = new Mock<IMapMod>();
		mockModMap.Setup(m => m.GetSystemObjectName(SystemConsoleType.AdminModule))
			.Returns(new HashSet<string> { "CustomAdmin" });

		var compatManager = CompatModManager.Instance;
		var mapField = typeof(CompatModManager).GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		mapField?.SetValue(compatManager, mockModMap.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<MapConsole>()).Returns(new Il2CppReferenceArray<MapConsole>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var exception = Record.Exception(() => Map.DisableAdmin());
		Assert.Null(exception);
		mockModMap.Verify(m => m.GetSystemObjectName(SystemConsoleType.AdminModule), Times.Once);
	}

	[Theory]
	[InlineData((byte)0)]
	[InlineData((byte)1)]
	[InlineData((byte)2)]
	[InlineData((byte)4)]
	[InlineData((byte)5)]
	public void DisableAdmin_VanillaMaps_DoesNotThrow(byte mapId)
	{
		currentMapId = mapId;

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<MapConsole>()).Returns(new Il2CppReferenceArray<MapConsole>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var exception = Record.Exception(() => Map.DisableAdmin());
		Assert.Null(exception);
	}

	[Fact]
	public void DisableConsole_SingleString_DoesNotThrow()
	{
		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<MapConsole>()).Returns(new Il2CppReferenceArray<MapConsole>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var exception = Record.Exception(() => Map.DisableConsole("TestConsole"));
		Assert.Null(exception);
	}

	[Fact]
	public void DisableMapConsole_IReadOnlySet_DoesNotThrow()
	{
		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<MapConsole>()).Returns(new Il2CppReferenceArray<MapConsole>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var exception = Record.Exception(() => Map.DisableMapConsole(new HashSet<string> { "ConsoleA" }));
		Assert.Null(exception);
	}

	[Fact]
	public void DisableSystemConsole_IReadOnlySet_DoesNotThrow()
	{
		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<SystemConsole>()).Returns(new Il2CppReferenceArray<SystemConsole>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var exception = Record.Exception(() => Map.DisableSystemConsole(new HashSet<string> { "SysConsoleA" }));
		Assert.Null(exception);
	}

	[Fact]
	public void GetAdminConsole_WhenModMapActive_ReturnsMatchingConsoles()
	{
		var mockModMap = new Mock<IMapMod>();
		mockModMap.Setup(m => m.GetSystemObjectName(SystemConsoleType.AdminModule))
			.Returns(new HashSet<string> { "CustomAdmin" });

		var compatManager = CompatModManager.Instance;
		var mapField = typeof(CompatModManager).GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		mapField?.SetValue(compatManager, mockModMap.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<MapConsole>()).Returns(new Il2CppReferenceArray<MapConsole>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var result = Map.GetAdminConsole();
		Assert.Empty(result);
	}

	[Theory]
	[InlineData((byte)0)]
	[InlineData((byte)1)]
	[InlineData((byte)2)]
	[InlineData((byte)4)]
	[InlineData((byte)5)]
	public void GetAdminConsole_VanillaMaps_ReturnsArray(byte mapId)
	{
		currentMapId = mapId;

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<MapConsole>()).Returns(new Il2CppReferenceArray<MapConsole>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var result = Map.GetAdminConsole();
		Assert.NotNull(result);
	}

	[Fact]
	public void RelinkVent_MiraHQ_DoesNothingAndReturns()
	{
		currentMapId = 1; // MiraHQ

		var vent0 = CreateMockVent(0);
		var vent1 = CreateMockVent(1);

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		mockShip.SetupGet(s => s.AllVents).Returns(new Il2CppReferenceArray<Vent>([vent0.Object, vent1.Object]));

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		Map.RelinkVent();

		Assert.Null(vent0.Object.Right);
		Assert.Null(vent0.Object.Center);
		Assert.Null(vent0.Object.Left);
		Assert.Null(vent1.Object.Right);
		Assert.Null(vent1.Object.Center);
		Assert.Null(vent1.Object.Left);
	}

	[Theory]
	[InlineData((byte)0, Map.SkeldKey)]
	[InlineData((byte)2, Map.PolusKey)]
	[InlineData((byte)4, Map.AirShipKey)]
	[InlineData((byte)5, Map.FungleKey)]
	public void RelinkVent_VanillaMaps_LinksVentsCorrectly(byte mapId, string mapKey)
	{
		currentMapId = mapId;
		VerifyVentRelinkForMap(mapKey);
	}

	[Fact]
	public void RelinkVent_SubmergedModMap_LinksVentsCorrectly()
	{
		var compatManager = CompatModManager.Instance;
		var mapField = typeof(CompatModManager).GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

		var mockSubmergedIntegrator = (SubmergedIntegrator)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(SubmergedIntegrator));
		mapField?.SetValue(compatManager, mockSubmergedIntegrator);

		VerifyVentRelinkForMap(Map.SubmergedKey);
	}

	private void VerifyVentRelinkForMap(string mapKey)
	{
		var expectedPairs = GetExpectedVentLinks(mapKey);
		Assert.NotEmpty(expectedPairs);

		var ventDict = new Dictionary<int, Mock<Vent>>();
		foreach (var (id1, id2) in expectedPairs)
		{
			if (!ventDict.ContainsKey(id1)) ventDict[id1] = CreateMockVent(id1);
			if (!ventDict.ContainsKey(id2)) ventDict[id2] = CreateMockVent(id2);
		}

		var ventList = new List<Vent>();
		foreach (var kvp in ventDict)
		{
			ventList.Add(kvp.Value.Object);
		}

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		mockShip.SetupGet(s => s.AllVents).Returns(new Il2CppReferenceArray<Vent>(ventList.ToArray()));

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		Map.RelinkVent();

		foreach (var (id1, id2) in expectedPairs)
		{
			var vent1 = ventDict[id1].Object;
			var vent2 = ventDict[id2].Object;

			AssertVentConnected(vent1, vent2);
			AssertVentConnected(vent2, vent1);
		}
	}

	private static Mock<Vent> CreateMockVent(int id)
	{
		var mockVent = new Mock<Vent>(IntPtr.Zero);
		mockVent.SetupGet(v => v.Id).Returns(id);

		Vent? right = null;
		Vent? center = null;
		Vent? left = null;

		mockVent.SetupGet(v => v.Right).Returns(() => right);
		mockVent.SetupSet(v => v.Right = It.IsAny<Vent>()).Callback<Vent>(val => right = val);

		mockVent.SetupGet(v => v.Center).Returns(() => center);
		mockVent.SetupSet(v => v.Center = It.IsAny<Vent>()).Callback<Vent>(val => center = val);

		mockVent.SetupGet(v => v.Left).Returns(() => left);
		mockVent.SetupSet(v => v.Left = It.IsAny<Vent>()).Callback<Vent>(val => left = val);

		return mockVent;
	}

	private static void AssertVentConnected(Vent from, Vent target)
	{
		bool isConnected = ReferenceEquals(from.Right, target) ||
		                   ReferenceEquals(from.Center, target) ||
		                   ReferenceEquals(from.Left, target);
		Assert.True(isConnected, $"Vent {from.Id} is not connected to Vent {target.Id}");
	}

	private static List<(int, int)> GetExpectedVentLinks(string mapKey)
	{
		var jObj = JsonParser.GetJObjectFromAssembly("ExtremeRoles.Resources.JsonData.AllVentLinkInfo.json");
		Assert.NotNull(jObj);

		var jArr = jObj[mapKey] as Newtonsoft.Json.Linq.JArray;
		Assert.NotNull(jArr);

		var list = new List<(int, int)>();
		for (int i = 0; i < jArr.Count; i++)
		{
			var pair = jArr[i] as Newtonsoft.Json.Linq.JArray;
			if (pair != null && pair.Count >= 2)
			{
				int v1 = pair[0] is MockSetupHelper.ManagedJToken m1 ? (int)m1 : (int)pair[0];
				int v2 = pair[1] is MockSetupHelper.ManagedJToken m2 ? (int)m2 : (int)pair[1];
				list.Add((v1, v2));
			}
		}
		return list;
	}
}
