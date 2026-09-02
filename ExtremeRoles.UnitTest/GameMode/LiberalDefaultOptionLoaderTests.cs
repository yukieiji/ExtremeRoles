using System;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode;

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

        EnsureGlobalOptionsCreated();
    }

    private static void EnsureGlobalOptionsCreated()
    {
        if (ClientOption.Instance == null || !OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)SpawnOptionCategory.LiberalSetting, out _))
        {
            OptionCreator.Create();
        }
    }

    [Fact]
    public void Constructor_InitializesOptionListsAndReadsCategory()
    {
        EnsureGlobalOptionsCreated();

        var loader = new LiberalDefaultOptionLoader();

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
        EnsureGlobalOptionsCreated();

        var loader = new LiberalDefaultOptionLoader();
        string setting = loader.RoleSpawnSetting;

        Assert.NotNull(setting);
        Assert.Contains("liberalRoles", setting);
    }

    [Fact]
    public void GetAndGetValue_ReturnExpectedOptionAndValue()
    {
        EnsureGlobalOptionsCreated();

        var loader = new LiberalDefaultOptionLoader();

        var option = loader.Get(LiberalGlobalSetting.WinMoney);
        Assert.NotNull(option);

        var enumOption = loader.Get(LiberalGlobalSetting.WinMoney);
        Assert.NotNull(enumOption);

        int winMoney = loader.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.WinMoney);
        Assert.Equal(100, winMoney);

        int winMoneyInt = loader.GetValue<int>((int)LiberalGlobalSetting.WinMoney);
        Assert.Equal(100, winMoneyInt);
    }

    [Fact]
    public void TryGetAndTryGetValue_ReturnTrueForValidSettings()
    {
        EnsureGlobalOptionsCreated();

        var loader = new LiberalDefaultOptionLoader();

        bool tryGetResult = loader.TryGet((int)LiberalGlobalSetting.KillMoney, out var option);
        Assert.True(tryGetResult);
        Assert.NotNull(option);

        bool tryGetEnumResult = loader.TryGet(LiberalGlobalSetting.KillMoney, out var enumOption);
        Assert.True(tryGetEnumResult);
        Assert.NotNull(enumOption);

        bool tryGetValueResult = loader.TryGetValue<int>((int)LiberalGlobalSetting.KillMoney, out int killMoney);
        Assert.True(tryGetValueResult);
        Assert.Equal(10, killMoney);

        bool tryGetValueEnumResult = loader.TryGetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.KillMoney, out int killMoneyEnum);
        Assert.True(tryGetValueEnumResult);
        Assert.Equal(10, killMoneyEnum);
    }
}
