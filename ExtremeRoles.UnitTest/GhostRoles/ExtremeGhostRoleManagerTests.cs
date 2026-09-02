using System;
using Hazel;
using Moq;
using Xunit;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Crewmate;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.UnitTest.GhostRoles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ExtremeGhostRoleManagerTests
{
    public ExtremeGhostRoleManagerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        MockSetupHelper.SetupLogger();

        EnsureGhostRoleOptionsCreated();

        ExtremeGhostRoleManager.Initialize();
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
    public void GetRoleGroupId_CalculatesCorrectOffset()
    {
        int idPoltergeist = ExtremeGhostRoleManager.GetRoleGroupId(ExtremeGhostRoleId.Poltergeist);
        int idForas = ExtremeGhostRoleManager.GetRoleGroupId(ExtremeGhostRoleId.Foras);

        Assert.Equal(512 + (int)ExtremeGhostRoleId.Poltergeist, idPoltergeist);
        Assert.Equal(512 + (int)ExtremeGhostRoleId.Foras, idForas);
    }

    [Fact]
    public void GetSafeCastedGhostRole_WithValidAndInvalidIds_ReturnsExpected()
    {
        byte playerId = 10;
        var poltergeist = ExtremeGhostRoleManager.AllGhostRole[ExtremeGhostRoleId.Poltergeist];
        ExtremeGhostRoleManager.GameRole[playerId] = poltergeist;

        var castedPoltergeist = ExtremeGhostRoleManager.GetSafeCastedGhostRole<Poltergeist>(playerId);
        var castedMismatch = ExtremeGhostRoleManager.GetSafeCastedGhostRole<Ventgeist>(playerId);
        var castedMissing = ExtremeGhostRoleManager.GetSafeCastedGhostRole<Poltergeist>(99);

        Assert.NotNull(castedPoltergeist);
        Assert.Same(poltergeist, castedPoltergeist);
        Assert.Null(castedMismatch);
        Assert.Null(castedMissing);
    }

    [Fact]
    public void Initialize_ClearsRolesAndResetsState()
    {
        ExtremeGhostRoleManager.GameRole[1] = ExtremeGhostRoleManager.AllGhostRole[ExtremeGhostRoleId.Poltergeist];

        ExtremeGhostRoleManager.Initialize();

        Assert.Empty(ExtremeGhostRoleManager.GameRole);
    }

    [Fact]
    public void UseAbility_SaboEvilResetSabotageCool_ExecutesWithoutError()
    {
        var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
        var dict = new Mock<Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, ISystemType>>(IntPtr.Zero);
        mockShipStatus.SetupGet(s => s.Systems).Returns(dict.Object);
        var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
        mockShipHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
        MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

        var reader = new MessageReader();

        ExtremeGhostRoleManager.UseAbility((byte)AbilityType.SaboEvilResetSabotageCool, false, ref reader);
    }
}
