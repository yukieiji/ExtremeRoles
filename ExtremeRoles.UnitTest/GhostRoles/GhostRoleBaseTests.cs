using System;
using System.Collections.Generic;
using UnityEngine;
using Xunit;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.UnitTest.GhostRoles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class GhostRoleBaseTests
{
    private sealed class DummyCombGhostRole : GhostRoleBase, ICombination
    {
        public MultiAssignRoleBase.OptionOffsetInfo? OffsetInfo { get; set; } = new MultiAssignRoleBase.OptionOffsetInfo(CombinationRoleType.Kids, 2);

        public DummyCombGhostRole() : base(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Poltergeist, "Poltergeist", Color.blue) { }

        public override void CreateAbility() { }
        public override HashSet<ExtremeRoles.Roles.ExtremeRoleId> GetRoleFilter() => new();
        public override void Initialize() { }
        protected override void OnMeetingEndHook() { }
        protected override void OnMeetingStartHook() { }
        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory parentOps) { }
        protected override void UseAbility(RPCOperator.RpcCaller caller) { }
    }

    private sealed class DummyGhostRole : GhostRoleBase
    {
        public DummyGhostRole(ExtremeRoleType team, ExtremeGhostRoleId id)
            : base(true, team, id, "TestRole", Color.green) { }

        public override void CreateAbility() { }
        public override HashSet<ExtremeRoles.Roles.ExtremeRoleId> GetRoleFilter() => new();
        public override void Initialize() { }
        protected override void OnMeetingEndHook() { }
        protected override void OnMeetingStartHook() { }
        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory parentOps) { }
        protected override void UseAbility(RPCOperator.RpcCaller caller) { }
    }

    public GhostRoleBaseTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void TeamPredicates_ReturnCorrectTeamFlags()
    {
        var crew = new DummyGhostRole(ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Poltergeist);
        var imp = new DummyGhostRole(ExtremeRoleType.Impostor, ExtremeGhostRoleId.Ventgeist);
        var neutral = new DummyGhostRole(ExtremeRoleType.Neutral, ExtremeGhostRoleId.Foras);
        var vanilla = new DummyGhostRole(ExtremeRoleType.Crewmate, ExtremeGhostRoleId.VanillaRole);

        Assert.True(crew.IsCrewmate());
        Assert.False(crew.IsImpostor());
        Assert.False(crew.IsNeutral());

        Assert.False(imp.IsCrewmate());
        Assert.True(imp.IsImpostor());
        Assert.False(imp.IsNeutral());

        Assert.False(neutral.IsCrewmate());
        Assert.False(neutral.IsImpostor());
        Assert.True(neutral.IsNeutral());

        Assert.True(vanilla.IsVanillaRole());
        Assert.False(crew.IsVanillaRole());
    }

    [Fact]
    public void Clone_CopiesColorAndCombinationOffsetInfo()
    {
        var original = new DummyCombGhostRole();
        original.OffsetInfo = new MultiAssignRoleBase.OptionOffsetInfo(CombinationRoleType.Kids, 10);

        var cloned = (DummyCombGhostRole)original.Clone();

        Assert.NotSame(original, cloned);
        Assert.Equal(original.Color, cloned.Color);
        Assert.NotNull(cloned.OffsetInfo);
        Assert.Equal(CombinationRoleType.Kids, cloned.OffsetInfo.RoleId);
        Assert.Equal(10, cloned.OffsetInfo.IdOffset);
    }

    [Fact]
    public void GetTargetRoleSeeColor_ImpostorAndOverloader_ReturnsExpectedColor()
    {
        var impGhost = new DummyGhostRole(ExtremeRoleType.Impostor, ExtremeGhostRoleId.Ventgeist);
        var crewGhost = new DummyGhostRole(ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Poltergeist);

        var impRole = new BountyHunter();
        var crewRole = new SpecialCrew();
        var overloader = new OverLoader();

        // Impostor ghost looking at Impostor role -> ImpostorRed
        Color seeImp = impGhost.GetTargetRoleSeeColor(1, impRole, null);
        Assert.Equal(Palette.ImpostorRed, seeImp);

        // Impostor ghost looking at Crewmate role -> clear
        Color seeCrew = impGhost.GetTargetRoleSeeColor(1, crewRole, null);
        Assert.Equal(Color.clear, seeCrew);

        // Crewmate ghost looking at Impostor role -> clear
        Color crewSeeImp = crewGhost.GetTargetRoleSeeColor(1, impRole, null);
        Assert.Equal(Color.clear, crewSeeImp);

        // Looking at overloader in Overload state -> ImpostorRed regardless of ghost role team
        overloader.IsOverLoad = true;
        Color seeOverload = crewGhost.GetTargetRoleSeeColor(1, overloader, null);
        Assert.Equal(Palette.ImpostorRed, seeOverload);
    }

    [Fact]
    public void SetGameControlId_UpdatesControlId()
    {
        var role = new DummyGhostRole(ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Poltergeist);
        Assert.Equal(0, role.GameControlId);

        role.SetGameControlId(42);

        Assert.Equal(0, role.GameControlId);
    }

    [Fact]
    public void ResetOnMeetingStartAndEnd_TriggersWithoutException()
    {
        var role = new DummyGhostRole(ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Poltergeist);

        role.ResetOnMeetingStart();
        role.ResetOnMeetingEnd();
    }
}
