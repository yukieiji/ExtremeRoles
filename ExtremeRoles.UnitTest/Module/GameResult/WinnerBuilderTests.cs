using ExtremeRoles.UnitTest.Mocks;
using System;
using System.Reflection;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameResult;
using Moq;
using Xunit;

using TaskInfo = ExtremeRoles.Module.GameResult.ExtremeGameResultManager.TaskInfo;

namespace ExtremeRoles.UnitTest.Module.GameResult;

public class WinnerBuilderTests : SerialTestBase, IClassFixture<UnityCommonMock>
{
    public WinnerBuilderTests(SerialFixture fixture, UnityCommonMock unityCommonMock)
        : base(fixture, unityCommonMock.OperatorsMock, unityCommonMock.Vector2Mock, unityCommonMock.ColorMock, unityCommonMock.MathfMock, new PaletteMock(), new GameOptionsManagerMock(), new CompatModManagerMock(), unityCommonMock.TimeMock, new LoggerMock())
    {
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