using ExtremeRoles.GameMode.Factory;
using ExtremeRoles.GameMode.Logic.Usable;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode.Factory;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class HideNSeekGameModeFactoryTests
{
    public HideNSeekGameModeFactoryTests()
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
    public void CreateGlobalOption_ReturnsHideNSeekModeShipGlobalOption()
    {
        // Arrange
        var factory = new HideNSeekGameModeFactory();

        // Act
        var result = factory.CreateGlobalOption();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<HideNSeekModeShipGlobalOption>(result);
    }

    [Fact]
    public void CreateRoleSelector_ReturnsHideNSeekGameModeRoleSelector()
    {
        // Arrange
        var factory = new HideNSeekGameModeFactory();

        // Act
        var result = factory.CreateRoleSelector();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<HideNSeekGameModeRoleSelector>(result);
    }

    [Fact]
    public void CreateLogicUsable_ReturnsHideNSeekModeUsableLogic()
    {
        // Arrange
        var factory = new HideNSeekGameModeFactory();

        // Act
        var result = factory.CreateLogicUsable();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<HideNSeekModeUsableLogic>(result);
    }
}
