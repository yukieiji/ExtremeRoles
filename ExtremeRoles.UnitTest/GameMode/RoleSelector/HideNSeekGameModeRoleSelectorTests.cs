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