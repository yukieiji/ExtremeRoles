using ExtremeRoles.GameMode.Factory;
using ExtremeRoles.GameMode.Logic.Usable;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode;

public class ClassicGameModeFactoryTests
{
    [Fact]
    public void CreateGlobalOption_ReturnsClassicGameModeShipGlobalOption()
    {
        IModeFactory factory = new ClassicGameModeFactory();
        var option = factory.CreateGlobalOption();

        Assert.NotNull(option);
        Assert.IsType<ClassicGameModeShipGlobalOption>(option);
    }

    [Fact]
    public void CreateRoleSelector_ReturnsClassicGameModeRoleSelector()
    {
        IModeFactory factory = new ClassicGameModeFactory();
        var selector = factory.CreateRoleSelector();

        Assert.NotNull(selector);
        Assert.IsType<ClassicGameModeRoleSelector>(selector);
    }

    [Fact]
    public void CreateLogicUsable_ReturnsClassicGameModeUsableLogic()
    {
        IModeFactory factory = new ClassicGameModeFactory();
        var usable = factory.CreateLogicUsable();

        Assert.NotNull(usable);
        Assert.IsType<ClassicGameModeUsableLogic>(usable);
    }
}
