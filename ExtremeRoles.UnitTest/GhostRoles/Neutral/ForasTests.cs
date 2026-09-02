using System.Collections.Generic;
using Hazel;
using UnityEngine;
using Xunit;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Neutal;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.UnitTest.GhostRoles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ForasTests
{
    public ForasTests()
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
    public void Properties_MatchForasDefaults()
    {
        var foras = new Foras();

        Assert.Equal(ExtremeGhostRoleId.Foras, foras.Id);
        Assert.Equal(ExtremeRoleType.Neutral, foras.Team);
        Assert.False(foras.HasTask);
        Assert.Equal(ExtremeGhostRoleId.Foras.ToString(), foras.Name);
    }

    [Fact]
    public void GetRoleFilter_ContainsSidekickAndServant()
    {
        var foras = new Foras();

        var filter = foras.GetRoleFilter();

        Assert.Equal(2, filter.Count);
        Assert.Contains(ExtremeRoleId.Sidekick, filter);
        Assert.Contains(ExtremeRoleId.Servant, filter);
    }

    [Fact]
    public void InitializeAndMeetingHooks_ExecuteWithoutException()
    {
        var foras = new Foras();

        foras.Initialize();
        foras.ResetOnMeetingStart();
        foras.ResetOnMeetingEnd();

        Assert.Equal(ExtremeGhostRoleId.Foras, foras.Id);
    }
}
