using System;
using System.Collections.Generic;
using BepInEx;
using ExtremeRoles.Compat;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.Core.Abstract;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Compat;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class CompatModManagerTests
{
    private sealed class DummyIntegrator : ModIntegratorBase
    {
        public DummyIntegrator(IInitializer init) : base(init)
        {
        }
    }

    public CompatModManagerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupCompatModManager();
    }

    [Fact]
    public void Manager_Initialization_LoadsModsFromLoader()
    {
        var mockLoader = new Mock<IPluginLoader>();
        var mockFactory = new Mock<IModInitializerFactory>();
        var mockLogger = new Mock<IModLogger>();

        var manager = new CompatModManager(mockLoader.Object, mockFactory.Object, mockLogger.Object);

        Assert.Empty(manager.LoadedMod);
    }

    [Fact]
    public void IsIntegrateOption_ReturnsCorrectBoolean()
    {
        var mockLoader = new Mock<IPluginLoader>();
        var mockFactory = new Mock<IModInitializerFactory>();
        var mockLogger = new Mock<IModLogger>();

        var mockPluginInfo = new PluginInfo();
        mockLoader.Setup(l => l.TryGetPlugin(It.IsAny<string>(), out mockPluginInfo)).Returns(true);

        var mockInitializer = new Mock<IInitializer>();
        mockInitializer.Setup(i => i.Version).Returns(new SemanticVersioning.Version("1.0.0"));
        mockInitializer.Setup(i => i.Name).Returns("TestMod");

        var dummyIntegrator = new DummyIntegrator(mockInitializer.Object);
        mockInitializer.Setup(i => i.Initialize()).Returns(dummyIntegrator);

        mockFactory.Setup(f => f.Create(It.IsAny<Type>(), It.IsAny<PluginInfo>()))
                   .Returns(mockInitializer.Object);

        var manager = new CompatModManager(mockLoader.Object, mockFactory.Object, mockLogger.Object);
        manager.CreateIntegrateOption(100);

        Assert.True(manager.IsIntegrateOption(100));
        Assert.False(manager.IsIntegrateOption(99));
        Assert.False(manager.IsIntegrateOption(100 + manager.LoadedMod.Count));
    }

    [Fact]
    public void GetIntegrateOptionCategoryId_ReturnsExpectedSequence()
    {
        var mockLoader = new Mock<IPluginLoader>();
        var mockFactory = new Mock<IModInitializerFactory>();
        var mockLogger = new Mock<IModLogger>();

        var mockPluginInfo = new PluginInfo();
        mockLoader.Setup(l => l.TryGetPlugin(It.IsAny<string>(), out mockPluginInfo)).Returns(true);

        var mockInitializer = new Mock<IInitializer>();
        mockInitializer.Setup(i => i.Version).Returns(new SemanticVersioning.Version("1.0.0"));
        mockInitializer.Setup(i => i.Name).Returns("TestMod");

        var dummyIntegrator = new DummyIntegrator(mockInitializer.Object);
        mockInitializer.Setup(i => i.Initialize()).Returns(dummyIntegrator);

        mockFactory.Setup(f => f.Create(It.IsAny<Type>(), It.IsAny<PluginInfo>()))
                   .Returns(mockInitializer.Object);

        var manager = new CompatModManager(mockLoader.Object, mockFactory.Object, mockLogger.Object);
        manager.CreateIntegrateOption(200);

        var optionIds = new List<int>(manager.GetIntegrateOptionCategoryId());
        Assert.Equal(manager.LoadedMod.Count, optionIds.Count);
        Assert.Equal(200, optionIds[0]);
    }

    [Fact]
    public void TryGetMod_WhenNotLoaded_ReturnsFalse()
    {
        var mockLoader = new Mock<IPluginLoader>();
        var mockFactory = new Mock<IModInitializerFactory>();
        var mockLogger = new Mock<IModLogger>();

        var manager = new CompatModManager(mockLoader.Object, mockFactory.Object, mockLogger.Object);

        var result = manager.TryGetMod<CrowdedMod>(CompatModType.CrowdedMod, out var mod);

        Assert.False(result);
        Assert.Null(mod);
    }

    [Fact]
    public void TryGetModMap_WhenNoMap_ReturnsFalse()
    {
        var mockLoader = new Mock<IPluginLoader>();
        var mockFactory = new Mock<IModInitializerFactory>();
        var mockLogger = new Mock<IModLogger>();

        var manager = new CompatModManager(mockLoader.Object, mockFactory.Object, mockLogger.Object);

        var result = manager.TryGetModMap(out var mapMod);

        Assert.False(result);
        Assert.Null(mapMod);
    }

    [Fact]
    public void TryGetModMapGeneric_WhenNoMap_ReturnsFalse()
    {
        var mockLoader = new Mock<IPluginLoader>();
        var mockFactory = new Mock<IModInitializerFactory>();
        var mockLogger = new Mock<IModLogger>();

        var manager = new CompatModManager(mockLoader.Object, mockFactory.Object, mockLogger.Object);

        var result = manager.TryGetModMap<SubmergedIntegrator>(out var submergedMap);

        Assert.False(result);
        Assert.Null(submergedMap);
    }

    [Fact]
    public void IsModMap_WhenNoMap_ReturnsFalse()
    {
        var mockLoader = new Mock<IPluginLoader>();
        var mockFactory = new Mock<IModInitializerFactory>();
        var mockLogger = new Mock<IModLogger>();

        var manager = new CompatModManager(mockLoader.Object, mockFactory.Object, mockLogger.Object);

        Assert.False(manager.IsModMap<SubmergedIntegrator>());
    }
}
