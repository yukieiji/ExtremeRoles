using System.Collections.Generic;
using Xunit;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Crewmate;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class FaunusTests
{
    public FaunusTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Act
        var faunus = new Faunus();

        // Assert
        Assert.True(faunus.HasTask);
        Assert.Equal(ExtremeRoleType.Crewmate, faunus.Team);
        Assert.Equal(ExtremeGhostRoleId.Faunus, faunus.Id);
        Assert.Equal("Faunus", faunus.Name);
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptySet()
    {
        // Arrange
        var faunus = new Faunus();

        // Act
        var filter = faunus.GetRoleFilter();

        // Assert
        Assert.NotNull(filter);
        Assert.Empty(filter);
    }

    [Fact]
    public void Initialize_ResetsInternalState()
    {
        // Arrange
        var faunus = new Faunus();

        // Act & Assert
        faunus.Initialize();
    }

    [Fact]
    public void ResetOnMeetingEndAndStart_DoesNotThrow()
    {
        // Arrange
        var faunus = new Faunus();

        // Act & Assert
        faunus.ResetOnMeetingStart();
        faunus.ResetOnMeetingEnd();
    }
}
