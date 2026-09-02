using System.Reflection;
using ExtremeRoles.GameMode.Logic.Usable;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode;

public class HideNSeekModeUsableLogicTests
{
    private sealed class DummySingleRole : SingleRoleBase
    {
        public DummySingleRole(ExtremeRoleId roleId, ExtremeRoleType team)
        {
            var core = new RoleCore(roleId, team, Color.white, roleId.ToString());
            var field = typeof(SingleRoleBase).GetField("<Core>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(this, core);
        }

        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) { }
        protected override void RoleSpecificInit() { }

        public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
        public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => Color.white;
    }

    [Fact]
    public void CanUseVent_NonImpostorRole_ReturnsTrue()
    {
        var usableLogic = new HideNSeekModeUsableLogic();

        var crewRole = new DummySingleRole(ExtremeRoleId.SpecialCrew, ExtremeRoleType.Crewmate);
        var neutralRole = new DummySingleRole(ExtremeRoleId.Jester, ExtremeRoleType.Neutral);

        Assert.True(usableLogic.CanUseVent(crewRole));
        Assert.True(usableLogic.CanUseVent(neutralRole));
    }

    [Fact]
    public void CanUseVent_ImpostorRole_ReturnsFalse()
    {
        var usableLogic = new HideNSeekModeUsableLogic();

        var impRole = new DummySingleRole(ExtremeRoleId.SpecialImpostor, ExtremeRoleType.Impostor);

        Assert.False(usableLogic.CanUseVent(impRole));
    }
}
