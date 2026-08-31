using System.Collections.Generic;
using ExtremeRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;


[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class RoleAssignValidatorTests
{
    public RoleAssignValidatorTests()
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
    public void Test_IsReBuild_NoCheckersOrNoNgData_ReturnsFalse()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var validator = new RoleAssignValidator(serviceProvider);

        var mockAssign = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssign.SetupGet(x => x.Data).Returns(new List<VanillaRolePlayerAssignData>());
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());

        var playerRoleAssign = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssign.Object);
        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimiter = new Mock<ISpawnLimiter>();

        var prepData = new PreparationData(playerRoleAssign, mockSpawnData.Object, mockLimiter.Object);

        bool result = validator.IsReBuild(prepData);

        Assert.False(result);
    }

    [Fact]
    public void Test_IsReBuild_WhenCheckerFindsNgData_RemovesNgAssignmentAndReturnsTrue()
    {
        var mockChecker = new Mock<IRoleAssignDataChecker>();
        mockChecker
            .Setup(x => x.GetNgData(It.Ref<PreparationData>.IsAny))
            .Returns(new HashSet<ExtremeRoleId> { ExtremeRoleId.Sheriff });

        var services = new ServiceCollection();
        services.AddSingleton<IRoleAssignDataChecker>(mockChecker.Object);
        var serviceProvider = services.BuildServiceProvider();

        var validator = new RoleAssignValidator(serviceProvider);

        var mockAssign = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssign.SetupGet(x => x.Data).Returns(new List<VanillaRolePlayerAssignData>());
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());

        var playerRoleAssign = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssign.Object);
        playerRoleAssign.AddAssignData(new PlayerToSingleRoleAssignData(1, (int)ExtremeRoleId.Sheriff, 100));

        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimiter = new Mock<ISpawnLimiter>();

        var prepData = new PreparationData(playerRoleAssign, mockSpawnData.Object, mockLimiter.Object);

        bool result = validator.IsReBuild(prepData);

        Assert.True(result);
        Assert.Empty(playerRoleAssign.Data);
    }
}
