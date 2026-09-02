using System.Collections.Generic;
using Xunit;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class VentgeistTests
{
    public VentgeistTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Act
        var ventgeist = new Ventgeist();

        // Assert
        Assert.False(ventgeist.HasTask);
        Assert.Equal(ExtremeRoleType.Impostor, ventgeist.Team);
        Assert.Equal(ExtremeGhostRoleId.Ventgeist, ventgeist.Id);
        Assert.Equal("Ventgeist", ventgeist.Name);
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptySet()
    {
        // Arrange
        var ventgeist = new Ventgeist();

        // Act
        var filter = ventgeist.GetRoleFilter();

        // Assert
        Assert.NotNull(filter);
        Assert.Empty(filter);
    }
}
