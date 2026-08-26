using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.RoleAssign;
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
    public void Test_VanillaRolePlayerAssignDataProviderSelector_WhenMockOptionSet_ResolvesMockProvider()
    {
        var mockOption = new VanillaRolePlayerOption { MockOption = new VanillaRolePlayerMockOption(2) };
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(x => x.GetService(typeof(MockVanillaRolePlayerAssignDataProvider)))
            .Throws(new InvalidOperationException("ResolvedMockProvider"));

        var selector = new VanillaRolePlayerAssignDataProviderSelector(mockOption, mockServiceProvider.Object);

        var ex = Assert.Throws<InvalidOperationException>(() => _ = selector.Data);
        Assert.Equal("ResolvedMockProvider", ex.Message);
        mockServiceProvider.Verify(x => x.GetService(typeof(MockVanillaRolePlayerAssignDataProvider)), Times.Once);
        mockServiceProvider.Verify(x => x.GetService(typeof(DefaultVanillaRolePlayerAssignDataProvider)), Times.Never);
    }

    [Fact]
    public void Test_VanillaRolePlayerAssignDataProviderSelector_WhenMockOptionNull_ResolvesDefaultProvider()
    {
        var mockOption = new VanillaRolePlayerOption { MockOption = null };
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(x => x.GetService(typeof(DefaultVanillaRolePlayerAssignDataProvider)))
            .Throws(new InvalidOperationException("ResolvedDefaultProvider"));

        var selector = new VanillaRolePlayerAssignDataProviderSelector(mockOption, mockServiceProvider.Object);

        var ex = Assert.Throws<InvalidOperationException>(() => _ = selector.Data);
        Assert.Equal("ResolvedDefaultProvider", ex.Message);
        mockServiceProvider.Verify(x => x.GetService(typeof(DefaultVanillaRolePlayerAssignDataProvider)), Times.Once);
        mockServiceProvider.Verify(x => x.GetService(typeof(MockVanillaRolePlayerAssignDataProvider)), Times.Never);
    }
}
