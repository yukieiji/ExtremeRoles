using System.Collections.Generic;
using UnityEngine;
using Xunit;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Crewmate;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.UnitTest.GhostRoles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class FaunusTests
{
    public FaunusTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        EnsureGhostRoleOptionsCreated();
    }

    private static void EnsureGhostRoleOptionsCreated()
    {
        if (ClientOption.Instance == null || !OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)OptionCreator.CommonOption.RandomOption, out _))
        {
            try
            {
                OptionCreator.Create();
            }
            catch (System.ArgumentException) { }
        }
    }

    [Fact]
    public void Properties_MatchExpectedDefaults()
    {
        var faunus = new Faunus();

        Assert.Equal(ExtremeGhostRoleId.Faunus, faunus.Id);
        Assert.Equal(ExtremeRoleType.Crewmate, faunus.Team);
        Assert.True(faunus.HasTask);
        Assert.Equal(ExtremeGhostRoleId.Faunus.ToString(), faunus.Name);
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptySet()
    {
        var faunus = new Faunus();

        var filter = faunus.GetRoleFilter();

        Assert.NotNull(filter);
        Assert.Empty(filter);
    }

    [Fact]
    public void Initialize_ResetsInternalStateWithoutThrowing()
    {
        var faunus = new Faunus();

        faunus.Initialize();

        Assert.Equal(ExtremeGhostRoleId.Faunus, faunus.Id);
    }
}
