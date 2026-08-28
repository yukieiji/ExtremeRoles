using System;
using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using UnityEngine;
using Xunit;

using DeadInfo = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.DeadInfo;
using PlayerStatus = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus;
using TaskInfo = ExtremeRoles.Module.GameResult.ExtremeGameResultManager.TaskInfo;

namespace ExtremeRoles.UnitTest.Module.GameResult;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
[Collection(nameof(MockSetupHelper.SetupLogger))]
public class PlayerSummaryBuilderTests
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

    public PlayerSummaryBuilderTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupLogger("PlayerSummaryBuilderTests");
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
    }

    private static NetworkedPlayerInfo CreateMockPlayerInfo(byte playerId, string name = "Player", bool isDead = false, bool disconnected = false)
    {
        var mockPlayer = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
        mockPlayer.SetupGet(p => p.PlayerName).Returns($"{name}{playerId}");
        mockPlayer.SetupGet(p => p.IsDead).Returns(isDead);
        mockPlayer.SetupGet(p => p.Disconnected).Returns(disconnected);
        return mockPlayer.Object;
    }

    private static SingleRoleBase CreateMockRole(ExtremeRoleId roleId, ExtremeRoleType team)
    {
        var core = new RoleCore(roleId, team, Color.white, "TestRole");
        return new DummySingleRole(core);
    }

    [Fact]
    public void Create_PlayerNotInTaskInfo_ReturnsNull()
    {
        using var builder = new PlayerSummaryBuilder((GameOverReason)0, new Dictionary<byte, DeadInfo>(), new Dictionary<byte, TaskInfo>());
        var player = CreateMockPlayerInfo(1);
        var role = CreateMockRole(ExtremeRoleId.Investigator, ExtremeRoleType.Crewmate);

        var summary = builder.Create(player, role, null);

        Assert.Null(summary);
    }

    [Fact]
    public void Create_AlivePlayer_ReturnsAliveSummary()
    {
        var taskInfo = new Dictionary<byte, TaskInfo> { [1] = new TaskInfo(3, 5) };
        using var builder = new PlayerSummaryBuilder((GameOverReason)0, new Dictionary<byte, DeadInfo>(), taskInfo);
        var player = CreateMockPlayerInfo(1, isDead: false);
        var role = CreateMockRole(ExtremeRoleId.Investigator, ExtremeRoleType.Crewmate);

        var summary = builder.Create(player, role, null);

        Assert.NotNull(summary);
        Assert.Equal(1, summary.Value.PlayerId);
        Assert.Equal(PlayerStatus.Alive, summary.Value.StatusInfo);
        Assert.Equal(3, summary.Value.CompletedTask);
        Assert.Equal(5, summary.Value.TotalTask);
    }

    [Fact]
    public void Create_DeadPlayer_ReturnsDeadInfoStatus()
    {
        var deadInfo = new Dictionary<byte, DeadInfo> { [1] = new DeadInfo(PlayerStatus.DeadAssassinate, DateTime.Now, null!) };
        var taskInfo = new Dictionary<byte, TaskInfo> { [1] = new TaskInfo(2, 5) };
        using var builder = new PlayerSummaryBuilder((GameOverReason)0, deadInfo, taskInfo);
        var player = CreateMockPlayerInfo(1, isDead: true);
        var role = CreateMockRole(ExtremeRoleId.Investigator, ExtremeRoleType.Crewmate);

        var summary = builder.Create(player, role, null);

        Assert.NotNull(summary);
        Assert.Equal(PlayerStatus.DeadAssassinate, summary.Value.StatusInfo);
    }

    [Fact]
    public void Create_CrewmatesByTask_ForceReplaceTaskNumToTotal()
    {
        var taskInfo = new Dictionary<byte, TaskInfo> { [1] = new TaskInfo(2, 5) };
        using var builder = new PlayerSummaryBuilder(GameOverReason.CrewmatesByTask, new Dictionary<byte, DeadInfo>(), taskInfo);
        var player = CreateMockPlayerInfo(1, isDead: false);
        var role = CreateMockRole(ExtremeRoleId.Investigator, ExtremeRoleType.Crewmate);

        var summary = builder.Create(player, role, null);

        Assert.NotNull(summary);
        Assert.Equal(5, summary.Value.CompletedTask);
    }
}
