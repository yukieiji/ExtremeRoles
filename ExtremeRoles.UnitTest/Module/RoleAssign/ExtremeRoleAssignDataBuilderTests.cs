using ExtremeRoles.UnitTest.Mocks;
using System;
using System.Collections.Generic;
using ExtremeRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

public class ExtremeRoleAssignDataBuilderTests : SerialTestBase, IClassFixture<UnityCommonMock>
{
    public ExtremeRoleAssignDataBuilderTests(SerialFixture fixture, UnityCommonMock unityCommonMock)
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
    public void Test_ExtremeRoleAssignDataBuilder_Build_ExecutesBehavioursAndReturnsAssignedData()
    {
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

        var mockBehaviour = new Mock<IRoleAssignDataBuildBehaviour>();
        mockBehaviour.SetupGet(x => x.Priority).Returns((int)ExtremeRoleAssignDataBuilder.Priority.Single);
        mockBehaviour.Setup(x => x.Build(It.Ref<PreparationData>.IsAny))
            .Callback((in PreparationData data) =>
            {
                data.Assign.AddAssignData(new PlayerToSingleRoleAssignData(1, (int)ExtremeRoleId.Sheriff, data.Assign.ControlId));
            });

        var services = new ServiceCollection();
        services.AddSingleton(mockBehaviour.Object);
        var serviceProvider = services.BuildServiceProvider();

        var builder = new ExtremeRoleAssignDataBuilder(
            serviceProvider,
            mockPreparer.Object,
            mockFilterInitializer.Object,
            mockValidator.Object);

        var result = builder.Build();

        Assert.Single(result);
        Assert.IsType<PlayerToSingleRoleAssignData>(result[0]);
        var single = (PlayerToSingleRoleAssignData)result[0];
        Assert.Equal((byte)1, single.PlayerId);
        Assert.Equal((int)ExtremeRoleId.Sheriff, single.RoleId);
        Assert.Equal(0, single.ControlId);

        mockFilterInitializer.Verify(x => x.Initialize(It.IsAny<RoleAssignFilter>(), It.IsAny<PreparationData>()), Times.Once);
        mockBehaviour.Verify(x => x.Build(It.Ref<PreparationData>.IsAny), Times.Once);
    }
}