using System;
using System.Collections.Generic;
using UnityEngine;
using Xunit;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.UnitTest.GhostRoles;

public class GhostRoleBaseTests
{
    private sealed class TestGhostRole : GhostRoleBase
    {
        public bool MeetingEndCalled { get; private set; }
        public bool MeetingStartCalled { get; private set; }
        public bool SpecificOptionCalled { get; private set; }

        public TestGhostRole(
            bool hasTask,
            ExtremeRoleType team,
            ExtremeGhostRoleId id,
            string roleName,
            Color color,
            OptionTab tab = OptionTab.GeneralTab)
            : base(hasTask, team, id, roleName, color, tab)
        {
        }

        public override void CreateAbility() { }

        public override HashSet<ExtremeRoles.Roles.ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoles.Roles.ExtremeRoleId>();

        public override void Initialize() { }

        protected override void OnMeetingEndHook()
        {
            MeetingEndCalled = true;
        }

        protected override void OnMeetingStartHook()
        {
            MeetingStartCalled = true;
        }

        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory parentOps)
        {
            SpecificOptionCalled = true;
        }

        protected override void UseAbility(RPCOperator.RpcCaller caller) { }
    }

    public GhostRoleBaseTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void Constructor_GeneralTab_AssignsTabBasedOnTeam()
    {
        // Arrange & Act
        var crewGhost = new TestGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Poltergeist, "Poltergeist", Color.white);
        var impGhost = new TestGhostRole(false, ExtremeRoleType.Impostor, ExtremeGhostRoleId.Ventgeist, "Ventgeist", Color.red);
        var neutralGhost = new TestGhostRole(false, ExtremeRoleType.Neutral, ExtremeGhostRoleId.Foras, "Foras", Color.yellow);

        // Assert
        Assert.True(crewGhost.IsCrewmate());
        Assert.True(impGhost.IsImpostor());
        Assert.True(neutralGhost.IsNeutral());
    }

    [Fact]
    public void Clone_CopiesPropertiesAndCreatesNewColorInstance()
    {
        // Arrange
        var originalColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        var original = new TestGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", originalColor);

        // Act
        var clone = original.Clone();

        // Assert
        Assert.NotSame(original, clone);
        Assert.Equal(original.Id, clone.Id);
        Assert.Equal(original.Team, clone.Team);
        Assert.Equal(original.HasTask, clone.HasTask);
        Assert.Equal(originalColor.r, clone.Color.r);
        Assert.Equal(originalColor.g, clone.Color.g);
        Assert.Equal(originalColor.b, clone.Color.b);
        Assert.Equal(originalColor.a, clone.Color.a);
    }

    [Fact]
    public void ResetOnMeetingEndAndStart_TriggersHooks()
    {
        // Arrange
        var role = new TestGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Poltergeist, "Poltergeist", Color.white);

        // Act
        role.ResetOnMeetingStart();
        role.ResetOnMeetingEnd();

        // Assert
        Assert.True(role.MeetingStartCalled);
        Assert.True(role.MeetingEndCalled);
    }

    [Fact]
    public void GetTargetRoleSeeColor_WhenTargetIsImpostorAndThisIsImpostor_ReturnsRed()
    {
        // Arrange
        var impGhost = new TestGhostRole(false, ExtremeRoleType.Impostor, ExtremeGhostRoleId.Ventgeist, "Ventgeist", Color.red);
        var impRole = new ExtremeRoles.Roles.Solo.Impostor.BountyHunter();

        // Act
        Color seeColor = impGhost.GetTargetRoleSeeColor(1, impRole, null);

        // Assert
        Assert.Equal(Palette.ImpostorRed, seeColor);
    }

    [Fact]
    public void GetTargetRoleSeeColor_WhenTargetIsCrewmateAndThisIsImpostor_ReturnsClear()
    {
        // Arrange
        var impGhost = new TestGhostRole(false, ExtremeRoleType.Impostor, ExtremeGhostRoleId.Ventgeist, "Ventgeist", Color.red);
        var crewRole = new SpecialCrew();

        // Act
        Color seeColor = impGhost.GetTargetRoleSeeColor(1, crewRole, null);

        // Assert
        Assert.Equal(Color.clear, seeColor);
    }
}
