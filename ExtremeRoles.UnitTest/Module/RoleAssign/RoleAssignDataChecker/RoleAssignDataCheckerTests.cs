using ExtremeRoles.UnitTest.Mocks;
using System;
using System.Collections.Generic;
using ExtremeRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign.RoleAssignDataChecker;

public class RoleAssignDataCheckerTests : SerialTestBase, IClassFixture<UnityCommonMock>
{
    public RoleAssignDataCheckerTests(SerialFixture fixture, UnityCommonMock unityCommonMock)
        : base(fixture, unityCommonMock.OperatorsMock, unityCommonMock.Vector2Mock, unityCommonMock.ColorMock, unityCommonMock.MathfMock, new PaletteMock(), new GameOptionsManagerMock(), new CompatModManagerMock(), unityCommonMock.TimeMock, new LoggerMock())
    {
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        MockSetupHelper.SetupDebugMode();

        if (ClientOption.Instance == null)
        {
            OptionCreator.Create();
        }
    }

    [Fact]
    public void Test_RoleAssignDependencyChecker_GetNgData_NoAssignments_ReturnsEmpty()
    {
        var mockFactory = new Mock<IRoleDependencyRuleFactory>();
        mockFactory.SetupGet(x => x.Rules).Returns(new List<RoleDependencyRule>
        {
            new RoleDependencyRule(ExtremeRoleId.Furry, ExtremeRoleId.Jackal, () => true)
        });

        var checker = new RoleAssignDependencyChecker(mockFactory.Object);

        var prepData = createPreparationData();

        var ngData = checker.GetNgData(prepData);

        Assert.Empty(ngData);
    }

    [Fact]
    public void Test_RoleAssignDependencyChecker_GetNgData_CheckRoleAssignedButDependRoleMissing_ReturnsCheckRole()
    {
        var mockFactory = new Mock<IRoleDependencyRuleFactory>();
        mockFactory.SetupGet(x => x.Rules).Returns(new List<RoleDependencyRule>
        {
            new RoleDependencyRule(ExtremeRoleId.Furry, ExtremeRoleId.Jackal, () => true)
        });

        var checker = new RoleAssignDependencyChecker(mockFactory.Object);

        var prepData = createPreparationData();
        prepData.Assign.AddAssignData(new PlayerToSingleRoleAssignData(1, (int)ExtremeRoleId.Furry, 1));

        var ngData = checker.GetNgData(prepData);

        Assert.Single(ngData);
        Assert.Contains(ExtremeRoleId.Furry, ngData);
    }

    [Fact]
    public void Test_RoleAssignDependencyChecker_GetNgData_BothCheckRoleAndDependRoleAssigned_ReturnsEmpty()
    {
        var mockFactory = new Mock<IRoleDependencyRuleFactory>();
        mockFactory.SetupGet(x => x.Rules).Returns(new List<RoleDependencyRule>
        {
            new RoleDependencyRule(ExtremeRoleId.Furry, ExtremeRoleId.Jackal, () => true)
        });

        var checker = new RoleAssignDependencyChecker(mockFactory.Object);

        var prepData = createPreparationData();
        prepData.Assign.AddAssignData(new PlayerToSingleRoleAssignData(1, (int)ExtremeRoleId.Furry, 1));
        prepData.Assign.AddAssignData(new PlayerToSingleRoleAssignData(2, (int)ExtremeRoleId.Jackal, 2));

        var ngData = checker.GetNgData(prepData);

        Assert.Empty(ngData);
    }

    [Fact]
    public void Test_RoleAssignDependencyChecker_GetNgData_WhenIsDependReturnsFalse_ReturnsEmpty()
    {
        var mockFactory = new Mock<IRoleDependencyRuleFactory>();
        mockFactory.SetupGet(x => x.Rules).Returns(new List<RoleDependencyRule>
        {
            new RoleDependencyRule(ExtremeRoleId.Furry, ExtremeRoleId.Jackal, () => false)
        });

        var checker = new RoleAssignDependencyChecker(mockFactory.Object);

        var prepData = createPreparationData();
        prepData.Assign.AddAssignData(new PlayerToSingleRoleAssignData(1, (int)ExtremeRoleId.Furry, 1));

        var ngData = checker.GetNgData(prepData);

        Assert.Empty(ngData);
    }

    private static PreparationData createPreparationData()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());

        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(new List<VanillaRolePlayerAssignData>());

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);
        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimiter = new Mock<ISpawnLimiter>();

        return new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);
    }
}