#nullable enable

using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles.API;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class PlayerRoleAssignDataTests
{
    public PlayerRoleAssignDataTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        if (!OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, ExtremeRoles.Module.CustomOption.Implemented.PresetOption.CategoryId, out _))
        {
            OptionCreator.Create();
        }
    }

    [Fact]
    public void ControlId_IncrementsSequentially()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockDataProvider.SetupGet(d => d.Data).Returns(System.Array.Empty<VanillaRolePlayerAssignData>());

        var assignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockDataProvider.Object);

        Assert.Equal(0, assignData.ControlId);
        Assert.Equal(1, assignData.ControlId);
        Assert.Equal(2, assignData.ControlId);
    }

    [Fact]
    public void GetCanCrewmateAssignPlayer_And_GetCanImpostorAssignPlayer_FilterCorrectly()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        mockRoleProvider.SetupGet(p => p.AllCrewmate).Returns(new HashSet<RoleTypes> { RoleTypes.Crewmate });
        mockRoleProvider.SetupGet(p => p.AllImpostor).Returns(new HashSet<RoleTypes> { RoleTypes.Impostor });

        var pCrew = new VanillaRolePlayerAssignData(1, "Crew", RoleTypes.Crewmate);
        var pImp = new VanillaRolePlayerAssignData(2, "Imp", RoleTypes.Impostor);

        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockDataProvider.SetupGet(d => d.Data).Returns(new[] { pCrew, pImp });

        var assignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockDataProvider.Object);

        var crewPlayers = assignData.GetCanCrewmateAssignPlayer();
        var impPlayers = assignData.GetCanImpostorAssignPlayer();

        Assert.Single(crewPlayers);
        Assert.Equal(1, crewPlayers[0].PlayerId);

        Assert.Single(impPlayers);
        Assert.Equal(2, impPlayers[0].PlayerId);
    }

    [Fact]
    public void TryAddCombRoleAssignData_And_TryGetCombRoleAssign()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockDataProvider.SetupGet(d => d.Data).Returns(System.Array.Empty<VanillaRolePlayerAssignData>());

        var assignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockDataProvider.Object);

        var combData = new PlayerToCombRoleAssignData(1, 100, 1, 0, 0);

        bool added = assignData.TryAddCombRoleAssignData(combData, ExtremeRoleType.Crewmate);
        Assert.True(added);

        bool secondAdd = assignData.TryAddCombRoleAssignData(combData, ExtremeRoleType.Crewmate);
        Assert.False(secondAdd);

        bool found = assignData.TryGetCombRoleAssign(1, out var team);
        Assert.True(found);
        Assert.Equal(ExtremeRoleType.Crewmate, team);
    }

    [Fact]
    public void TryRemoveAssignment_SingleRoleAssignment()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockDataProvider.SetupGet(d => d.Data).Returns(System.Array.Empty<VanillaRolePlayerAssignData>());

        var assignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockDataProvider.Object);

        var singleData = new PlayerToSingleRoleAssignData(1, 100, 0);
        assignData.AddAssignData(singleData);

        Assert.Single(assignData.Data);

        bool removed = assignData.TryRemoveAssignment(1, 100);
        Assert.True(removed);
        Assert.Empty(assignData.Data);
    }

    [Fact]
    public void AddPlayerToReassign_NullPlayer_DoesNothing()
    {
        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockDataProvider.SetupGet(d => d.Data).Returns(System.Array.Empty<VanillaRolePlayerAssignData>());

        var assignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockDataProvider.Object);

        assignData.AddPlayerToReassign(null!);
        Assert.Empty(assignData.NeedRoleAssignPlayer);
    }
}
