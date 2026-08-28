using System.Collections.Generic;
using AmongUs.GameOptions;
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

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class BuildBehaviourTests
{
    public BuildBehaviourTests()
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

        if (ExtremeGameModeManager.Instance == null)
        {
            ExtremeGameModeManager.Create(GameModes.Normal);
        }

        RoleAssignFilter.Instance.Model.FilterSet.Clear();
        RoleAssignFilter.Instance.Initialize();
    }

    [Fact]
    public void Test_NotAssignedPlayerAssignDataBuilder_AssignsVanillaRoleToUnassignedPlayer()
    {
        var builder = new NotAssignedPlayerAssignDataBuilder();
        Assert.Equal((int)ExtremeRoleAssignDataBuilder.Priority.Not, builder.Priority);

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes>());

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "Player1", RoleTypes.Crewmate)
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
        Assert.Equal((byte)1, assign.PlayerId);
        Assert.Equal((int)RoleTypes.Crewmate, assign.RoleId);
    }

    [Fact]
    public void Test_SingleRoleAssignDataBuilder_AssignsCrewmateVanillaRoleWhenInCrewmateSet()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.ImpostorRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes>());

        var builder = new SingleRoleAssignDataBuilder(mockRoleProvider.Object);

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "CrewPlayer", RoleTypes.Crewmate)
        };
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(players);

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);

        var crewSpawnDict = new Dictionary<int, SingleRoleSpawnData>
        {
            { (int)ExtremeRoleId.Sheriff, new SingleRoleSpawnData(1, 100, 10) }
        };
        var singleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
        {
            { ExtremeRoleType.Crewmate, crewSpawnDict }
        };

        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentSingleRoleSpawnData).Returns(singleSpawnData);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.CanSpawn(ExtremeRoleType.Crewmate, It.IsAny<int>())).Returns(true);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Neutral)).Returns(0);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Liberal)).Returns(0);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.NotEmpty(playerRoleAssignData.Data);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.PlayerId == 1 && single.RoleId == (int)RoleTypes.Crewmate);
    }

    [Fact]
    public void Test_SingleRoleAssignDataBuilder_AssignsExtremeRoleIdWhenNotInVanillaSet()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.ImpostorRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes>());

        var builder = new SingleRoleAssignDataBuilder(mockRoleProvider.Object);

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "CrewPlayer", RoleTypes.Crewmate)
        };
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(players);

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);

        var crewSpawnDict = new Dictionary<int, SingleRoleSpawnData>
        {
            { (int)ExtremeRoleId.Sheriff, new SingleRoleSpawnData(1, 100, 10) }
        };
        var singleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
        {
            { ExtremeRoleType.Crewmate, crewSpawnDict }
        };

        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentSingleRoleSpawnData).Returns(singleSpawnData);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.CanSpawn(ExtremeRoleType.Crewmate, It.IsAny<int>())).Returns(true);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Neutral)).Returns(0);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Liberal)).Returns(0);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.NotEmpty(playerRoleAssignData.Data);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.PlayerId == 1 && single.RoleId == (int)ExtremeRoleId.Sheriff);
    }

    [Fact]
    public void Test_CombinationRoleAssignDataBuilder_BuildWithEmptyCombRoles_DoesNotAssign()
    {
        var builder = new CombinationRoleAssignDataBuilder();
        Assert.Equal((int)ExtremeRoleAssignDataBuilder.Priority.Combination, builder.Priority);

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes>());

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
}
