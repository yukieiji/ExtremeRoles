using System.Collections.Generic;
using AmongUs.GameOptions;
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
public class NotAssignedPlayerAssignDataBuilderTests
{
    public NotAssignedPlayerAssignDataBuilderTests()
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
    public void Build_AssignsVanillaRoleToUnassignedPlayers()
    {
        var builder = new NotAssignedPlayerAssignDataBuilder();

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes> { RoleTypes.Impostor });

        var players = new List<VanillaRolePlayerAssignData>
        {
            new VanillaRolePlayerAssignData(1, "Player1", RoleTypes.Crewmate),
            new VanillaRolePlayerAssignData(2, "Player2", RoleTypes.Impostor)
        };
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(players);

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);
        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimiter = new Mock<ISpawnLimiter>();

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.Equal(2, playerRoleAssignData.Data.Count);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.PlayerId == 1 && single.RoleId == (int)RoleTypes.Crewmate);
        Assert.Contains(playerRoleAssignData.Data, a => a is PlayerToSingleRoleAssignData single && single.PlayerId == 2 && single.RoleId == (int)RoleTypes.Impostor);
    }

    [Fact]
    public void Build_WithNoNeedAssignPlayer_AddsNoAssignData()
    {
        var builder = new NotAssignedPlayerAssignDataBuilder();

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(x => x.AllCrewmate).Returns(new HashSet<RoleTypes>());
        mockRoleProvider.SetupGet(x => x.AllImpostor).Returns(new HashSet<RoleTypes>());

        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(new List<VanillaRolePlayerAssignData>());

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);
        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimiter = new Mock<ISpawnLimiter>();

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        builder.Build(prepData);

        Assert.Empty(playerRoleAssignData.Data);
    }
}
