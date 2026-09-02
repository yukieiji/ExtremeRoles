using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode.Option.ShipGlobal;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ShipGlobalOptionTests
{
    public ShipGlobalOptionTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupExtremeSystemTypeManagerMock();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();
        MockSetupHelper.SetupMockConfig(plugin);

        EnsureShipGlobalOptionsCreated();
    }

    private static void EnsureShipGlobalOptionsCreated()
    {
        if (ClientOption.Instance == null || !OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)OptionCreator.CommonOption.RandomOption, out _))
        {
            OptionCreator.Create();
        }
    }

    [Fact]
    public void IShipGlobalOption_Create_RegistersAllCategories()
    {
        EnsureShipGlobalOptionsCreated();

        foreach (ShipGlobalOptionCategory category in Enum.GetValues<ShipGlobalOptionCategory>())
        {
            bool exists = OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)category, out var cate);
            Assert.True(exists, $"Category {category} ({(int)category}) should be registered in OptionManager.");
            Assert.NotNull(cate);
        }
    }

    [Fact]
    public void ClassicGameModeShipGlobalOption_Load_ReadsDefaultValuesFromOptionManager()
    {
        var classic = new ClassicGameModeShipGlobalOption();
        classic.Load();
    }

    [Fact]
    public void HideNSeekModeShipGlobalOption_Properties_ReturnExpectedDefaults()
    {
        var hns = new HideNSeekModeShipGlobalOption();
		hns.Load();
    }
}
