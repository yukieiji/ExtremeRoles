#nullable enable

using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataChecker;
using ExtremeRoles.Roles;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign.RoleAssignDataChecker;

[Collection("UnityMock")]
public class RoleAssignDependencyCheckerTests
{
    public RoleAssignDependencyCheckerTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        var debugModeProp = typeof(ExtremeRolesPlugin).GetProperty("DebugMode", BindingFlags.Public | BindingFlags.Static);
        if (debugModeProp != null && debugModeProp.GetValue(null) == null)
        {
            var entry = plugin.Config.Bind("DeBug", "DebugMode", false);
            debugModeProp.SetValue(null, entry);
        }

        if (!OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, PresetOption.CategoryId, out _))
        {
            OptionCreator.Create();
        }
    }

    [Fact]
    public void GetNgData_NoRules_ReturnsEmptySet()
    {
        var mockFactory = new Mock<IRoleDependencyRuleFactory>();
        mockFactory.SetupGet(f => f.Rules).Returns(System.Array.Empty<RoleDependencyRule>());

        var mockProvider = new Mock<IVanillaRoleProvider>();
        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();
        var assignData = new PlayerRoleAssignData(mockProvider.Object, mockDataProvider.Object);

        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimit = new Mock<ISpawnLimiter>();
        var prepData = new PreparationData(assignData, mockSpawnData.Object, mockLimit.Object);

        var checker = new RoleAssignDependencyChecker(mockFactory.Object);
        var ngData = checker.GetNgData(in prepData);

        Assert.Empty(ngData);
    }

    [Fact]
    public void GetNgData_DependentRoleAssignedWithoutMaster_ReturnsNgRoleId()
    {
        var rule = new RoleDependencyRule(
            ExtremeRoleId.Furry,
            ExtremeRoleId.Jackal,
            () => true);

        var mockFactory = new Mock<IRoleDependencyRuleFactory>();
        mockFactory.SetupGet(f => f.Rules).Returns(new[] { rule });

        var mockProvider = new Mock<IVanillaRoleProvider>();
        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();
        var assignData = new PlayerRoleAssignData(mockProvider.Object, mockDataProvider.Object);

        // Assign Furry (checkRole) without assigning Jackal (dependRole)
        assignData.AddAssignData(new PlayerToSingleRoleAssignData(1, (int)ExtremeRoleId.Furry, 0));

        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimit = new Mock<ISpawnLimiter>();
        var prepData = new PreparationData(assignData, mockSpawnData.Object, mockLimit.Object);

        var checker = new RoleAssignDependencyChecker(mockFactory.Object);
        var ngData = checker.GetNgData(in prepData);

        Assert.Single(ngData);
        Assert.Contains(ExtremeRoleId.Furry, ngData);
    }

    [Fact]
    public void GetNgData_DependentRoleAssignedWithMaster_ReturnsEmpty()
    {
        var rule = new RoleDependencyRule(
            ExtremeRoleId.Furry,
            ExtremeRoleId.Jackal,
            () => true);

        var mockFactory = new Mock<IRoleDependencyRuleFactory>();
        mockFactory.SetupGet(f => f.Rules).Returns(new[] { rule });

        var mockProvider = new Mock<IVanillaRoleProvider>();
        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();
        var assignData = new PlayerRoleAssignData(mockProvider.Object, mockDataProvider.Object);

        // Assign both Furry (checkRole) and Jackal (dependRole)
        assignData.AddAssignData(new PlayerToSingleRoleAssignData(1, (int)ExtremeRoleId.Furry, 0));
        assignData.AddAssignData(new PlayerToSingleRoleAssignData(2, (int)ExtremeRoleId.Jackal, 1));

        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimit = new Mock<ISpawnLimiter>();
        var prepData = new PreparationData(assignData, mockSpawnData.Object, mockLimit.Object);

        var checker = new RoleAssignDependencyChecker(mockFactory.Object);
        var ngData = checker.GetNgData(in prepData);

        Assert.Empty(ngData);
    }
}
