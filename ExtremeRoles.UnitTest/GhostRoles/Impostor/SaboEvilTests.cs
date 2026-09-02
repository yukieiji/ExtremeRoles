using System.Collections.Generic;
using Xunit;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class SaboEvilTests
{
    public SaboEvilTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Act
        var saboEvil = new SaboEvil();

        // Assert
        Assert.False(saboEvil.HasTask);
        Assert.Equal(ExtremeRoleType.Impostor, saboEvil.Team);
        Assert.Equal(ExtremeGhostRoleId.SaboEvil, saboEvil.Id);
        Assert.Equal("SaboEvil", saboEvil.Name);
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptySet()
    {
        // Arrange
        var saboEvil = new SaboEvil();

        // Act
        var filter = saboEvil.GetRoleFilter();

        // Assert
        Assert.NotNull(filter);
        Assert.Empty(filter);
    }
}
