using Xunit;
using ExtremeRoles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class ExtremeGhostRoleManagerTests
{
    public ExtremeGhostRoleManagerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        if (ClientOption.Instance == null || !OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)OptionCreator.CommonOption.RandomOption, out _))
        {
            OptionCreator.Create();
        }

        ExtremeGhostRoleManager.Initialize();
    }

    [Fact]
    public void AllGhostRole_ContainsAllExpectedGhostRoles()
    {
        // Assert
        Assert.Contains(ExtremeGhostRoleId.Poltergeist, ExtremeGhostRoleManager.AllGhostRole.Keys);
        Assert.Contains(ExtremeGhostRoleId.Faunus, ExtremeGhostRoleManager.AllGhostRole.Keys);
        Assert.Contains(ExtremeGhostRoleId.Shutter, ExtremeGhostRoleManager.AllGhostRole.Keys);
        Assert.Contains(ExtremeGhostRoleId.Ventgeist, ExtremeGhostRoleManager.AllGhostRole.Keys);
        Assert.Contains(ExtremeGhostRoleId.SaboEvil, ExtremeGhostRoleManager.AllGhostRole.Keys);
        Assert.Contains(ExtremeGhostRoleId.Igniter, ExtremeGhostRoleManager.AllGhostRole.Keys);
        Assert.Contains(ExtremeGhostRoleId.Doppelganger, ExtremeGhostRoleManager.AllGhostRole.Keys);
        Assert.Contains(ExtremeGhostRoleId.Foras, ExtremeGhostRoleManager.AllGhostRole.Keys);
    }

    [Fact]
    public void GetRoleGroupId_ReturnsIdWithOffset()
    {
        // Act
        int groupId = ExtremeGhostRoleManager.GetRoleGroupId(ExtremeGhostRoleId.Poltergeist);

        // Assert
        Assert.Equal(512 + (int)ExtremeGhostRoleId.Poltergeist, groupId);
    }

    [Fact]
    public void Initialize_ClearsGameRoleDictionary()
    {
        // Arrange
        ExtremeGhostRoleManager.GameRole[0] = ExtremeGhostRoleManager.AllGhostRole[ExtremeGhostRoleId.Poltergeist];

        // Act
        ExtremeGhostRoleManager.Initialize();

        // Assert
        Assert.Empty(ExtremeGhostRoleManager.GameRole);
    }

    [Fact]
    public void GetSafeCastedGhostRole_WhenRoleExists_ReturnsCastedRole()
    {
        // Arrange
        byte playerId = 1;
        var poltergeist = ExtremeGhostRoleManager.AllGhostRole[ExtremeGhostRoleId.Poltergeist];
        ExtremeGhostRoleManager.GameRole[playerId] = poltergeist;

        // Act
        var result = ExtremeGhostRoleManager.GetSafeCastedGhostRole<ExtremeRoles.GhostRoles.Crewmate.Poltergeist>(playerId);

        // Assert
        Assert.NotNull(result);
        Assert.Same(poltergeist, result);
    }

    [Fact]
    public void GetSafeCastedGhostRole_WhenTypeMismatch_ReturnsNull()
    {
        // Arrange
        byte playerId = 1;
        var poltergeist = ExtremeGhostRoleManager.AllGhostRole[ExtremeGhostRoleId.Poltergeist];
        ExtremeGhostRoleManager.GameRole[playerId] = poltergeist;

        // Act
        var result = ExtremeGhostRoleManager.GetSafeCastedGhostRole<ExtremeRoles.GhostRoles.Impostor.Ventgeist>(playerId);

        // Assert
        Assert.Null(result);
    }
}
