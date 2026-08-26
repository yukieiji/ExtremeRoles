#nullable enable

using AmongUs.GameOptions;
using ExtremeRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign.RoleAssignDataBuildBehaviour;

[Collection("UnityMock")]
public class NotAssignedPlayerAssignDataBuilderTests
{
    public NotAssignedPlayerAssignDataBuilderTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        if (!OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)OptionCreator.CommonOption.RandomOption, out _))
        {
            OptionCreator.Create();
            OptionManager.Load();
        }
    }

    [Fact]
    public void Priority_ReturnsNotPriority()
    {
        var builder = new NotAssignedPlayerAssignDataBuilder();
        Assert.Equal((int)ExtremeRoleAssignDataBuilder.Priority.Not, builder.Priority);
    }

    [Fact]
    public void Build_AssignsRemainingPlayersToVanillaRoles()
    {
        var mockProvider = new Mock<IVanillaRoleProvider>();
        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();

        var p1 = new VanillaRolePlayerAssignData(1, "Player1", RoleTypes.Crewmate);
        var p2 = new VanillaRolePlayerAssignData(2, "Player2", RoleTypes.Impostor);

        mockDataProvider.SetupGet(d => d.Data).Returns(new[] { p1, p2 });

        var assignData = new PlayerRoleAssignData(mockProvider.Object, mockDataProvider.Object);
        var mockSpawnData = new Mock<ISpawnDataManager>();
        var mockLimit = new Mock<ISpawnLimiter>();
        var prepData = new PreparationData(assignData, mockSpawnData.Object, mockLimit.Object);

        var builder = new NotAssignedPlayerAssignDataBuilder();
        builder.Build(in prepData);

        Assert.Equal(2, assignData.Data.Count);
        Assert.Contains(assignData.Data, d => d.PlayerId == 1 && d.RoleId == (int)RoleTypes.Crewmate);
        Assert.Contains(assignData.Data, d => d.PlayerId == 2 && d.RoleId == (int)RoleTypes.Impostor);
    }
}
