using System.Linq;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode;

public class HideNSeekGameModeRoleSelectorTests
{
    [Fact]
    public void Properties_ReturnExpectedDefaults()
    {
        var selector = new HideNSeekGameModeRoleSelector();

        Assert.True(selector.IsAdjustImpostorNum);
        Assert.True(selector.CanUseXion);
        Assert.True(selector.IsVanillaRoleToMultiAssign);
    }

    [Fact]
    public void UseNormalRoleId_ContainsExpectedRoles()
    {
        var selector = new HideNSeekGameModeRoleSelector();
        var normalRoles = selector.UseNormalRoleId.ToList();

        Assert.NotEmpty(normalRoles);
        Assert.Contains(ExtremeRoleId.SpecialCrew, normalRoles);
        Assert.Contains(ExtremeRoleId.BountyHunter, normalRoles);
        Assert.DoesNotContain(ExtremeRoleId.Sheriff, normalRoles);
    }

    [Fact]
    public void UseCombRoleType_ContainsAcceleratorOnly()
    {
        var selector = new HideNSeekGameModeRoleSelector();
        var combRoles = selector.UseCombRoleType.ToList();

        Assert.Single(combRoles);
        Assert.Contains(CombinationRoleType.Accelerator, combRoles);
    }

    [Fact]
    public void UseGhostRoleId_IsEmpty()
    {
        var selector = new HideNSeekGameModeRoleSelector();
        var ghostRoles = selector.UseGhostRoleId.ToList();

        Assert.Empty(ghostRoles);
    }

    [Fact]
    public void IsValidCategory_ValidAndInvalidCategoryIds_ReturnsExpectedResult()
    {
        var selector = new HideNSeekGameModeRoleSelector();

        int validCategoryId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.SpecialCrew);
        Assert.True(selector.IsValidCategory(validCategoryId));

        int invalidCategoryId = -99999;
        Assert.False(selector.IsValidCategory(invalidCategoryId));
    }
}
