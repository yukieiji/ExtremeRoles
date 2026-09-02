using System.Collections.Generic;
using Xunit;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Crewmate;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class PoltergeistTests
{
    public PoltergeistTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Act
        var poltergeist = new Poltergeist();

        // Assert
        Assert.True(poltergeist.HasTask);
        Assert.Equal(ExtremeRoleType.Crewmate, poltergeist.Team);
        Assert.Equal(ExtremeGhostRoleId.Poltergeist, poltergeist.Id);
        Assert.Equal("Poltergeist", poltergeist.Name);
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptySet()
    {
        // Arrange
        var poltergeist = new Poltergeist();

        // Act
        var filter = poltergeist.GetRoleFilter();

        // Assert
        Assert.NotNull(filter);
        Assert.Empty(filter);
    }
}
