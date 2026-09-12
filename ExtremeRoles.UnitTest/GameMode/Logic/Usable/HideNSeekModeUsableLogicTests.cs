using ExtremeRoles.GameMode.Logic.Usable;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Helper;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode.Logic.Usable;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class HideNSeekModeUsableLogicTests
{
    private sealed class DummySingleRole : SingleRoleBase
    {
        public DummySingleRole(RoleCore core)
            : base(new RoleArgs(core, RoleProp.None))
        {
        }

        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) { }
        protected override void RoleSpecificInit() { }
    }

    public HideNSeekModeUsableLogicTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void CanUseVent_CrewmateRole_ReturnsTrue()
    {
        // Arrange
        var logic = new HideNSeekModeUsableLogic();
        var core = new RoleCore(ExtremeRoleId.SpecialCrew, ExtremeRoleType.Crewmate, Color.white, "TestCrew");
        var role = new DummySingleRole(core);

        // Act
        var canUseVent = logic.CanUseVent(role);

        // Assert
        Assert.True(canUseVent);
    }

    [Fact]
    public void CanUseVent_ImpostorRole_ReturnsFalse()
    {
        // Arrange
        var logic = new HideNSeekModeUsableLogic();
        var core = new RoleCore(ExtremeRoleId.SpecialImpostor, ExtremeRoleType.Impostor, Color.red, "TestImp");
        var role = new DummySingleRole(core);

        // Act
        var canUseVent = logic.CanUseVent(role);

        // Assert
        Assert.False(canUseVent);
    }
}
