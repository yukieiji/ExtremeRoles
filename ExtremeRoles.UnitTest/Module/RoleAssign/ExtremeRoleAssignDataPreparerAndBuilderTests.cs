using System;
using ExtremeRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class ExtremeRoleAssignDataPreparerAndBuilderTests
{
    public ExtremeRoleAssignDataPreparerAndBuilderTests()
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
    public void Test_ExtremeRoleAssginDataPreparer_Prepare()
    {
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(new System.Collections.Generic.List<VanillaRolePlayerAssignData>());
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new System.Collections.Generic.HashSet<AmongUs.GameOptions.RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new System.Collections.Generic.HashSet<AmongUs.GameOptions.RoleTypes>());

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);
        var mockSpawnDataManager = new Mock<ISpawnDataManager>();
        var mockSpawnLimiter = new Mock<ISpawnLimiter>();

        var services = new ServiceCollection();
        services.AddSingleton(playerRoleAssignData);
        services.AddSingleton(mockSpawnDataManager.Object);
        services.AddSingleton(mockSpawnLimiter.Object);
        var serviceProvider = services.BuildServiceProvider();

        var preparer = new ExtremeRoleAssginDataPreparer(serviceProvider);
        var prepData = preparer.Prepare();

        Assert.NotNull(prepData.Assign);
        Assert.NotNull(prepData.RoleSpawn);
        Assert.NotNull(prepData.Limit);
    }

    [Fact]
    public void Test_AssignFilterInitializer_Initialize()
    {
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(new System.Collections.Generic.List<VanillaRolePlayerAssignData>());
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new System.Collections.Generic.HashSet<AmongUs.GameOptions.RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new System.Collections.Generic.HashSet<AmongUs.GameOptions.RoleTypes>());

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);
        var singleAssign = new PlayerToSingleRoleAssignData(1, 10, 100);
        playerRoleAssignData.AddAssignData(singleAssign);

        var mockSpawnDataManager = new Mock<ISpawnDataManager>();
        var mockSpawnLimiter = new Mock<ISpawnLimiter>();

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnDataManager.Object, mockSpawnLimiter.Object);

        var initializer = new AssignFilterInitializer();
        initializer.Initialize(RoleAssignFilter.Instance, prepData);
    }
}
