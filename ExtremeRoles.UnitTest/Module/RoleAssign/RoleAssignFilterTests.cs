#nullable enable

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class RoleAssignFilterTests
{
    public RoleAssignFilterTests()
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
    public void SingletonInstance_NotNull()
    {
        var filter = RoleAssignFilter.Instance;
        Assert.NotNull(filter);
        Assert.NotNull(filter.Model);
    }

    [Fact]
    public void IsBlock_WhenEmpty_ReturnsFalse()
    {
        var filter = RoleAssignFilter.Instance;
        filter.Initialize();

        Assert.False(filter.IsBlock((int)ExtremeRoleId.Vigilante));
        Assert.False(filter.IsBlock((byte)CombinationRoleType.Lover));
        Assert.False(filter.IsBlock(ExtremeGhostRoleId.Wisp));
    }

    [Fact]
    public void SerializeModel_ReturnsString()
    {
        var filter = RoleAssignFilter.Instance;
        var serialized = filter.SerializeModel();

        Assert.NotNull(serialized);
        Assert.StartsWith("v1", serialized);
    }
}
