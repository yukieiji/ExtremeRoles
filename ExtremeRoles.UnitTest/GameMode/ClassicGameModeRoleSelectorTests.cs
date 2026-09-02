using System.Linq;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode;

public class ClassicGameModeRoleSelectorTests
{
    [Fact]
    public void Properties_ReturnExpectedDefaults()
    {
        var selector = new ClassicGameModeRoleSelector();

        Assert.False(selector.IsAdjustImpostorNum);
        Assert.True(selector.CanUseXion);
        Assert.False(selector.IsVanillaRoleToMultiAssign);
    }

    [Fact]
    public void UseNormalRoleId_ContainsExpectedRoles()
    {
        var selector = new ClassicGameModeRoleSelector();
        var normalRoles = selector.UseNormalRoleId.ToList();

        Assert.NotEmpty(normalRoles);
        Assert.Contains(ExtremeRoleId.SpecialCrew, normalRoles);
        Assert.Contains(ExtremeRoleId.Sheriff, normalRoles);
        Assert.Contains(ExtremeRoleId.SpecialImpostor, normalRoles);
        Assert.Contains(ExtremeRoleId.Jackal, normalRoles);
    }

    [Fact]
    public void UseCombRoleType_ContainsExpectedCombinationRoles()
    {
        var selector = new ClassicGameModeRoleSelector();
        var combRoles = selector.UseCombRoleType.ToList();

        Assert.NotEmpty(combRoles);
        Assert.Contains(CombinationRoleType.Avalon, combRoles);
        Assert.Contains(CombinationRoleType.Lover, combRoles);
    }

    [Fact]
    public void UseGhostRoleId_ContainsExpectedGhostRoles()
    {
        var selector = new ClassicGameModeRoleSelector();
        var ghostRoles = selector.UseGhostRoleId.ToList();

        Assert.NotEmpty(ghostRoles);
        Assert.Contains(ExtremeGhostRoleId.Poltergeist, ghostRoles);
        Assert.Contains(ExtremeGhostRoleId.Faunus, ghostRoles);
    }

    [Fact]
    public void IsValidCategory_ValidAndInvalidCategoryIds_ReturnsExpectedResult()
    {
        var selector = new ClassicGameModeRoleSelector();

        int validCategoryId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.SpecialCrew);
        Assert.True(selector.IsValidCategory(validCategoryId));

        int invalidCategoryId = -99999;
        Assert.False(selector.IsValidCategory(invalidCategoryId));
    }
}
