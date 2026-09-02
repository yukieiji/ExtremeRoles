using ExtremeRoles.GameMode.Factory;
using ExtremeRoles.GameMode.Logic.Usable;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode;

public class HideNSeekGameModeFactoryTests
{
    [Fact]
    public void CreateGlobalOption_ReturnsHideNSeekModeShipGlobalOption()
    {
        IModeFactory factory = new HideNSeekGameModeFactory();
        var option = factory.CreateGlobalOption();

        Assert.NotNull(option);
        Assert.IsType<HideNSeekModeShipGlobalOption>(option);
    }

    [Fact]
    public void CreateRoleSelector_ReturnsHideNSeekGameModeRoleSelector()
    {
        IModeFactory factory = new HideNSeekGameModeFactory();
        var selector = factory.CreateRoleSelector();

        Assert.NotNull(selector);
        Assert.IsType<HideNSeekGameModeRoleSelector>(selector);
    }

    [Fact]
    public void CreateLogicUsable_ReturnsHideNSeekModeUsableLogic()
    {
        IModeFactory factory = new HideNSeekGameModeFactory();
        var usable = factory.CreateLogicUsable();

        Assert.NotNull(usable);
        Assert.IsType<HideNSeekModeUsableLogic>(usable);
    }
}
