using System;
using System.Collections.Generic;
using BepInEx;
using ExtremeRoles.Compat;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.Core.Abstract;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Compat;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class CompatModManagerTests
{
    private sealed class DummyMapIntegrator : ModIntegratorBase, IMapMod, IIntegrateOption
    {
        public bool AwakeCalled { get; private set; }
        public bool DestroyCalled { get; private set; }
        public bool RepairAllSaboCalled { get; private set; }
        public TaskTypes? RepairedSaboTask { get; private set; }
        public bool CreateOptionCalled { get; private set; }

        public DummyMapIntegrator(IInitializer init) : base(init)
        {
        }

        public byte MapId => 6;
        public ShipStatus.MapType MapType => (ShipStatus.MapType)MapId;
        public bool CanPlaceCamera => false;
        public bool IsCustomCalculateLightRadius => false;

        public void Awake(ShipStatus map)
        {
            AwakeCalled = true;
        }

        public void Destroy()
        {
            DestroyCalled = true;
        }

        public float CalculateLightRadius(NetworkedPlayerInfo player, bool neutral, bool neutralImpostor) => 1.0f;
        public float CalculateLightRadius(NetworkedPlayerInfo player, float visionMod, bool applayVisionEffects = true) => 1.0f;
        public bool IsCustomSabotageNow() => false;
        public bool IsCustomSabotageTask(TaskTypes saboTask) => false;
        public bool IsCustomVentUse(Vent vent) => false;
        public (float, bool, bool) IsCustomVentUseResult(Vent vent, NetworkedPlayerInfo player, bool isVentUse) => (0f, false, false);
        public void RpcRepairCustomSabotage()
        {
        }
        public void RpcRepairCustomSabotage(TaskTypes saboTask)
        {
        }
        public void RepairCustomSabotage()
        {
            RepairAllSaboCalled = true;
        }

        public void RepairCustomSabotage(TaskTypes saboTask)
        {
            RepairedSaboTask = saboTask;
        }

        public Console GetConsole(TaskTypes task) => null!;
        public HashSet<string> GetSystemObjectName(SystemConsoleType sysConsole) => new();
        public SystemConsole GetSystemConsole(SystemConsoleType sysConsole) => null!;
        public List<Vector2> GetSpawnPos(byte playerId) => new();
        public void AddCustomComponent(GameObject addObject, CustomMonoBehaviourType customMonoType)
        {
        }

        public void SetUpNewCamera(SurvCamera camera)
        {
        }

        public void CreateIntegrateOption(ExtremeRoles.Module.CustomOption.Factory.SequentialOptionCategoryFactory factory)
        {
            CreateOptionCalled = true;
        }
    }

    public CompatModManagerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupCompatModManager();
    }

    [Fact]
    public void ModInfo_ContainsExpectedKeys()
    {
        Assert.True(CompatModManager.ModInfo.ContainsKey(CompatModType.Submerged));
        Assert.True(CompatModManager.ModInfo.ContainsKey(CompatModType.CrowdedMod));
    }

    [Fact]
    public void Initialize_SetsInstanceProperty()
    {
        Assert.NotNull(CompatModManager.Instance);
    }

    [Fact]
    public void TryGetMod_TruePattern_ReturnsTrueAndSetsMod()
    {
        var mockLoader = new Mock<IPluginLoader>();
        var mockFactory = new Mock<IModInitializerFactory>();
        var mockLogger = new Mock<IModLogger>();

        var mockPluginInfo = new PluginInfo();
        mockLoader.Setup(l => l.TryGetPlugin(CrowdedMod.Guid, out mockPluginInfo)).Returns(true);

        var mockInitializer = new Mock<IInitializer>();
        mockInitializer.Setup(i => i.Version).Returns(new SemanticVersioning.Version("1.0.0"));
        mockInitializer.Setup(i => i.Name).Returns("CrowdedMod");

        var dummyIntegrator = new DummyMapIntegrator(mockInitializer.Object);
        mockInitializer.Setup(i => i.Initialize()).Returns(dummyIntegrator);

        mockFactory.Setup(f => f.Create(It.IsAny<Type>(), It.IsAny<PluginInfo>()))
                   .Returns(mockInitializer.Object);

        var manager = new CompatModManager(mockLoader.Object, mockFactory.Object, mockLogger.Object);

        bool success = manager.TryGetMod<DummyMapIntegrator>(CompatModType.CrowdedMod, out var mod);

        Assert.True(success);
        Assert.NotNull(mod);
        Assert.Same(dummyIntegrator, mod);
    }

    [Fact]
    public void TryGetMod_FalsePattern_ReturnsFalseWhenNotLoadedOrTypeMismatch()
    {
        var mockLoader = new Mock<IPluginLoader>();
        var mockFactory = new Mock<IModInitializerFactory>();
        var mockLogger = new Mock<IModLogger>();

        var manager = new CompatModManager(mockLoader.Object, mockFactory.Object, mockLogger.Object);

        bool result1 = manager.TryGetMod<CrowdedMod>(CompatModType.CrowdedMod, out var mod1);
        Assert.False(result1);
        Assert.Null(mod1);

        bool result2 = manager.TryGetMod<CrowdedMod>(CompatModType.Submerged, out var mod2);
        Assert.False(result2);
        Assert.Null(mod2);
    }

    [Fact]
    public void SetUpMap_And_TryGetModMap_TruePatterns()
    {
        var mockLoader = new Mock<IPluginLoader>();
        var mockFactory = new Mock<IModInitializerFactory>();
        var mockLogger = new Mock<IModLogger>();

        var mockPluginInfo = new PluginInfo();
        mockLoader.Setup(l => l.TryGetPlugin(SubmergedIntegrator.Guid, out mockPluginInfo)).Returns(true);

        var mockInitializer = new Mock<IInitializer>();
        mockInitializer.Setup(i => i.Version).Returns(new SemanticVersioning.Version("1.0.0"));
        mockInitializer.Setup(i => i.Name).Returns("Submerged");

        var dummyMapIntegrator = new DummyMapIntegrator(mockInitializer.Object);
        mockInitializer.Setup(i => i.Initialize()).Returns(dummyMapIntegrator);

        mockFactory.Setup(f => f.Create(It.IsAny<Type>(), It.IsAny<PluginInfo>()))
                   .Returns(mockInitializer.Object);

        var manager = new CompatModManager(mockLoader.Object, mockFactory.Object, mockLogger.Object);

        var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
        mockShip.SetupGet(s => s.Type).Returns((ShipStatus.MapType)6);

        manager.SetUpMap(mockShip.Object);

        Assert.True(dummyMapIntegrator.AwakeCalled);

        Assert.True(manager.IsModMap<DummyMapIntegrator>());

        bool hasMap = manager.TryGetModMap(out var mapMod);
        Assert.True(hasMap);
        Assert.Same(dummyMapIntegrator, mapMod);

        bool hasTypedMap = manager.TryGetModMap<DummyMapIntegrator>(out var typedMap);
        Assert.True(hasTypedMap);
        Assert.Same(dummyMapIntegrator, typedMap);
    }

    [Fact]
    public void RemoveMap_CallsDestroy_AndResetsMap()
    {
        var mockLoader = new Mock<IPluginLoader>();
        var mockFactory = new Mock<IModInitializerFactory>();
        var mockLogger = new Mock<IModLogger>();

        var mockPluginInfo = new PluginInfo();
        mockLoader.Setup(l => l.TryGetPlugin(SubmergedIntegrator.Guid, out mockPluginInfo)).Returns(true);

        var mockInitializer = new Mock<IInitializer>();
        mockInitializer.Setup(i => i.Version).Returns(new SemanticVersioning.Version("1.0.0"));
        mockInitializer.Setup(i => i.Name).Returns("Submerged");

        var dummyMapIntegrator = new DummyMapIntegrator(mockInitializer.Object);
        mockInitializer.Setup(i => i.Initialize()).Returns(dummyMapIntegrator);

        mockFactory.Setup(f => f.Create(It.IsAny<Type>(), It.IsAny<PluginInfo>()))
                   .Returns(mockInitializer.Object);

        var manager = new CompatModManager(mockLoader.Object, mockFactory.Object, mockLogger.Object);

        var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
        mockShip.SetupGet(s => s.Type).Returns((ShipStatus.MapType)6);

        manager.SetUpMap(mockShip.Object);
        Assert.True(manager.TryGetModMap(out _));

        manager.RemoveMap();

        Assert.True(dummyMapIntegrator.DestroyCalled);
        Assert.False(manager.TryGetModMap(out _));
    }

    [Fact]
    public void IntegrateModCall_TriggersRepairSabotageOnMap()
    {
        var mockLoader = new Mock<IPluginLoader>();
        var mockFactory = new Mock<IModInitializerFactory>();
        var mockLogger = new Mock<IModLogger>();

        var mockPluginInfo = new PluginInfo();
        mockLoader.Setup(l => l.TryGetPlugin(SubmergedIntegrator.Guid, out mockPluginInfo)).Returns(true);

        var mockInitializer = new Mock<IInitializer>();
        mockInitializer.Setup(i => i.Version).Returns(new SemanticVersioning.Version("1.0.0"));
        mockInitializer.Setup(i => i.Name).Returns("Submerged");

        var dummyMapIntegrator = new DummyMapIntegrator(mockInitializer.Object);
        mockInitializer.Setup(i => i.Initialize()).Returns(dummyMapIntegrator);

        mockFactory.Setup(f => f.Create(It.IsAny<Type>(), It.IsAny<PluginInfo>()))
                   .Returns(mockInitializer.Object);

        var manager = new CompatModManager(mockLoader.Object, mockFactory.Object, mockLogger.Object);

        var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
        mockShip.SetupGet(s => s.Type).Returns((ShipStatus.MapType)6);
        manager.SetUpMap(mockShip.Object);

        // Test RepairAllSabo
        var readerRepairAll = new Mock<MessageReader>();
        readerRepairAll.SetupSequence(r => r.ReadByte())
                       .Returns((byte)IMapMod.RpcCallType)
                       .Returns((byte)MapRpcCall.RepairAllSabo);

        var readerRef1 = readerRepairAll.Object;
        manager.IntegrateModCall(ref readerRef1);

        Assert.True(dummyMapIntegrator.RepairAllSaboCalled);

        // Test RepairCustomSaboType
        var readerRepairType = new Mock<MessageReader>();
        readerRepairType.SetupSequence(r => r.ReadByte())
                        .Returns((byte)IMapMod.RpcCallType)
                        .Returns((byte)MapRpcCall.RepairCustomSaboType);
        readerRepairType.Setup(r => r.ReadInt32()).Returns((int)TaskTypes.FixLights);

        var readerRef2 = readerRepairType.Object;
        manager.IntegrateModCall(ref readerRef2);

        Assert.Equal(TaskTypes.FixLights, dummyMapIntegrator.RepairedSaboTask);
    }

    [Fact]
    public void CreateIntegrateOption_AndOptionCheckers()
    {
        var mockLoader = new Mock<IPluginLoader>();
        var mockFactory = new Mock<IModInitializerFactory>();
        var mockLogger = new Mock<IModLogger>();

        var mockPluginInfo = new PluginInfo();
        mockLoader.Setup(l => l.TryGetPlugin(It.IsAny<string>(), out mockPluginInfo)).Returns(true);

        var mockInitializer = new Mock<IInitializer>();
        mockInitializer.Setup(i => i.Version).Returns(new SemanticVersioning.Version("1.0.0"));
        mockInitializer.Setup(i => i.Name).Returns("TestMod");

        var dummyIntegrator = new DummyMapIntegrator(mockInitializer.Object);
        mockInitializer.Setup(i => i.Initialize()).Returns(dummyIntegrator);

        mockFactory.Setup(f => f.Create(It.IsAny<Type>(), It.IsAny<PluginInfo>()))
                   .Returns(mockInitializer.Object);

        var manager = new CompatModManager(mockLoader.Object, mockFactory.Object, mockLogger.Object);
        manager.CreateIntegrateOption(100);

        Assert.True(dummyIntegrator.CreateOptionCalled);

        Assert.True(manager.IsIntegrateOption(100));
        Assert.False(manager.IsIntegrateOption(99));

        var optionIds = new List<int>(manager.GetIntegrateOptionCategoryId());
        Assert.Equal(manager.LoadedMod.Count, optionIds.Count);
        Assert.Equal(100, optionIds[0]);
    }
}
