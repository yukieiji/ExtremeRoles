using System;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using ExtremeRoles.Compat;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.Core.Abstract;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Compat;

public class ModInitializerFactoryTests
{
    private sealed class DummyInitializer : IInitializer
    {
        public Assembly Dll => throw new NotImplementedException();
        public IHarmonyPatch Patch { get; }
        public BasePlugin Plugin => throw new NotImplementedException();
        public SemanticVersioning.Version Version => throw new NotImplementedException();
        public string Name => throw new NotImplementedException();

        public DummyInitializer(PluginInfo plugin, IHarmonyPatch patch, IAccessTool accessTool)
        {
            Patch = patch;
        }

        public ModIntegratorBase Initialize()
        {
            throw new NotImplementedException();
        }

        public Type GetClass(string name)
        {
            throw new NotImplementedException();
        }

        public MethodInfo GetMethod(string className, string methodName, Type[]? param = null)
        {
            throw new NotImplementedException();
        }

        public MethodInfo GetMethod(Type fromType, string methodName, Type[]? param = null)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class InvalidInitializer
    {
        public InvalidInitializer(PluginInfo plugin, IHarmonyPatch patch, IAccessTool accessTool)
        {
        }
    }

    [Fact]
    public void Create_ValidInitializer_ReturnsInitializerInstance()
    {
        var mockAccessTool = new Mock<IAccessTool>();
        var mockLogger = new Mock<IModLogger>();
        var mockPatchProvider = new Mock<IHarmonyPatchProvider>();
        var mockPatch = new Mock<IHarmonyPatch>();

        var plugin = new PluginInfo();
        mockPatchProvider.Setup(p => p.Get(plugin)).Returns(mockPatch.Object);

        var factory = new ModInitializerFactory(mockAccessTool.Object, mockLogger.Object, mockPatchProvider.Object);

        var initializer = factory.Create(typeof(DummyInitializer), plugin);

        Assert.NotNull(initializer);
        Assert.IsType<DummyInitializer>(initializer);
    }

    [Fact]
    public void Create_NonInitializerType_ReturnsNullAndLogsError()
    {
        var mockAccessTool = new Mock<IAccessTool>();
        var mockLogger = new Mock<IModLogger>();
        var mockPatchProvider = new Mock<IHarmonyPatchProvider>();
        var mockPatch = new Mock<IHarmonyPatch>();

        var plugin = new PluginInfo();
        mockPatchProvider.Setup(p => p.Get(plugin)).Returns(mockPatch.Object);

        var factory = new ModInitializerFactory(mockAccessTool.Object, mockLogger.Object, mockPatchProvider.Object);

        var initializer = factory.Create(typeof(InvalidInitializer), plugin);

        Assert.Null(initializer);
        mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("NOT IMP IInitializer"))), Times.Once);
    }
}

public class BepInExPluginLoaderTests
{
    [Fact]
    public void TryGetPlugin_WhenChainloaderNull_ReturnsFalse()
    {
        var loader = new BepInExPluginLoader();

        bool result = loader.TryGetPlugin("some.guid", out var plugin);

        Assert.False(result);
        Assert.Null(plugin);
    }
}
