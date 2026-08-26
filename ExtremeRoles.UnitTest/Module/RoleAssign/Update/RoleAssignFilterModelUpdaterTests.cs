using System;
using BepInEx.Configuration;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Module.RoleAssign.Update;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign.Update;

[Collection("UnityMock")]
public class RoleAssignFilterModelUpdaterTests
{
    public RoleAssignFilterModelUpdaterTests()
    {
        MockSetupHelper.SetupCommonMocks();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();

        if (ClientOption.Instance == null)
        {
            OptionCreator.Create();
        }
    }

    [Fact]
    public void Test_RoleAssignFilterModelUpdater_AddAndRemoveFilterAndRoleData()
    {
        string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
        var config = new ConfigFile(tempPath, true);
        var entry = config.Bind("Test", "Filter", "");

        var model = new RoleAssignFilterModel(entry);
        var filterGuid = Guid.NewGuid();

        RoleAssignFilterModelUpdater.AddFilter(model, filterGuid);
        Assert.True(model.FilterSet.ContainsKey(filterGuid));

        bool addedNormal = RoleAssignFilterModelUpdater.AddRoleData(model, filterGuid, 1, ExtremeRoleId.Sheriff);
        Assert.True(addedNormal);

        bool addedComb = RoleAssignFilterModelUpdater.AddRoleData(model, filterGuid, 2, CombinationRoleType.Lover);
        Assert.True(addedComb);

        bool addedGhost = RoleAssignFilterModelUpdater.AddRoleData(model, filterGuid, 3, ExtremeGhostRoleId.Wisp);
        Assert.True(addedGhost);

        RoleAssignFilterModelUpdater.IncreaseFilterAssignNum(model, filterGuid);
        Assert.Equal(2, model.FilterSet[filterGuid].AssignNum);

        RoleAssignFilterModelUpdater.DecreaseFilterAssignNum(model, filterGuid);
        Assert.Equal(1, model.FilterSet[filterGuid].AssignNum);

        RoleAssignFilterModelUpdater.RemoveFilterRole(model, filterGuid, 1);
        Assert.False(model.FilterSet[filterGuid].FilterNormalId.ContainsKey(1));

        RoleAssignFilterModelUpdater.ResetFilter(model, filterGuid);
        Assert.Empty(model.FilterSet[filterGuid].FilterNormalId);

        RoleAssignFilterModelUpdater.RemoveFilter(model, filterGuid);
        Assert.False(model.FilterSet.ContainsKey(filterGuid));
    }
}
