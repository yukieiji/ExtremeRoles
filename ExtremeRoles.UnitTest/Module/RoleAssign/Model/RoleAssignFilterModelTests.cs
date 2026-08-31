using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using BepInEx.Configuration;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign.Model;


[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class RoleAssignFilterModelTests
{
    public RoleAssignFilterModelTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
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
    public void Test_RoleAssignFilterModel_Initialize_AddsDefaultLiberalRoles()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var config = new ConfigFile(tempPath, true);
        var entry = config.Bind("Test", "Filter", "");

        var model = new RoleAssignFilterModel(entry);
        model.Initialize();

        Assert.Contains(0, model.Id);
        Assert.Equal(ExtremeRoleId.Leader, model.NormalRole[0]);
        Assert.Contains(1, model.Id);
        Assert.Equal(ExtremeRoleId.Dove, model.NormalRole[1]);
        Assert.Contains(2, model.Id);
        Assert.Equal(ExtremeRoleId.Militant, model.NormalRole[2]);
    }

    [Fact]
    public void Test_RoleAssignFilterModel_SerializeAndDeserialize_V1_PreservesData()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var config = new ConfigFile(tempPath, true);
        var entry = config.Bind("Test", "Filter", "");

        var model = new RoleAssignFilterModel(entry);
        var filterData = new RoleFilterData
        {
            AssignNum = 3,
            FilterNormalId = new Dictionary<int, ExtremeRoleId> { { 0, ExtremeRoleId.Sheriff } },
            FilterCombinationId = new Dictionary<int, CombinationRoleType> { { 1, CombinationRoleType.Lover } },
            FilterGhostRole = new Dictionary<int, ExtremeGhostRoleId> { { 2, ExtremeGhostRoleId.Wisp } }
        };
        model.FilterSet.Add(Guid.NewGuid(), filterData);

        string serialized = model.SerializeToString();
        Assert.StartsWith("v1|", serialized);

        var deserializedModel = new RoleAssignFilterModel(entry);
        deserializedModel.DeserializeFromString(serialized);

        Assert.Single(deserializedModel.FilterSet);
        var deserializedData = deserializedModel.FilterSet.Values.First();
        Assert.Equal(3, deserializedData.AssignNum);
        Assert.Equal(ExtremeRoleId.Sheriff, deserializedData.FilterNormalId[0]);
        Assert.Equal(CombinationRoleType.Lover, deserializedData.FilterCombinationId[1]);
        Assert.Equal(ExtremeGhostRoleId.Wisp, deserializedData.FilterGhostRole[2]);
    }

    [Fact]
    public void Test_RoleAssignFilterModel_DeserializeLegacy_MigratesLiberalOffset()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var config = new ConfigFile(tempPath, true);
        var entry = config.Bind("Test", "Filter", "");

        var filterData = new RoleFilterData
        {
            AssignNum = 1,
            FilterNormalId = new Dictionary<int, ExtremeRoleId> { { 0, ExtremeRoleId.Sheriff } },
            FilterCombinationId = new Dictionary<int, CombinationRoleType> { { 1, CombinationRoleType.Lover } },
            FilterGhostRole = new Dictionary<int, ExtremeGhostRoleId> { { 2, ExtremeGhostRoleId.Wisp } }
        };

        var serializer = new DataContractSerializer(typeof(RoleFilterData));
        string legacyBase64;
        using (var stream = new MemoryStream())
        {
            serializer.WriteObject(stream, filterData);
            legacyBase64 = Convert.ToBase64String(stream.ToArray());
        }

        var deserializedModel = new RoleAssignFilterModel(entry);
        deserializedModel.DeserializeFromString(legacyBase64);

        Assert.Single(deserializedModel.FilterSet);
        var deserializedData = deserializedModel.FilterSet.Values.First();
        Assert.Equal(1, deserializedData.AssignNum);
        Assert.Equal(ExtremeRoleId.Sheriff, deserializedData.FilterNormalId[3]);
        Assert.Equal(CombinationRoleType.Lover, deserializedData.FilterCombinationId[4]);
        Assert.Equal(ExtremeGhostRoleId.Wisp, deserializedData.FilterGhostRole[5]);
    }
}
