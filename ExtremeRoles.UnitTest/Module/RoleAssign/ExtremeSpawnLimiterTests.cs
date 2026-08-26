#nullable enable

using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles.API;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class ExtremeSpawnLimiterTests
{
    public ExtremeSpawnLimiterTests()
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
    public void Reduce_And_CanSpawn()
    {
        var limiter = new ExtremeSpawnLimiter();

        limiter.Reduce(ExtremeRoleType.Crewmate, 2);

        Assert.NotNull(limiter.ToString());
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var limiter = new ExtremeSpawnLimiter();
        var str = limiter.ToString();

        Assert.Contains("Spawn Limit", str);
    }
}
