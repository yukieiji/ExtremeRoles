using ExtremeRoles.UnitTest.Mocks;
using System;
using BepInEx.Configuration;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Module.RoleAssign.Update;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign.Update;

public class RoleAssignFilterModelUpdaterTests : SerialTestBase, IClassFixture<UnityCommonMock>
{
    public RoleAssignFilterModelUpdaterTests(SerialFixture fixture, UnityCommonMock unityCommonMock)
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
    public void Test_RoleAssignFilterModelUpdater_AddRoleData_And_FilterBlockingLogic()
    {
        string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
        var config = new ConfigFile(tempPath, true);
        var entry = config.Bind("Test", "Filter", "");

        var model = RoleAssignFilter.Instance.Model;
        model.FilterSet.Clear();

        var filterGuid = Guid.NewGuid();
        RoleAssignFilterModelUpdater.AddFilter(model, filterGuid);
        Assert.True(model.FilterSet.ContainsKey(filterGuid));

        bool addedNormal = RoleAssignFilterModelUpdater.AddRoleData(model, filterGuid, 1, ExtremeRoleId.Sheriff);
        bool addedComb = RoleAssignFilterModelUpdater.AddRoleData(model, filterGuid, 2, CombinationRoleType.Lover);
        bool addedGhost = RoleAssignFilterModelUpdater.AddRoleData(model, filterGuid, 3, ExtremeGhostRoleId.Wisp);

        Assert.True(addedNormal);
        Assert.True(addedComb);
        Assert.True(addedGhost);

        RoleAssignFilter.Instance.Initialize();

        // Before update: not blocked
        Assert.False(RoleAssignFilter.Instance.IsBlock((int)ExtremeRoleId.Sheriff));
        Assert.False(RoleAssignFilter.Instance.IsBlock((byte)CombinationRoleType.Lover));
        Assert.False(RoleAssignFilter.Instance.IsBlock(ExtremeGhostRoleId.Wisp));

        // Update assignment count for Sheriff (AssignNum is 1) -> now blocked
        RoleAssignFilter.Instance.Update((int)ExtremeRoleId.Sheriff);
        Assert.True(RoleAssignFilter.Instance.IsBlock((int)ExtremeRoleId.Sheriff));

        // Update assignment count for Lover -> now blocked
        RoleAssignFilter.Instance.Update((byte)CombinationRoleType.Lover);
        Assert.True(RoleAssignFilter.Instance.IsBlock((byte)CombinationRoleType.Lover));

        // Increase AssignNum to 2 via updater
        RoleAssignFilterModelUpdater.IncreaseFilterAssignNum(model, filterGuid);
        Assert.Equal(2, model.FilterSet[filterGuid].AssignNum);

        // Remove filter role data
        RoleAssignFilterModelUpdater.RemoveFilterRole(model, filterGuid, 1);
        Assert.False(model.FilterSet[filterGuid].FilterNormalId.ContainsKey(1));

        // Reset filter
        RoleAssignFilterModelUpdater.ResetFilter(model, filterGuid);
        Assert.Empty(model.FilterSet[filterGuid].FilterNormalId);

        // Remove filter
        RoleAssignFilterModelUpdater.RemoveFilter(model, filterGuid);
        Assert.False(model.FilterSet.ContainsKey(filterGuid));
    }
}