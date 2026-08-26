using ExtremeRoles;
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
    public void Test_RoleAssignFilter_InstanceAndMethods()
    {
        var filter = RoleAssignFilter.Instance;
        Assert.NotNull(filter);
        Assert.NotNull(filter.Model);

        filter.Initialize();

        Assert.False(filter.IsBlock((int)ExtremeRoleId.Sheriff));
        Assert.False(filter.IsBlock((byte)CombinationRoleType.Lover));
        Assert.False(filter.IsBlock(ExtremeGhostRoleId.Wisp));

        filter.Update((int)ExtremeRoleId.Sheriff);
        filter.Update((byte)CombinationRoleType.Lover);
        filter.Update(ExtremeGhostRoleId.Wisp);

        string serialized = filter.SerializeModel();
        Assert.NotNull(serialized);

        filter.DeserializeModel("");
        filter.SwitchPreset();
        Assert.NotNull(filter.Model);
    }
}
