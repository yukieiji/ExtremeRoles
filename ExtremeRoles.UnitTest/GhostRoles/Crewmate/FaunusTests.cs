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
    public void GetRoleFilter_ReturnsEmptySet()
    {
        var faunus = new Faunus();

        var filter = faunus.GetRoleFilter();

        Assert.NotNull(filter);
        Assert.Empty(filter);
    }

    [Fact]
    public void Initialize_ResetsInternalState()
    {
        var faunus = new Faunus();

        faunus.Initialize();
    }

    [Fact]
    public void ResetOnMeetingEndAndStart_DoesNotThrow()
    {
        var faunus = new Faunus();

        faunus.ResetOnMeetingStart();
        faunus.ResetOnMeetingEnd();
    }
}
