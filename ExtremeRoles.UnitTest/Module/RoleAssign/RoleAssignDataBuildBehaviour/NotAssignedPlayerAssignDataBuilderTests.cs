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
    public void Priority_ReturnsNotPriority()
    {
        var builder = new NotAssignedPlayerAssignDataBuilder();

        int priority = builder.Priority;

        Assert.Equal((int)ExtremeRoleAssignDataBuilder.Priority.Not, priority);
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

        var assign1 = playerRoleAssignData.Data[0];
        Assert.True(assign1 is PlayerToSingleRoleAssignData);
        var singleAssign1 = (PlayerToSingleRoleAssignData)assign1;
        Assert.Equal((byte)1, singleAssign1.PlayerId);
        Assert.Equal((int)RoleTypes.Crewmate, singleAssign1.RoleId);

        var assign2 = playerRoleAssignData.Data[1];
        Assert.True(assign2 is PlayerToSingleRoleAssignData);
        var singleAssign2 = (PlayerToSingleRoleAssignData)assign2;
        Assert.Equal((byte)2, singleAssign2.PlayerId);
        Assert.Equal((int)RoleTypes.Impostor, singleAssign2.RoleId);
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
