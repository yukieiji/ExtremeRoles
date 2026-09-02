using System.Collections.Generic;
using UnityEngine;
using Xunit;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.UnitTest.GhostRoles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class IgniterTests
{
    public IgniterTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        EnsureGhostRoleOptionsCreated();
    }

    private static void EnsureGhostRoleOptionsCreated()
    {
        try
        {
            if (ClientOption.Instance == null)
            {
                OptionCreator.Create();
            }
        }
        catch (System.ArgumentException) { }

        foreach (var ghost in ExtremeGhostRoleManager.AllGhostRole.Values)
        {
            OptionTab tab = ghost.IsCrewmate() ? OptionTab.GhostCrewmateTab : ghost.IsImpostor() ? OptionTab.GhostImpostorTab : OptionTab.GhostNeutralTab;
            if (!OptionManager.Instance.TryGetCategory(tab, ExtremeGhostRoleManager.GetRoleGroupId(ghost.Id), out _))
            {
                try
                {
                    ghost.CreateRoleAllOption();
                }
                catch (System.ArgumentException) { }
            }
        }
    }

    [Fact]
    public void Properties_MatchIgniterDefaults()
    {
        var igniter = new Igniter();

        Assert.Equal(ExtremeGhostRoleId.Igniter, igniter.Id);
        Assert.Equal(ExtremeRoleType.Impostor, igniter.Team);
        Assert.False(igniter.HasTask);
        Assert.Equal(ExtremeGhostRoleId.Igniter.ToString(), igniter.Name);
    }

    [Fact]
    public void GetRoleFilter_ContainsLastWolf()
    {
        var igniter = new Igniter();

        var filter = igniter.GetRoleFilter();

        Assert.Single(filter);
        Assert.Contains(ExtremeRoleId.LastWolf, filter);
    }

    [Fact]
    public void InitializeAndMeetingHooks_ExecuteWithoutException()
    {
        var igniter = new Igniter();

        igniter.Initialize();
        igniter.ResetOnMeetingStart();
        igniter.ResetOnMeetingEnd();

        Assert.Equal(ExtremeGhostRoleId.Igniter, igniter.Id);
    }
}
