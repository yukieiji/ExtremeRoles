using System;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Interfaces;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode.RoleSelector;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class LiberalDefaultOptionLoaderTests
{
    public LiberalDefaultOptionLoaderTests()
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
    public void Constructor_WhenCategoryExists_LoadsOptionsAndProperties()
    {
        // Act
        var loader = new LiberalDefaultOptionLoader();

        // Assert
        Assert.NotNull(loader.GlobalOption);
        Assert.NotEmpty(loader.GlobalOption);
        Assert.NotNull(loader.LeaderOption);
        Assert.NotEmpty(loader.LeaderOption);
        Assert.NotNull(loader.MilitantOption);
        Assert.NotEmpty(loader.MilitantOption);
    }

    [Fact]
    public void RoleSpawnSetting_ReturnsFormattedString()
    {
        // Arrange
        var loader = new LiberalDefaultOptionLoader();

        // Act
        string roleSpawnSetting = loader.RoleSpawnSetting;

        // Assert
        Assert.NotNull(roleSpawnSetting);
        Assert.NotEmpty(roleSpawnSetting);
    }

    [Fact]
    public void Get_ByIdAndEnum_ReturnsOption()
    {
        // Arrange
        var loader = new LiberalDefaultOptionLoader();

        // Act
        var optionByEnum = loader.Get(LiberalGlobalSetting.WinMoney);
        var optionById = loader.Get((int)LiberalGlobalSetting.WinMoney);

        // Assert
        Assert.NotNull(optionByEnum);
        Assert.NotNull(optionById);
        Assert.Same(optionByEnum, optionById);
    }

    [Fact]
    public void GetValue_ByEnumAndId_ReturnsValue()
    {
        // Arrange
        var loader = new LiberalDefaultOptionLoader();

        // Act
        int winMoneyByEnum = loader.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.WinMoney);
        int winMoneyById = loader.GetValue<int>((int)LiberalGlobalSetting.WinMoney);

        // Assert
        Assert.Equal(100, winMoneyByEnum);
        Assert.Equal(100, winMoneyById);
    }

    [Fact]
    public void TryGet_ByEnumAndId_ReturnsTrueAndOption()
    {
        // Arrange
        var loader = new LiberalDefaultOptionLoader();

        // Act
        bool tryGetEnum = loader.TryGet(LiberalGlobalSetting.WinMoney, out IOption? optionEnum);
        bool tryGetId = loader.TryGet((int)LiberalGlobalSetting.WinMoney, out IOption? optionId);

        // Assert
        Assert.True(tryGetEnum);
        Assert.NotNull(optionEnum);
        Assert.True(tryGetId);
        Assert.NotNull(optionId);
    }

    [Fact]
    public void TryGetValue_ByEnumAndId_ReturnsTrueAndValue()
    {
        // Arrange
        var loader = new LiberalDefaultOptionLoader();

        // Act
        bool tryGetValueEnum = loader.TryGetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.WinMoney, out int valEnum);
        bool tryGetValueId = loader.TryGetValue<int>((int)LiberalGlobalSetting.WinMoney, out int valId);

        // Assert
        Assert.True(tryGetValueEnum);
        Assert.Equal(100, valEnum);
        Assert.True(tryGetValueId);
        Assert.Equal(100, valId);
    }
}
