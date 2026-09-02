using System.Collections.Generic;
using Xunit;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class IgniterTests
{
    public IgniterTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Act
        var igniter = new Igniter();

        // Assert
        Assert.False(igniter.HasTask);
        Assert.Equal(ExtremeRoleType.Impostor, igniter.Team);
        Assert.Equal(ExtremeGhostRoleId.Igniter, igniter.Id);
        Assert.Equal("Igniter", igniter.Name);
    }

    [Fact]
    public void GetRoleFilter_ContainsLastWolf()
    {
        // Arrange
        var igniter = new Igniter();

        // Act
        var filter = igniter.GetRoleFilter();

        // Assert
        Assert.Contains(ExtremeRoleId.LastWolf, filter);
    }
}
