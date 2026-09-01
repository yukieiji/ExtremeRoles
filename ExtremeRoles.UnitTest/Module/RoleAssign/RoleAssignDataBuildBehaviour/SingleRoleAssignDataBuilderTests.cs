using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
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
public class SingleRoleAssignDataBuilderTests
{
    public SingleRoleAssignDataBuilderTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupExtremeSystemTypeManagerMock();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();

        if (ClientOption.Instance == null || !OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)OptionCreator.CommonOption.RandomOption, out _))
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
    public void Priority_ReturnsSinglePriority()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        var builder = new SingleRoleAssignDataBuilder(mockRoleProvider.Object);

        Assert.Equal((int)ExtremeRoleAssignDataBuilder.Priority.Single, builder.Priority);
    }

    [Fact]
    public void Build_AssignsCrewmateAndImpostorRoles()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.ImpostorRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes> { RoleTypes.Impostor });

        var builder = new SingleRoleAssignDataBuilder(mockRoleProvider.Object);

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "CrewPlayer", RoleTypes.Crewmate),
            new VanillaRolePlayerAssignData(2, "ImpPlayer", RoleTypes.Impostor)
        };
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(players);

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);

        var crewSpawnDict = new Dictionary<int, SingleRoleSpawnData>
        {
            { (int)ExtremeRoleId.Sheriff, new SingleRoleSpawnData(1, 100, 10) }
        };
        var impSpawnDict = new Dictionary<int, SingleRoleSpawnData>
        {
            { (int)ExtremeRoleId.BountyHunter, new SingleRoleSpawnData(1, 100, 10) }
        };

        var singleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
        {
            { ExtremeRoleType.Crewmate, crewSpawnDict },
            { ExtremeRoleType.Impostor, impSpawnDict }
        };

        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentSingleRoleSpawnData).Returns(singleSpawnData);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.CanSpawn(It.IsAny<ExtremeRoleType>(), It.IsAny<int>())).Returns(true);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Neutral)).Returns(0);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Liberal)).Returns(0);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.NotEmpty(playerRoleAssignData.Data);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.PlayerId == 1 && single.RoleId == (int)ExtremeRoleId.Sheriff);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.PlayerId == 2 && single.RoleId == (int)ExtremeRoleId.BountyHunter);
    }

    [Fact]
    public void Build_AssignsNeutralRoles_WhenLimitGreaterThanZero()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.ImpostorRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes>());

        var builder = new SingleRoleAssignDataBuilder(mockRoleProvider.Object);

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
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Liberal)).Returns(0);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.NotEmpty(playerRoleAssignData.Data);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.PlayerId == 1 && single.RoleId == (int)ExtremeRoleId.Jester);
    }

    [Fact]
    public void Build_AssignsLiberalRoles_WhenLiberalLimitIsPositiveAndLeaderNotBlocked()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.ImpostorRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes>());

        var builder = new SingleRoleAssignDataBuilder(mockRoleProvider.Object);

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
            { ExtremeRoleType.Liberal, new Dictionary<int, SingleRoleSpawnData>() },
            { ExtremeRoleType.Crewmate, new Dictionary<int, SingleRoleSpawnData>() },
            { ExtremeRoleType.Impostor, new Dictionary<int, SingleRoleSpawnData>() }
        };

        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentSingleRoleSpawnData).Returns(singleSpawnData);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.CanSpawn(ExtremeRoleType.Liberal, It.IsAny<int>())).Returns(true);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Neutral)).Returns(0);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Liberal)).Returns(3);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.NotEmpty(playerRoleAssignData.Data);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.RoleId == (int)ExtremeRoleId.Leader);
    }

    [Fact]
    public void Build_SkipsLiberal_WhenLiberalLimitIsZero()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.ImpostorRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes>());

        var builder = new SingleRoleAssignDataBuilder(mockRoleProvider.Object);

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "Player1", RoleTypes.Crewmate)
        };
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(players);

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);
        var singleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
        {
            { ExtremeRoleType.Crewmate, new Dictionary<int, SingleRoleSpawnData>() },
            { ExtremeRoleType.Impostor, new Dictionary<int, SingleRoleSpawnData>() }
        };
        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentSingleRoleSpawnData).Returns(singleSpawnData);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Neutral)).Returns(0);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Liberal)).Returns(0);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.DoesNotContain(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.RoleId == (int)ExtremeRoleId.Leader);
    }

    [Fact]
    public void Build_SkipsNeutral_WhenNeutralLimitIsZero()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.ImpostorRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes>());

        var builder = new SingleRoleAssignDataBuilder(mockRoleProvider.Object);

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "Player1", RoleTypes.Crewmate)
        };
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(players);

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);
        var singleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
        {
            { ExtremeRoleType.Crewmate, new Dictionary<int, SingleRoleSpawnData>() },
            { ExtremeRoleType.Impostor, new Dictionary<int, SingleRoleSpawnData>() }
        };
        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentSingleRoleSpawnData).Returns(singleSpawnData);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Neutral)).Returns(0);
        mockLimiter.Setup(x => x.Get(ExtremeRoleType.Liberal)).Returns(0);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.DoesNotContain(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.RoleId == (int)ExtremeRoleId.Jester);
    }

    [Fact]
    public void Build_SkipsBlockedRole_WhenRoleIsBlockedInFilter()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.CrewmateRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.ImpostorRole).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes>());

        var builder = new SingleRoleAssignDataBuilder(mockRoleProvider.Object);

        var filterGuid = Guid.NewGuid();
        RoleAssignFilterModelUpdater.AddFilter(RoleAssignFilter.Instance.Model, filterGuid);
        RoleAssignFilterModelUpdater.AddRoleData(RoleAssignFilter.Instance.Model, filterGuid, 1, ExtremeRoleId.Sheriff);
        RoleAssignFilter.Instance.Initialize();
        RoleAssignFilter.Instance.Update((int)ExtremeRoleId.Sheriff);

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

        Assert.DoesNotContain(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.RoleId == (int)ExtremeRoleId.Sheriff);
    }
}
