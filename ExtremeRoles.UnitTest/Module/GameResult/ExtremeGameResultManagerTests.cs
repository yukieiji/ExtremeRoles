using System;
using System.Reflection;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameResult;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameResult;

[Collection("UnityMock")]
public class ExtremeGameResultManagerTests
{
    public ExtremeGameResultManagerTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger("ExtremeGameResultManagerTests");
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        if (ExtremeRolesPlugin.ShipState == null)
        {
            var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty("ShipState", BindingFlags.Public | BindingFlags.Static);
            shipStateProp?.SetValue(null, new ExtremeShipStatus());
        }
    }

    [Fact]
    public void Manager_Constructor_InitializesEmptyWinnerAndPlayerSummaries()
    {
        var manager = new ExtremeGameResultManager();

        Assert.Empty(manager.PlayerSummaries);
        var winner = manager.Winner;
        Assert.Empty(winner.Winner);
    }
}
