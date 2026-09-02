using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode.RoleSelector;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class LiberalOptionTests
{
    public LiberalOptionTests()
    {
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
    public void LiberalOption_Create_RegistersOptionsAndSubscribesToValueChangedEvents()
    {
        // Arrange
        int otherCategoryId = 8887;
        using (var otherFactory = OptionCategoryAssembler.CreateOptionCategory(otherCategoryId, "OtherCategory", OptionTab.GeneralTab, Color.white))
        {
            var mockMaxSetting = otherFactory.CreateIntOption(RoleSpawnOption.MaxLiberal, 2, 0, 10, 1);

            int tempCategoryId = 8888;
            using (var factory = OptionCategoryAssembler.CreateOptionCategory(tempCategoryId, "TestLiberalCategory", OptionTab.GeneralTab, Color.white))
            {
                // Act
                LiberalOption.Create(factory, mockMaxSetting);
            }

            // Assert
            bool categoryExists = OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, tempCategoryId, out var category);
            Assert.True(categoryExists);
            Assert.NotNull(category);

            bool winMoneyExists = category.TryGet((int)LiberalGlobalSetting.WinMoney, out var winMoneyOption);
            Assert.True(winMoneyExists);
            Assert.NotNull(winMoneyOption);

            // Act: trigger OnValueChanged on mockMaxSetting
            mockMaxSetting.Selection = 3;

            // Assert: no exception thrown and value updated
            Assert.Equal(3, mockMaxSetting.Value<int>());
        }
    }
}
