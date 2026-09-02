using System.Collections.Generic;
using Hazel;
using Moq;
using Xunit;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Crewmate;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class ShutterTests
{
    public ShutterTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();

        var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        mockTranslation.Setup(x => x.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>>()))
            .Returns((string id, string defaultStr, Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object> parts) => defaultStr ?? id);

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
    public void GhostPhotoSerializer_ToString_WhenEmpty_ReturnsEmptyString()
    {
        var serializer = new Shutter.GhostPhotoSerializer();

        string result = serializer.ToString();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetRoleFilter_ReturnsPhotographer()
    {
        var shutter = new Shutter();

        var filter = shutter.GetRoleFilter();

        Assert.Contains(ExtremeRoleId.Photographer, filter);
    }

    [Fact]
    public void InitializeAndMeetingHooks_ExecuteWithoutError()
    {
        var shutter = new Shutter();

        shutter.Initialize();
        shutter.ResetOnMeetingStart();
        shutter.ResetOnMeetingEnd();
    }
}
