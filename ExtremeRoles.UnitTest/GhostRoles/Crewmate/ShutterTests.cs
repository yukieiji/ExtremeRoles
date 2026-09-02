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

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ShutterTests
{
    public ShutterTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        MockSetupHelper.SetupDebugMode();
        MockSetupHelper.SetupLogger();
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
            catch (ArgumentException) { }
        }
    }

    [Fact]
    public void Properties_MatchShutterDefaults()
    {
        var shutter = new Shutter();

        Assert.Equal(ExtremeGhostRoleId.Shutter, shutter.Id);
        Assert.Equal(ExtremeRoleType.Crewmate, shutter.Team);
        Assert.True(shutter.HasTask);
        Assert.Equal(ExtremeGhostRoleId.Shutter.ToString(), shutter.Name);
    }

    [Fact]
    public void GetRoleFilter_ReturnsExpectedFilter()
    {
        var shutter = new Shutter();

        var filter = shutter.GetRoleFilter();

        Assert.Single(filter);
        Assert.Contains(Roles.ExtremeRoleId.Photographer, filter);
    }

    [Fact]
    public void InitializeAndMeetingHooks_ExecuteWithoutException()
    {
        var shutter = new Shutter();

        shutter.Initialize();
        shutter.ResetOnMeetingStart();
        shutter.ResetOnMeetingEnd();

        Assert.Equal(ExtremeGhostRoleId.Shutter, shutter.Id);
    }

    [Fact]
    public void GhostPhotoSerializer_ToString_WhenEmpty_ReturnsEmptyString()
    {
        var serializer = new Shutter.GhostPhotoSerializer();

        string result = serializer.ToString();

        Assert.Equal(string.Empty, result);
    }
}
