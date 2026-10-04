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

        MockSetupHelper.SetupOptionManager();

        if (ExtremeGameModeManager.Instance == null)
        {
            ExtremeGameModeManager.Create(GameModes.Normal);
        }

        RoleAssignFilter.Instance.Model.FilterSet.Clear();
        RoleAssignFilter.Instance.Initialize();
    }

    [Fact]
    public void Build_CallsAllSubBuildersInOrder()
    {
        var mockImp = new Mock<IImpostorSingleRoleAssignDataBuilder>();
        var mockNeutral = new Mock<INeutralSingleRoleAssignDataBuilder>();
        var mockLiberal = new Mock<ILiberalSingleRoleAssignDataBuilder>();
        var mockCrew = new Mock<ICrewmateSingleRoleAssignDataBuilder>();
        var mockLogger = new Mock<IModLogger>();

        var builder = new SingleRoleAssignDataBuilder(
            mockImp.Object,
            mockNeutral.Object,
            mockLiberal.Object,
            mockCrew.Object,
            mockLogger.Object);

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        var mockAssignData = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockAssignData.SetupGet(x => x.Data).Returns(new List<VanillaRolePlayerAssignData>());

        var playerRoleAssignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockAssignData.Object);
        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimiter = new Mock<ISpawnLimiter>();

        var prepData = new PreparationData(playerRoleAssignData, mockSpawnData.Object, mockLimiter.Object);

        int callOrder = 0;
        int impOrder = 0, neutralOrder = 0, liberalOrder = 0, crewOrder = 0;

        mockImp.Setup(x => x.Build(It.Ref<PreparationData>.IsAny)).Callback(() => impOrder = ++callOrder);
        mockNeutral.Setup(x => x.Build(It.Ref<PreparationData>.IsAny)).Callback(() => neutralOrder = ++callOrder);
        mockLiberal.Setup(x => x.Build(It.Ref<PreparationData>.IsAny)).Callback(() => liberalOrder = ++callOrder);
        mockCrew.Setup(x => x.Build(It.Ref<PreparationData>.IsAny)).Callback(() => crewOrder = ++callOrder);

        builder.Build(prepData);

        Assert.Equal(1, impOrder);
        Assert.Equal(2, neutralOrder);
        Assert.Equal(3, liberalOrder);
        Assert.Equal(4, crewOrder);
    }
}
