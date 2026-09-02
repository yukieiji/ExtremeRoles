using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Xunit;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class VanillaGhostRoleWrapperTests
{
    public VanillaGhostRoleWrapperTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void Constructor_GuardianAngel_SetsCrewmateAndHasTaskTrue()
    {
        // Arrange & Act
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.GuardianAngel);

        // Assert
        Assert.Equal(ExtremeRoleType.Crewmate, wrapper.Team);
        Assert.True(wrapper.HasTask);
        Assert.Equal(ExtremeGhostRoleId.VanillaRole, wrapper.Id);
        Assert.Equal(RoleTypes.GuardianAngel.ToString(), wrapper.Name);
    }

    [Fact]
    public void Constructor_ImpostorGhost_SetsImpostorAndHasTaskFalse()
    {
        // Arrange & Act
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.ImpostorGhost);

        // Assert
        Assert.Equal(ExtremeRoleType.Impostor, wrapper.Team);
        Assert.False(wrapper.HasTask);
        Assert.Equal(ExtremeGhostRoleId.VanillaRole, wrapper.Id);
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptyHashSet()
    {
        // Arrange
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.GuardianAngel);

        // Act
        var filter = wrapper.GetRoleFilter();

        // Assert
        Assert.NotNull(filter);
        Assert.Empty(filter);
    }

    [Fact]
    public void CallingDisabledMethods_ThrowsException()
    {
        // Arrange
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.GuardianAngel);

        // Act & Assert
        Assert.Throws<Exception>(() => wrapper.CreateAbility());
    }
}
