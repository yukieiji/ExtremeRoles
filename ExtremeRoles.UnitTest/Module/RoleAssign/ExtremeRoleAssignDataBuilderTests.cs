#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class ExtremeRoleAssignDataBuilderTests
{
    public ExtremeRoleAssignDataBuilderTests()
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
    public void Build_ExecutesBehavioursAndReturnsAssignData()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockPreparer = new Mock<IRoleAssignDataPreparer>();
        var mockInitializer = new Mock<IAssignFilterInitializer>();
        var mockValidator = new Mock<IRoleAssignValidator>();

        var mockRoleProvider = new Mock<IVanillaRoleProvider>();
        var mockDataProvider = new Mock<IVanillaRolePlayerAssignDataProvider>();
        var assignData = new PlayerRoleAssignData(mockRoleProvider.Object, mockDataProvider.Object);

        var mockSpawnData = new Mock<ISpawnDataManager>();
        mockSpawnData.SetupGet(s => s.UseGhostCombRole).Returns(System.Array.Empty<(CombinationRoleType, GhostAndAliveCombinationRoleManagerBase)>());

        var mockLimit = new Mock<ISpawnLimiter>();
        var prepData = new PreparationData(assignData, mockSpawnData.Object, mockLimit.Object);

        mockPreparer.Setup(p => p.Prepare()).Returns(prepData);

        var mockBehaviour = new Mock<IRoleAssignDataBuildBehaviour>();
        mockBehaviour.SetupGet(b => b.Priority).Returns((int)ExtremeRoleAssignDataBuilder.Priority.Single);

        mockServiceProvider.Setup(p => p.GetService(typeof(IEnumerable<IRoleAssignDataBuildBehaviour>)))
            .Returns(new[] { mockBehaviour.Object });

        mockValidator.Setup(v => v.IsReBuild(prepData)).Returns(false);

        var builder = new ExtremeRoleAssignDataBuilder(
            mockServiceProvider.Object,
            mockPreparer.Object,
            mockInitializer.Object,
            mockValidator.Object);

        var result = builder.Build();

        Assert.NotNull(result);
        mockInitializer.Verify(i => i.Initialize(It.IsAny<RoleAssignFilter>(), prepData), Times.Once);
        mockBehaviour.Verify(b => b.Build(prepData), Times.Once);
    }
}
