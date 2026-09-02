using System.Collections.Generic;
using Xunit;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Crewmate;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class ShutterTests
{
    public ShutterTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Act
        var shutter = new Shutter();

        // Assert
        Assert.True(shutter.HasTask);
        Assert.Equal(ExtremeRoleType.Crewmate, shutter.Team);
        Assert.Equal(ExtremeGhostRoleId.Shutter, shutter.Id);
        Assert.Equal("Shutter", shutter.Name);
    }

    [Fact]
    public void GetRoleFilter_ContainsPhotographer()
    {
        // Arrange
        var shutter = new Shutter();

        // Act
        var filter = shutter.GetRoleFilter();

        // Assert
        Assert.Contains(ExtremeRoleId.Photographer, filter);
    }
}
