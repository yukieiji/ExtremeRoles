using System;
using System.Collections.Generic;
using Xunit;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Crewmate;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class PoltergeistTests
{
    public PoltergeistTests()
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
        try
        {
            if (ClientOption.Instance == null)
            {
                OptionCreator.Create();
            }
        }
        catch (ArgumentException) { }

        foreach (var ghost in ExtremeGhostRoleManager.AllGhostRole.Values)
        {
            OptionTab tab = ghost.IsCrewmate() ? OptionTab.GhostCrewmateTab : ghost.IsImpostor() ? OptionTab.GhostImpostorTab : OptionTab.GhostNeutralTab;
            if (!OptionManager.Instance.TryGetCategory(tab, ExtremeGhostRoleManager.GetRoleGroupId(ghost.Id), out _))
            {
                try
                {
                    ghost.CreateRoleAllOption();
                }
                catch (ArgumentException) { }
            }
        }
    }

    [Fact]
    public void DeadbodyMove_WhenPlayerNotFound_ExecutesWithoutError()
    {
        Poltergeist.DeadbodyMove(99, 98, 0.0f, 0.0f, false);
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptySet()
    {
        var poltergeist = new Poltergeist();

        var filter = poltergeist.GetRoleFilter();

        Assert.NotNull(filter);
        Assert.Empty(filter);
    }

    [Fact]
    public void Initialize_ReadsLoaderOptionsWithoutError()
    {
        var poltergeist = new Poltergeist();

        poltergeist.Initialize();
        poltergeist.ResetOnMeetingStart();
        poltergeist.ResetOnMeetingEnd();
    }
}
