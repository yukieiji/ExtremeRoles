using System.Collections.Generic;
using Xunit;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Neutal;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class ForasTests
{
    public ForasTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Act
        var foras = new Foras();

        // Assert
        Assert.False(foras.HasTask);
        Assert.Equal(ExtremeRoleType.Neutral, foras.Team);
        Assert.Equal(ExtremeGhostRoleId.Foras, foras.Id);
        Assert.Equal("Foras", foras.Name);
    }

    [Fact]
    public void GetRoleFilter_ContainsSidekickAndServant()
    {
        // Arrange
        var foras = new Foras();

        // Act
        var filter = foras.GetRoleFilter();

        // Assert
        Assert.Contains(ExtremeRoleId.Sidekick, filter);
        Assert.Contains(ExtremeRoleId.Servant, filter);
    }
}
