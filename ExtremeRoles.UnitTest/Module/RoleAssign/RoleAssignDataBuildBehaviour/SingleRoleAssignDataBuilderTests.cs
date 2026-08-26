#nullable enable

using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign.RoleAssignDataBuildBehaviour;

[Collection("UnityMock")]
public class SingleRoleAssignDataBuilderTests
{
    public SingleRoleAssignDataBuilderTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        var debugModeProp = typeof(ExtremeRolesPlugin).GetProperty("DebugMode", BindingFlags.Public | BindingFlags.Static);
        if (debugModeProp != null && debugModeProp.GetValue(null) == null)
        {
            var entry = plugin.Config.Bind("DeBug", "DebugMode", false);
            debugModeProp.SetValue(null, entry);
        }

        if (!OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, PresetOption.CategoryId, out _))
        {
            OptionCreator.Create();
        }
    }

    [Fact]
    public void Priority_ReturnsSinglePriority()
    {
        var mockProvider = new Mock<IVanillaRoleProvider>();
        var builder = new SingleRoleAssignDataBuilder(mockProvider.Object);
        Assert.Equal((int)ExtremeRoleAssignDataBuilder.Priority.Single, builder.Priority);
    }

    [Fact]
    public void Build_EmptyPlayers_DoesNotThrow()
    {
        var mockProvider = new Mock<IVanillaRoleProvider>();
        mockProvider.SetupGet(p => p.CrewmateRole).Returns(new HashSet<RoleTypes>());
        mockProvider.SetupGet(p => p.ImpostorRole).Returns(new HashSet<RoleTypes>());
        mockProvider.SetupGet(p => p.AllCrewmate).Returns(new HashSet<RoleTypes>());
        mockProvider.SetupGet(p => p.AllImpostor).Returns(new HashSet<RoleTypes>());

        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();
        mockDataProvider.SetupGet(d => d.Data).Returns(System.Array.Empty<VanillaRolePlayerAssignData>());

        var assignData = new PlayerRoleAssignData(mockProvider.Object, mockDataProvider.Object);
        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(s => s.CurrentSingleRoleSpawnData).Returns(new Dictionary<ExtremeRoles.Roles.API.ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>());

        var mockLimit = new Mock<ISpawnLimiter>();
        var prepData = new PreparationData(assignData, mockSpawnData.Object, mockLimit.Object);

        var builder = new SingleRoleAssignDataBuilder(mockProvider.Object);
        builder.Build(in prepData);

        Assert.Empty(assignData.Data);
    }
}
