using System;
using System.Collections.Generic;
using Xunit;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class DoppelgangerTests
{
    public DoppelgangerTests()
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
    public void Doppl_WhenPlayersMissing_ExecutesWithoutError()
    {
        Doppelganger.Doppl(99, 98);
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptySet()
    {
        var doppelganger = new Doppelganger();

        var filter = doppelganger.GetRoleFilter();

        Assert.NotNull(filter);
        Assert.Empty(filter);
    }

    [Fact]
    public void InitializeAndHooks_ExecuteWithoutError()
    {
        var doppelganger = new Doppelganger();

        doppelganger.Initialize();
        doppelganger.ResetOnMeetingStart();
        doppelganger.ResetOnMeetingEnd();
    }
}
