using System.Collections.Generic;
using UnityEngine;
using Xunit;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.UnitTest.GhostRoles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class DoppelgangerTests
{
    public DoppelgangerTests()
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
    public void GetRoleFilter_ReturnsEmptySet()
    {
        var doppelganger = new Doppelganger();

        var filter = doppelganger.GetRoleFilter();

        Assert.NotNull(filter);
        Assert.Empty(filter);
    }

    [Fact]
    public void GetImportantText_ReturnsFormattedString()
    {
        var doppelganger = new Doppelganger();

        string importantText = doppelganger.GetImportantText();

        Assert.NotNull(importantText);
    }

    [Fact]
    public void Doppl_WhenPlayersMissing_ExecutesWithoutError()
    {
        Doppelganger.Doppl(99, 98);

        var role = ExtremeGhostRoleManager.GetSafeCastedGhostRole<Doppelganger>(99);
        Assert.Null(role);
    }
}
