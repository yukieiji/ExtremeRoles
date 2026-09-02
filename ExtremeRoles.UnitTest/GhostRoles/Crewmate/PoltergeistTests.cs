using System.Collections.Generic;
using Xunit;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Crewmate;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.UnitTest.GhostRoles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
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
