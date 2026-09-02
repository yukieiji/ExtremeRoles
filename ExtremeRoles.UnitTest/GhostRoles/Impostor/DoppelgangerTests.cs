using System.Collections.Generic;
using Xunit;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class DoppelgangerTests
{
    public DoppelgangerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Act
        var doppelganger = new Doppelganger();

        // Assert
        Assert.False(doppelganger.HasTask);
        Assert.Equal(ExtremeRoleType.Impostor, doppelganger.Team);
        Assert.Equal(ExtremeGhostRoleId.Doppelganger, doppelganger.Id);
        Assert.Equal("Doppelganger", doppelganger.Name);
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptySet()
    {
        // Arrange
        var doppelganger = new Doppelganger();

        // Act
        var filter = doppelganger.GetRoleFilter();

        // Assert
        Assert.NotNull(filter);
        Assert.Empty(filter);
    }
}
