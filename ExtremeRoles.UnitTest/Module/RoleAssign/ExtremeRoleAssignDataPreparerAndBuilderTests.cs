using System;
using System.Collections.Generic;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.Update;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class ExtremeRoleAssignDataPreparerAndBuilderTests
{
    public ExtremeRoleAssignDataPreparerAndBuilderTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
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
        mockAssignData.SetupGet(x => x.Data).Returns(new List<VanillaRolePlayerAssignData>());
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());

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
    public void Test_AssignFilterInitializer_Initialize_ReappliesAssignmentsToFilterState()
    {
        var filter = RoleAssignFilter.Instance;
        filter.Model.FilterSet.Clear();

        var filterGuid = Guid.NewGuid();
        RoleAssignFilterModelUpdater.AddFilter(filter.Model, filterGuid);
        RoleAssignFilterModelUpdater.AddRoleData(filter.Model, filterGuid, 1, ExtremeRoleId.Sheriff);
        RoleAssignFilterModelUpdater.AddRoleData(filter.Model, filterGuid, 2, CombinationRoleType.Lover);

        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(new List<VanillaRolePlayerAssignData>());
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);
        playerRoleAssignData.AddAssignData(new PlayerToSingleRoleAssignData(1, (int)ExtremeRoleId.Sheriff, 100));
        var combData = new PlayerToCombRoleAssignData(2, (int)ExtremeRoleId.Sheriff, (byte)CombinationRoleType.Lover, 101, 0);
        playerRoleAssignData.TryAddCombRoleAssignData(combData, ExtremeRoleType.Crewmate);

        var mockSpawnDataManager = new Mock<ISpawnDataManager>();
        var mockSpawnLimiter = new Mock<ISpawnLimiter>();

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnDataManager.Object, mockSpawnLimiter.Object);

        var initializer = new AssignFilterInitializer();
        initializer.Initialize(filter, prepData);

        Assert.True(filter.IsBlock((int)ExtremeRoleId.Sheriff));
        Assert.True(filter.IsBlock((byte)CombinationRoleType.Lover));
    }
}
