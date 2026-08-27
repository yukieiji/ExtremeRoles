using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using ExtremeRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
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
    public void Test_VanillaRolePlayerAssignDataProviderSelector_WhenMockOptionSet_QueriesMockProviderFromServiceProviderAndReturnsData()
    {
        var mockProviderInstance = (MockVanillaRolePlayerAssignDataProvider)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(MockVanillaRolePlayerAssignDataProvider));
        var expectedData = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "Player1", RoleTypes.Crewmate)
        };
        typeof(MockVanillaRolePlayerAssignDataProvider)
            .GetField("<Data>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?
            .SetValue(mockProviderInstance, expectedData);

        var services = new ServiceCollection();
        services.AddSingleton(mockProviderInstance);
        var serviceProvider = services.BuildServiceProvider();

        var mockOption = new VanillaRolePlayerOption { MockOption = new VanillaRolePlayerMockOption(2) };
        var selector = new VanillaRolePlayerAssignDataProviderSelector(mockOption, serviceProvider);

        Assert.Same(expectedData, selector.Data);
    }

    [Fact]
    public void Test_VanillaRolePlayerAssignDataProviderSelector_WhenMockOptionNull_QueriesDefaultProviderFromServiceProvider()
    {
        bool defaultProviderQueried = false;
        var defaultProviderInstance = (DefaultVanillaRolePlayerAssignDataProvider)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(DefaultVanillaRolePlayerAssignDataProvider));

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(x => x.GetService(typeof(DefaultVanillaRolePlayerAssignDataProvider)))
            .Callback(() => defaultProviderQueried = true)
            .Returns(defaultProviderInstance);

        var mockOptionNull = new VanillaRolePlayerOption { MockOption = null };
        var selector = new VanillaRolePlayerAssignDataProviderSelector(mockOptionNull, mockServiceProvider.Object);

        // Accessing Data queries DefaultVanillaRolePlayerAssignDataProvider from serviceProvider
        _ = Assert.Throws<TypeInitializationException>(() => selector.Data);
        Assert.True(defaultProviderQueried);
        mockServiceProvider.Verify(x => x.GetService(typeof(DefaultVanillaRolePlayerAssignDataProvider)), Times.Once);
        mockServiceProvider.Verify(x => x.GetService(typeof(MockVanillaRolePlayerAssignDataProvider)), Times.Never);
    }
}
