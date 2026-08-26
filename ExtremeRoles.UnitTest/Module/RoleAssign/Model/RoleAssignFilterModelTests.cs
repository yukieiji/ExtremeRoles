#nullable enable

using System;
using BepInEx.Configuration;
using ExtremeRoles.Module.RoleAssign.Model;

using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign.Model;

[Collection("UnityMock")]
public class RoleAssignFilterModelTests
{
    private readonly ConfigEntry<string> testConfig;

    public RoleAssignFilterModelTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid()}.cfg");
        var configFile = new ConfigFile(tempFile, true);
        this.testConfig = configFile.Bind("TestFilter", "Key", "");
    }

    [Fact]
    public void Properties_DefaultValues()
    {
        var model = new RoleAssignFilterModel(testConfig);

        Assert.Same(testConfig, model.Config);
        Assert.NotNull(model.FilterSet);
        Assert.Empty(model.FilterSet);
        Assert.NotNull(model.Id);
        Assert.NotNull(model.NormalRole);
        Assert.NotNull(model.CombRole);
        Assert.NotNull(model.GhostRole);
    }

    [Fact]
    public void SerializeToString_And_DeserializeFromString()
    {
        var model = new RoleAssignFilterModel(testConfig);
        var filterData = new RoleFilterData
        {
            AssignNum = 2,
            FilterNormalId = new System.Collections.Generic.Dictionary<int, ExtremeRoles.Roles.ExtremeRoleId>(),
            FilterCombinationId = new System.Collections.Generic.Dictionary<int, ExtremeRoles.Roles.CombinationRoleType>(),
            FilterGhostRole = new System.Collections.Generic.Dictionary<int, ExtremeRoles.GhostRoles.ExtremeGhostRoleId>()
        };

        var guid = Guid.NewGuid();
        model.FilterSet.Add(guid, filterData);

        var serialized = model.SerializeToString();
        Assert.StartsWith("v1|", serialized);

        var newModel = new RoleAssignFilterModel(testConfig);
        newModel.DeserializeFromString(serialized);

        Assert.Single(newModel.FilterSet);
    }
}
