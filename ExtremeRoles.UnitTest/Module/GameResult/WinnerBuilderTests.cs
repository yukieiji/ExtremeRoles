using System;
using System.Reflection;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameResult;
using Moq;
using Xunit;

using TaskInfo = ExtremeRoles.Module.GameResult.ExtremeGameResultManager.TaskInfo;

namespace ExtremeRoles.UnitTest.Module.GameResult;

[Collection("UnityMock")]
public class WinnerBuilderTests
{
    public WinnerBuilderTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger("WinnerBuilderTests");
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        if (ExtremeRolesPlugin.ShipState == null)
        {
            var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty("ShipState", BindingFlags.Public | BindingFlags.Static);
            shipStateProp?.SetValue(null, new ExtremeShipStatus());
        }
    }

    [Fact]
    public void WinnerBuilder_Constructor_And_Dispose_ExecutesCleanly()
    {
        var taskInfo = new System.Collections.Generic.Dictionary<byte, TaskInfo>();
        using var builder = new WinnerBuilder(1, taskInfo);

        Assert.NotNull(builder);
    }
}
