using System.Collections.Generic;
using Xunit;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class IgniterTests
{
    public IgniterTests()
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
        catch (System.ArgumentException) { }
    }

    [Fact]
    public void GetRoleFilter_ContainsLastWolf()
    {
        var igniter = new Igniter();

        var filter = igniter.GetRoleFilter();

        Assert.Contains(ExtremeRoleId.LastWolf, filter);
    }

    [Fact]
    public void Initialize_ReadsLoaderOptionsWithoutError()
    {
        var igniter = new Igniter();

        if (!OptionManager.Instance.TryGetCategory(OptionTab.GhostImpostorTab, ExtremeGhostRoleManager.GetRoleGroupId(igniter.Id), out _))
        {
            try { igniter.CreateRoleAllOption(); } catch (System.ArgumentException) { }
        }

        igniter.Initialize();
        igniter.ResetOnMeetingStart();
        igniter.ResetOnMeetingEnd();
    }
}
