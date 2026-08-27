using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.RoleAssign;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class VanillaRolePlayerAssignDataProviderSelectorTests
{
    public VanillaRolePlayerAssignDataProviderSelectorTests()
    {
        MockSetupHelper.SetupCommonMocks();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();

        if (ClientOption.Instance == null)
        {
            OptionCreator.Create();
        }
    }

    [Fact]
    public void Test_MockVanillaRolePlayerAssignDataProvider_NullMockOption_ThrowsArgumentNullException()
    {
        var option = new VanillaRolePlayerOption();
        Assert.Throws<ArgumentNullException>(() => new MockVanillaRolePlayerAssignDataProvider(option));
    }

    [Fact]
    public void Test_VanillaRolePlayerAssignDataProviderSelector_WhenMockOptionSet_ReturnsMockProviderData()
    {
        var mockProviderInstance = (MockVanillaRolePlayerAssignDataProvider)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(MockVanillaRolePlayerAssignDataProvider));
        var expectedData = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "MockPlayer", RoleTypes.Crewmate)
        };

        typeof(MockVanillaRolePlayerAssignDataProvider)
            .GetField("<Data>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?
            .SetValue(mockProviderInstance, expectedData);

        var mockOption = new VanillaRolePlayerOption { MockOption = new VanillaRolePlayerMockOption(2) };
        var services = new ServiceCollection();
        services.AddSingleton(mockProviderInstance);
        var serviceProvider = services.BuildServiceProvider();

        var selector = new VanillaRolePlayerAssignDataProviderSelector(mockOption, serviceProvider);

        Assert.Same(expectedData, selector.Data);
    }

    [Fact]
    public void Test_VanillaRolePlayerAssignDataProviderSelector_WhenMockOptionNull_ResolvesDefaultProviderFromServices()
    {
        bool defaultProviderRequested = false;
        var defaultProviderInstance = (DefaultVanillaRolePlayerAssignDataProvider)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(DefaultVanillaRolePlayerAssignDataProvider));

        var mockOption = new VanillaRolePlayerOption { MockOption = null };
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(x => x.GetService(typeof(DefaultVanillaRolePlayerAssignDataProvider)))
            .Callback(() => defaultProviderRequested = true)
            .Returns(defaultProviderInstance);

        var selector = new VanillaRolePlayerAssignDataProviderSelector(mockOption, mockServiceProvider.Object);

        // Accessing Data attempts to call DefaultVanillaRolePlayerAssignDataProvider.Data which throws due to unmanaged GameAssembly
        Assert.Throws<TypeInitializationException>(() => _ = selector.Data);
        Assert.True(defaultProviderRequested);
        mockServiceProvider.Verify(x => x.GetService(typeof(DefaultVanillaRolePlayerAssignDataProvider)), Times.Once);
        mockServiceProvider.Verify(x => x.GetService(typeof(MockVanillaRolePlayerAssignDataProvider)), Times.Never);
    }
}
