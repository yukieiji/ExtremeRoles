using ExtremeRoles.UnitTest.Mocks;
using System;
using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using UnityEngine;
using Xunit;

using DeadInfo = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.DeadInfo;
using TaskInfo = ExtremeRoles.Module.GameResult.ExtremeGameResultManager.TaskInfo;

namespace ExtremeRoles.UnitTest.Module.GameResult;

public class WinnerInitializerTests : SerialTestBase, IClassFixture<UnityCommonMock>
{
    private sealed class DummySingleRole : SingleRoleBase
    {
        public DummySingleRole(RoleCore core)
        {
            var field = typeof(SingleRoleBase).GetField("<Core>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(this, core);
        }

        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) { }
        protected override void RoleSpecificInit() { }
    }

    public WinnerInitializerTests(SerialFixture fixture, UnityCommonMock unityCommonMock)
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
    public void WinnerInitializer_Constructor_And_Dispose_ExecutesCleanly()
    {
        var taskInfo = new System.Collections.Generic.Dictionary<byte, TaskInfo>();
        using var summaryBuilder = new PlayerSummaryBuilder((GameOverReason)0, new System.Collections.Generic.Dictionary<byte, DeadInfo>(), taskInfo);
        using var initializer = new WinnerInitializer(summaryBuilder);

        Assert.NotNull(initializer);
    }
}