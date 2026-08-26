#nullable enable

using System.Collections.Generic;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class RoleAssignValidatorTests
{
    public RoleAssignValidatorTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
    }

    [Fact]
    public void IsReBuild_NoCheckers_ReturnsFalse()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        var validator = new RoleAssignValidator(provider);

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();
        var assignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockDataProvider.Object);

        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimit = new Mock<ISpawnLimiter>();
        var prepData = new PreparationData(assignData, mockSpawnData.Object, mockLimit.Object);

        bool isRebuild = validator.IsReBuild(prepData);

        Assert.False(isRebuild);
    }

    [Fact]
    public void IsReBuild_CheckerReturnsNoNgData_ReturnsFalse()
    {
        var mockChecker = new Mock<IRoleAssignDataChecker>();

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();
        var assignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockDataProvider.Object);

        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimit = new Mock<ISpawnLimiter>();
        var prepData = new PreparationData(assignData, mockSpawnData.Object, mockLimit.Object);

        mockChecker.Setup(c => c.GetNgData(prepData)).Returns(new HashSet<ExtremeRoleId>());

        var services = new ServiceCollection();
        services.AddSingleton(mockChecker.Object);
        var provider = services.BuildServiceProvider();

        var validator = new RoleAssignValidator(provider);
        bool isRebuild = validator.IsReBuild(prepData);

        Assert.False(isRebuild);
    }
}
