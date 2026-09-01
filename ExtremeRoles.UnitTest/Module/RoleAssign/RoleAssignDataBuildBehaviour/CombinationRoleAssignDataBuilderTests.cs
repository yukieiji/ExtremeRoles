using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;
using ExtremeRoles.Module.RoleAssign.Update;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign.RoleAssignDataBuildBehaviour;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class CombinationRoleAssignDataBuilderTests
{
    public CombinationRoleAssignDataBuilderTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupExtremeSystemTypeManagerMock();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        MockSetupHelper.SetupGameDataMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();

        var mockOptions = new Mock<IGameOptions>(System.IntPtr.Zero);
        mockOptions.SetupGet(o => o.NumImpostors).Returns(2);
        mockOptions.Setup(o => o.GetInt(Int32OptionNames.NumImpostors)).Returns(2);

        var mockOptionsMgr = new Mock<GameOptionsManager>(System.IntPtr.Zero);
        mockOptionsMgr.SetupGet(m => m.currentGameOptions).Returns(mockOptions.Object);
        mockOptionsMgr.SetupGet(m => m.CurrentGameOptions).Returns(mockOptions.Object);

        var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
        mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockOptionsMgr.Object);
        MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;

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
        PlayerCache.RemovePlayerControl(_ => true);
    }

    private static PlayerControl CreateMockPlayerControl(byte playerId, RoleTypes roleType = RoleTypes.Crewmate)
    {
        var mockPc = new Mock<PlayerControl>(System.IntPtr.Zero);
        mockPc.SetupGet(p => p.PlayerId).Returns(playerId);

        var mockData = new Mock<NetworkedPlayerInfo>(System.IntPtr.Zero);
        mockPc.SetupGet(p => p.Data).Returns(mockData.Object);

        var mockRole = new Mock<RoleBehaviour>(System.IntPtr.Zero);
        mockRole.SetupGet(r => r.Role).Returns(roleType);
        mockData.SetupGet(d => d.Role).Returns(mockRole.Object);

        return mockPc.Object;
    }

    [Fact]
    public void Priority_ReturnsCombinationPriority()
    {
        var builder = new CombinationRoleAssignDataBuilder();

        Assert.Equal((int)ExtremeRoleAssignDataBuilder.Priority.Combination, builder.Priority);
    }

    [Fact]
    public void Build_WithEmptyCombRoles_DoesNotAssign()
    {
        var builder = new CombinationRoleAssignDataBuilder();

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

    [Fact]
    public void Build_AssignsMultipleCombinationRolesToMultiplePlayers()
    {
        PlayerCache.RemovePlayerControl(_ => true);
        var pc1 = CreateMockPlayerControl(1, RoleTypes.Crewmate);
        var pc2 = CreateMockPlayerControl(2, RoleTypes.Crewmate);
        PlayerCache.AddPlayerControl(pc1);
        PlayerCache.AddPlayerControl(pc2);

        var builder = new CombinationRoleAssignDataBuilder();

        byte combType = (byte)CombinationRoleType.Lover;

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes>());

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "Player1", RoleTypes.Crewmate),
            new VanillaRolePlayerAssignData(2, "Player2", RoleTypes.Crewmate)
        };
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(players);

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);

        var combManager = ExtremeRoleManager.CombRole[combType];
        combManager.Initialize();

        var combSpawnData = new Dictionary<byte, CombinationRoleSpawnData>
        {
            { combType, new CombinationRoleSpawnData(combManager, 1, 100, 10, false) }
        };

        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentCombRoleSpawnData).Returns(combSpawnData);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.CanSpawn(It.IsAny<ExtremeRoleType>(), It.IsAny<int>())).Returns(true);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.True(playerRoleAssignData.Data.Count >= 2);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToCombRoleAssignData comb && comb.PlayerId == 1 && comb.CombTypeId == combType);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToCombRoleAssignData comb && comb.PlayerId == 2 && comb.CombTypeId == combType);
    }

    [Fact]
    public void Build_WhenCombRoleBlockedByFilter_DoesNotAssign()
    {
        var builder = new CombinationRoleAssignDataBuilder();

        byte combType = (byte)CombinationRoleType.Lover;
        var filterGuid = Guid.NewGuid();
        RoleAssignFilterModelUpdater.AddFilter(RoleAssignFilter.Instance.Model, filterGuid);
        RoleAssignFilterModelUpdater.AddRoleData(RoleAssignFilter.Instance.Model, filterGuid, 1, CombinationRoleType.Lover);
        RoleAssignFilter.Instance.Initialize();
        RoleAssignFilter.Instance.Update(combType);

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes>());

        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "Player1", RoleTypes.Crewmate)
        });

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);

        var combManager = ExtremeRoleManager.CombRole[combType];
        var combSpawnData = new Dictionary<byte, CombinationRoleSpawnData>
        {
            { combType, new CombinationRoleSpawnData(combManager, 1, 100, 10, false) }
        };

        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentCombRoleSpawnData).Returns(combSpawnData);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.CanSpawn(It.IsAny<ExtremeRoleType>(), It.IsAny<int>())).Returns(true);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.Empty(playerRoleAssignData.Data);
    }

    [Fact]
    public void Build_WhenLimiterCannotSpawn_DoesNotAssign()
    {
        var builder = new CombinationRoleAssignDataBuilder();

        byte combType = (byte)CombinationRoleType.Lover;

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes>());

        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "Player1", RoleTypes.Crewmate),
            new VanillaRolePlayerAssignData(2, "Player2", RoleTypes.Crewmate)
        });

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);

        var combManager = ExtremeRoleManager.CombRole[combType];
        var combSpawnData = new Dictionary<byte, CombinationRoleSpawnData>
        {
            { combType, new CombinationRoleSpawnData(combManager, 1, 100, 10, false) }
        };

        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(x => x.CurrentCombRoleSpawnData).Returns(combSpawnData);

        var mockLimiter = new Mock<ISpawnLimiter>();
        mockLimiter.Setup(x => x.CanSpawn(It.IsAny<ExtremeRoleType>(), It.IsAny<int>())).Returns(false);

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.Empty(playerRoleAssignData.Data);
    }
}
