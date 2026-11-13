using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;
using ExtremeRoles.Module.RoleAssign.Update;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign.RoleAssignDataBuildBehaviour;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class RoleAssignSubBuildersTests
{
    private readonly Mock<IModLogger> mockLogger = new();

    public RoleAssignSubBuildersTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupExtremeSystemTypeManagerMock();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();

        MockSetupHelper.SetupOptionManager();

        if (ExtremeGameModeManager.Instance == null)
        {
            ExtremeGameModeManager.Create(GameModes.Normal);
        }

        RoleAssignFilter.Instance.Model.FilterSet.Clear();
        RoleAssignFilter.Instance.Initialize();
    }

    [Fact]
    public void ImpostorBuilder_AssignsImpostorRole()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.ImpostorRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes> { RoleTypes.Impostor });

        var helper = new SingleRoleAssignHelper(mockLogger.Object);
        var builder = new ImpostorSingleRoleAssignDataBuilder(mockRoleProvider.Object, helper, mockLogger.Object);

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(2, "ImpPlayer", RoleTypes.Impostor)
        };
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(players);

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);

        var impSpawnDict = new Dictionary<int, SingleRoleSpawnData>
        {
            { (int)ExtremeRoleId.BountyHunter, new SingleRoleSpawnData(1, 100, 10) }
        };

        var singleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
        {
            { ExtremeRoleType.Impostor, impSpawnDict }
        };

        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentSingleRoleSpawnData).Returns(singleSpawnData);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.CanSpawn(It.IsAny<ExtremeRoleType>(), It.IsAny<int>())).Returns(true);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.NotEmpty(playerRoleAssignData.Data);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.PlayerId == 2 && single.RoleId == (int)ExtremeRoleId.BountyHunter);
    }

    [Fact]
    public void NeutralBuilder_AssignsNeutralRoles_WhenLimitGreaterThanZero()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.ImpostorRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });

        var helper = new SingleRoleAssignHelper(mockLogger.Object);
        var builder = new NeutralSingleRoleAssignDataBuilder(mockRoleProvider.Object, helper, mockLogger.Object);

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "NeutralTarget", RoleTypes.Crewmate)
        };
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(players);

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);

        var neutralSpawnDict = new Dictionary<int, SingleRoleSpawnData>
        {
            { (int)ExtremeRoleId.Jester, new SingleRoleSpawnData(1, 100, 10) }
        };
        var singleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
        {
            { ExtremeRoleType.Neutral, neutralSpawnDict }
        };
        var useNumDict = new Dictionary<ExtremeRoleType, int>
        {
            { ExtremeRoleType.Neutral, 1 }
        };

        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentSingleRoleSpawnData).Returns(singleSpawnData);
        mockSpawnData.SetupGet(x => x.CurrentSingleRoleUseNum).Returns(useNumDict);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.CanSpawn(It.IsAny<ExtremeRoleType>(), It.IsAny<int>())).Returns(true);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Neutral)).Returns(1);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.NotEmpty(playerRoleAssignData.Data);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.PlayerId == 1 && single.RoleId == (int)ExtremeRoleId.Jester);
    }

    [Fact]
    public void LiberalBuilder_AssignsLiberalRoles_WhenLiberalLimitIsPositive()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });

        var helper = new SingleRoleAssignHelper(mockLogger.Object);
        var builder = new LiberalSingleRoleAssignDataBuilder(helper, mockLogger.Object);

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "Liberal1", RoleTypes.Crewmate),
            new VanillaRolePlayerAssignData(2, "Liberal2", RoleTypes.Crewmate),
            new VanillaRolePlayerAssignData(3, "Liberal3", RoleTypes.Crewmate)
        };
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(players);

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);

        var singleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
        {
            { ExtremeRoleType.Liberal, new Dictionary<int, SingleRoleSpawnData>() }
        };

        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentSingleRoleSpawnData).Returns(singleSpawnData);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.CanSpawn(ExtremeRoleType.Liberal, It.IsAny<int>())).Returns(true);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Liberal)).Returns(3);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.NotEmpty(playerRoleAssignData.Data);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.RoleId == (int)ExtremeRoleId.Leader);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && (single.RoleId == (int)ExtremeRoleId.Dove || single.RoleId == (int)ExtremeRoleId.Militant));
    }

    [Fact]
    public void LiberalBuilder_AssignsCustomLiberalRoles_BasedOnHasTaskProperty()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });

        var helper = new SingleRoleAssignHelper(mockLogger.Object);
        var builder = new LiberalSingleRoleAssignDataBuilder(helper, mockLogger.Object);

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "Liberal1", RoleTypes.Crewmate),
            new VanillaRolePlayerAssignData(2, "Liberal2", RoleTypes.Crewmate)
        };
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(players);

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);

        var liberalSpawnDict = new Dictionary<int, SingleRoleSpawnData>
        {
            { (int)ExtremeRoleId.Dove, new SingleRoleSpawnData(1, 100, 10) }
        };

        var singleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
        {
            { ExtremeRoleType.Liberal, liberalSpawnDict }
        };

        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentSingleRoleSpawnData).Returns(singleSpawnData);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.CanSpawn(ExtremeRoleType.Liberal, It.IsAny<int>())).Returns(true);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Liberal)).Returns(2);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.Equal(2, playerRoleAssignData.Data.Count);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.RoleId == (int)ExtremeRoleId.Leader);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.RoleId == (int)ExtremeRoleId.Dove);
    }

    [Fact]
    public void LiberalBuilder_FallbackToDefaultRole_WhenCustomRoleBlockedByFilter()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });

        var helper = new SingleRoleAssignHelper(mockLogger.Object);
        var builder = new LiberalSingleRoleAssignDataBuilder(helper, mockLogger.Object);

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "Liberal1", RoleTypes.Crewmate),
            new VanillaRolePlayerAssignData(2, "Liberal2", RoleTypes.Crewmate)
        };
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(players);

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);

        // Register custom Liberal role (Dove) but block it via RoleAssignFilter
        int customRoleId = (int)ExtremeRoleId.Dove;
        RoleAssignFilter.Instance.Update(customRoleId); // Block custom role

        var liberalSpawnDict = new Dictionary<int, SingleRoleSpawnData>
        {
            { customRoleId, new SingleRoleSpawnData(1, 100, 10) }
        };

        var singleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
        {
            { ExtremeRoleType.Liberal, liberalSpawnDict }
        };

        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentSingleRoleSpawnData).Returns(singleSpawnData);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.CanSpawn(ExtremeRoleType.Liberal, It.IsAny<int>())).Returns(true);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Liberal)).Returns(2);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        // Fallback to default Dove (or Leader if not blocked)
        Assert.Equal(2, playerRoleAssignData.Data.Count);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.RoleId == (int)ExtremeRoleId.Leader);
    }

    [Fact]
    public void CrewmateBuilder_AssignsCrewmateRole()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });

        var helper = new SingleRoleAssignHelper(mockLogger.Object);
        var builder = new CrewmateSingleRoleAssignDataBuilder(mockRoleProvider.Object, helper, mockLogger.Object);

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
        mockLimiter.Setup(x => x.CanSpawn(It.IsAny<ExtremeRoleType>(), It.IsAny<int>())).Returns(true);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.NotEmpty(playerRoleAssignData.Data);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.PlayerId == 1 && single.RoleId == (int)ExtremeRoleId.Sheriff);
    }
}
