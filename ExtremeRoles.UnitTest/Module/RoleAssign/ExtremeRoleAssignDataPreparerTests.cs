#nullable enable

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

public class ExtremeRoleAssignDataPreparerTests
{
    [Fact]
    public void Prepare_ResolvesRequiredServicesAndReturnsPreparationData()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();
        var assignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockDataProvider.Object);

        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimit = new Mock<ISpawnLimiter>();

        var services = new ServiceCollection();
        services.AddSingleton(assignData);
        services.AddSingleton(mockSpawnData.Object);
        services.AddSingleton(mockLimit.Object);
        var provider = services.BuildServiceProvider();

        var preparer = new ExtremeRoleAssginDataPreparer(provider);
        var result = preparer.Prepare();

        Assert.Same(assignData, result.Assign);
        Assert.Same(mockSpawnData.Object, result.RoleSpawn);
        Assert.Same(mockLimit.Object, result.Limit);
    }
}
