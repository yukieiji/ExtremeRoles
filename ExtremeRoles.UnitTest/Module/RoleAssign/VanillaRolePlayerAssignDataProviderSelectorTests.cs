#nullable enable

using System.Runtime.CompilerServices;
using ExtremeRoles.Module.RoleAssign;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class VanillaRolePlayerAssignDataProviderSelectorTests
{
    public VanillaRolePlayerAssignDataProviderSelectorTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
    }

    [Fact]
    public void Data_WithMockOption_ResolvesMockProviderFromServiceProvider()
    {
        var mockOption = new VanillaRolePlayerMockOption(1);
        var option = new VanillaRolePlayerOption { MockOption = mockOption };

        var mockInstance = (MockVanillaRolePlayerAssignDataProvider)RuntimeHelpers.GetUninitializedObject(typeof(MockVanillaRolePlayerAssignDataProvider));

        var services = new ServiceCollection();
        services.AddSingleton(mockInstance);

        var provider = services.BuildServiceProvider();

        var selector = new VanillaRolePlayerAssignDataProviderSelector(option, provider);

        var resolvedInstance = provider.GetService<MockVanillaRolePlayerAssignDataProvider>();
        Assert.Same(mockInstance, resolvedInstance);
    }

    [Fact]
    public void Data_WithoutMockOption_ResolvesDefaultProviderFromServiceProvider()
    {
        var option = new VanillaRolePlayerOption { MockOption = null };

        var defaultInstance = (DefaultVanillaRolePlayerAssignDataProvider)RuntimeHelpers.GetUninitializedObject(typeof(DefaultVanillaRolePlayerAssignDataProvider));

        var services = new ServiceCollection();
        services.AddSingleton(defaultInstance);

        var provider = services.BuildServiceProvider();

        var selector = new VanillaRolePlayerAssignDataProviderSelector(option, provider);

        var resolvedInstance = provider.GetService<DefaultVanillaRolePlayerAssignDataProvider>();
        Assert.Same(defaultInstance, resolvedInstance);
    }
}
