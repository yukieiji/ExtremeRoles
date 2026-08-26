using System;
using BepInEx.Configuration;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign.Model;

[Collection("UnityMock")]
public class RoleAssignFilterModelTests
{
    public RoleAssignFilterModelTests()
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
    public void Test_RoleFilterData_Properties()
    {
        var data = new RoleFilterData
        {
            AssignNum = 2,
            FilterNormalId = new System.Collections.Generic.Dictionary<int, ExtremeRoleId>(),
            FilterCombinationId = new System.Collections.Generic.Dictionary<int, CombinationRoleType>(),
            FilterGhostRole = new System.Collections.Generic.Dictionary<int, ExtremeGhostRoleId>()
        };

        Assert.Equal(2, data.AssignNum);
        Assert.Empty(data.FilterNormalId);
        Assert.Empty(data.FilterCombinationId);
        Assert.Empty(data.FilterGhostRole);
    }

    [Fact]
    public void Test_RoleAssignFilterModel_InitializeAndSerialize()
    {
        string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
        var config = new ConfigFile(tempPath, true);
        var entry = config.Bind("Test", "Filter", "");

        var model = new RoleAssignFilterModel(entry);
        model.Initialize();

        Assert.Contains(0, model.Id);
        Assert.Equal(ExtremeRoleId.Leader, model.NormalRole[0]);

        string serialized = model.SerializeToString();
        Assert.NotNull(serialized);
        Assert.StartsWith("v1", serialized);

        model.DeserializeFromString(serialized);
    }
}
