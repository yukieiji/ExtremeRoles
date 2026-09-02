using System.Reflection;
using ExtremeRoles.GameMode.Logic.Usable;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode;

public class ClassicGameModeUsableLogicTests
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

    [Theory]
    [InlineData(ExtremeRoleType.Crewmate)]
    [InlineData(ExtremeRoleType.Impostor)]
    [InlineData(ExtremeRoleType.Neutral)]
    public void CanUseVent_ReturnsTrueForAnyRoleType(ExtremeRoleType team)
    {
        var usableLogic = new ClassicGameModeUsableLogic();
        var dummyRole = new DummySingleRole(ExtremeRoleId.SpecialCrew, team);

        bool result = usableLogic.CanUseVent(dummyRole);

        Assert.True(result);
    }
}
