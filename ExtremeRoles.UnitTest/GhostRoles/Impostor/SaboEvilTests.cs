using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class SaboEvilTests
{
    public SaboEvilTests()
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
    public void ResetCool_ExecutesWithoutError()
    {
        var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
        var dict = new Mock<Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, ISystemType>>(IntPtr.Zero);
        mockShipStatus.SetupGet(s => s.Systems).Returns(dict.Object);
        var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
        mockShipHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
        MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

        SaboEvil.ResetCool();
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptySet()
    {
        var saboEvil = new SaboEvil();

        var filter = saboEvil.GetRoleFilter();

        Assert.NotNull(filter);
        Assert.Empty(filter);
    }

    [Fact]
    public void InitializeAndHooks_ExecuteWithoutError()
    {
        var saboEvil = new SaboEvil();

        saboEvil.Initialize();
        saboEvil.ResetOnMeetingStart();
        saboEvil.ResetOnMeetingEnd();
    }
}
