using System;
using System.Collections.Generic;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.RoleAssign;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class RoleAndGhostSpawnDataManagerTests
{
    public RoleAndGhostSpawnDataManagerTests()
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
    public void Test_ExtremeSpawnLimiter_CanSpawnAndReduce()
    {
        var limiter = new ExtremeSpawnLimiter();

        limiter.Reduce(ExtremeRoleType.Crewmate, 1);
        string str = limiter.ToString();
        Assert.NotNull(str);
        Assert.Contains("Spawn Limit", str);
    }

    [Fact]
    public void Test_GhostRoleSpawnDataManager_Methods()
    {
        var ghostManager = GhostRoleSpawnDataManager.Instance;
        Assert.NotNull(ghostManager);

        ghostManager.Create(new List<(CombinationRoleType, GhostAndAliveCombinationRoleManagerBase)>());

        Assert.Equal(0, ghostManager.GetGlobalSpawnLimit(ExtremeRoleType.Crewmate));
        Assert.True(ghostManager.IsGlobalSpawnLimit(ExtremeRoleType.Crewmate));
        Assert.Null(ghostManager.GetUseGhostRole(ExtremeRoleType.Crewmate));
        Assert.False(ghostManager.IsCombRole(ExtremeRoleId.Sheriff));
    }

    [Fact]
    public void Test_RoleSpawnDataManager_Initialization()
    {
        var manager = new RoleSpawnDataManager();

        Assert.NotNull(manager.CurrentSingleRoleSpawnData);
        Assert.NotNull(manager.CurrentCombRoleSpawnData);
        Assert.NotNull(manager.UseGhostCombRole);
        Assert.NotNull(manager.CurrentSingleRoleUseNum);

        string str = manager.ToString();
        Assert.NotNull(str);
        Assert.Contains("RoleSpawnInfo", str);
    }

    [Fact]
    public void Test_MockVanillaRolePlayerAssignDataProvider_NullMockOption_Throws()
    {
        var option = new VanillaRolePlayerOption();
        Assert.Throws<ArgumentNullException>(() => new MockVanillaRolePlayerAssignDataProvider(option));
    }

    [Fact]
    public void Test_VanillaRolePlayerAssignDataProviderSelector_SelectsMockOrDefault()
    {
        var mockOption = new VanillaRolePlayerOption { MockOption = null };
        var mockDefaultProvider = (DefaultVanillaRolePlayerAssignDataProvider)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(DefaultVanillaRolePlayerAssignDataProvider));

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(x => x.GetService(typeof(DefaultVanillaRolePlayerAssignDataProvider)))
            .Returns(mockDefaultProvider);

        var selector = new VanillaRolePlayerAssignDataProviderSelector(mockOption, mockServiceProvider.Object);
        Assert.NotNull(selector);

        var mockOption2 = new VanillaRolePlayerOption { MockOption = new VanillaRolePlayerMockOption(5) };
        var mockMockProvider = (MockVanillaRolePlayerAssignDataProvider)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(MockVanillaRolePlayerAssignDataProvider));

        mockServiceProvider
            .Setup(x => x.GetService(typeof(MockVanillaRolePlayerAssignDataProvider)))
            .Returns(mockMockProvider);

        var selector2 = new VanillaRolePlayerAssignDataProviderSelector(mockOption2, mockServiceProvider.Object);
        Assert.NotNull(selector2);
    }
}
