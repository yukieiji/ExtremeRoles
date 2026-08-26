using System;
using System.Collections.Generic;
using ExtremeRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class ExtremeRoleAssignDataBuilderTests
{
    public ExtremeRoleAssignDataBuilderTests()
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
    public void Test_ExtremeRoleAssignDataBuilder_Build()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());

        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(new List<VanillaRolePlayerAssignData>());

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);
        var mockSpawnDataManager = new Mock<ISpawnDataManager>();
        mockSpawnDataManager.SetupGet(x => x.UseGhostCombRole).Returns(new List<(CombinationRoleType, GhostAndAliveCombinationRoleManagerBase)>());

        var mockSpawnLimiter = new Mock<ISpawnLimiter>();

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnDataManager.Object, mockSpawnLimiter.Object);

        var mockPreparer = new Mock<IRoleAssignDataPreparer>();
        mockPreparer.Setup(x => x.Prepare()).Returns(prepData);

        var mockFilterInitializer = new Mock<IAssignFilterInitializer>();
        var mockValidator = new Mock<IRoleAssignValidator>();
        mockValidator.Setup(x => x.IsReBuild(It.IsAny<PreparationData>())).Returns(false);

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IEnumerable<IRoleAssignDataBuildBehaviour>)))
            .Returns(new List<IRoleAssignDataBuildBehaviour>());

        var builder = new ExtremeRoleAssignDataBuilder(
            mockServiceProvider.Object,
            mockPreparer.Object,
            mockFilterInitializer.Object,
            mockValidator.Object);

        var result = builder.Build();

        Assert.NotNull(result);
    }
}
