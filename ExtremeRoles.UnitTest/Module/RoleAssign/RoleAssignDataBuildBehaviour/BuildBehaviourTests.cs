using System.Collections.Generic;
using ExtremeRoles;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign.RoleAssignDataBuildBehaviour;

[Collection("UnityMock")]
public class BuildBehaviourTests
{
    public BuildBehaviourTests()
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
    public void Test_NotAssignedPlayerAssignDataBuilder_Build()
    {
        var builder = new NotAssignedPlayerAssignDataBuilder();
        Assert.Equal((int)ExtremeRoleAssignDataBuilder.Priority.Not, builder.Priority);

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "Player1", AmongUs.GameOptions.RoleTypes.Crewmate)
        };
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(players);

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);
        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimiter = new Mock<ISpawnLimiter>();

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.Single(playerRoleAssignData.Data);
        var assign = (PlayerToSingleRoleAssignData)playerRoleAssignData.Data[0];
        Assert.Equal(1, assign.PlayerId);
        Assert.Equal((int)AmongUs.GameOptions.RoleTypes.Crewmate, assign.RoleId);
    }

    [Fact]
    public void Test_CombinationRoleAssignDataBuilder_PriorityAndBuildWithNoCombRoles()
    {
        var builder = new CombinationRoleAssignDataBuilder();
        Assert.Equal((int)ExtremeRoleAssignDataBuilder.Priority.Combination, builder.Priority);

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());

        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(new List<VanillaRolePlayerAssignData>());

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);
        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentCombRoleSpawnData)
            .Returns(new Dictionary<byte, CombinationRoleSpawnData>());

        var mockLimiter = new Mock<ISpawnLimiter>();

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.Empty(playerRoleAssignData.Data);
    }

    [Fact]
    public void Test_SingleRoleAssignDataBuilder_PriorityAndBuildWithEmptyTarget()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());
        mockRoleProvider.SetupGet(x => x.ImpostorRole).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<AmongUs.GameOptions.RoleTypes>());

        var builder = new SingleRoleAssignDataBuilder(mockRoleProvider.Object);
        Assert.Equal((int)ExtremeRoleAssignDataBuilder.Priority.Single, builder.Priority);

        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(new List<VanillaRolePlayerAssignData>());

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);
        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentSingleRoleSpawnData)
            .Returns(new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>());

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.Get(It.IsAny<ExtremeRoleType>())).Returns(0);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.Empty(playerRoleAssignData.Data);
    }
}
