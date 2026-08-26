#nullable enable

using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.OLDS;
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
    public void CanSpawn_And_Reduce_Behavior()
    {
        var limiter = new ExtremeSpawnLimiter();

        int initialCrewLimit = limiter.Get(ExtremeRoleType.Crewmate);
        Assert.True(limiter.CanSpawn(ExtremeRoleType.Crewmate, 0));

        limiter.Reduce(ExtremeRoleType.Crewmate, -2);
        Assert.Equal(initialCrewLimit + 2, limiter.Get(ExtremeRoleType.Crewmate));
        Assert.True(limiter.CanSpawn(ExtremeRoleType.Crewmate, 1));
    }

    [Fact]
    public void ToString_ContainsTeamAndLimitDetails()
    {
        var limiter = new ExtremeSpawnLimiter();
        var str = limiter.ToString();

        Assert.Contains("Spawn Limit", str);
        Assert.Contains("Team:Crewmate", str);
        Assert.Contains("Team:Impostor", str);
        Assert.Contains("Team:Neutral", str);
        Assert.Contains("Team:Liberal", str);
    }
}
