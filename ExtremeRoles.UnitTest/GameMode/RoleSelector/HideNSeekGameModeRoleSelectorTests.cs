using System.Linq;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode.RoleSelector;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class HideNSeekGameModeRoleSelectorTests
{
    public HideNSeekGameModeRoleSelectorTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupExtremeSystemTypeManagerMock();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();
        MockSetupHelper.SetupMockConfig(plugin);

        if (ClientOption.Instance == null || !OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)OptionCreator.CommonOption.RandomOption, out _))
        {
            OptionCreator.Create();
        }
    }

    [Fact]
    public void Properties_ReturnExpectedDefaults()
    {
        // Arrange & Act
        var selector = new HideNSeekGameModeRoleSelector();

        // Assert
        Assert.True(selector.IsAdjustImpostorNum);
        Assert.True(selector.CanUseXion);
        Assert.True(selector.IsVanillaRoleToMultiAssign);
    }

    [Fact]
    public void Enumerations_ReturnExpectedLists()
    {
        // Arrange
        var selector = new HideNSeekGameModeRoleSelector();

        // Act
        var normalRoles = selector.UseNormalRoleId.ToList();
        var combRoles = selector.UseCombRoleType.ToList();
        var ghostRoles = selector.UseGhostRoleId.ToList();

        // Assert
        Assert.NotEmpty(normalRoles);
        Assert.Contains(ExtremeRoleId.SpecialCrew, normalRoles);

        Assert.NotEmpty(combRoles);
        Assert.Contains(CombinationRoleType.Accelerator, combRoles);

        Assert.Empty(ghostRoles);
    }

    [Fact]
    public void IsValidCategory_ValidAndInvalidCategoryIds_ReturnsExpected()
    {
        // Arrange
        var selector = new HideNSeekGameModeRoleSelector();
        int validGroupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.SpecialCrew);

        // Act
        bool validResult = selector.IsValidCategory(validGroupId);
        bool invalidResult = selector.IsValidCategory(-9999);

        // Assert
        Assert.True(validResult);
        Assert.False(invalidResult);
    }
}
