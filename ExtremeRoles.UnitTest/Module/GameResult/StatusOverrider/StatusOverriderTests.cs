using System;
using System.Reflection;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult.StatusOverrider;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using UnityEngine;
using Xunit;

using PlayerStatus = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus;

namespace ExtremeRoles.UnitTest.Module.GameResult.StatusOverrider;

public class StatusOverriderTests
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

    private static NetworkedPlayerInfo CreateMockPlayerInfo(byte playerId, bool isDead = false, bool disconnected = false)
    {
        var mockPlayer = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
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
    public void AssassinAssassinateStatusOverrider_TargetPlayer_Dead_ReturnsDeadAssassinate()
    {
        byte marinId = 1;
        var overrider = new AssassinAssassinateStatusOverrider(marinId);
        var player = CreateMockPlayerInfo(marinId, isDead: true);
        var role = CreateMockRole(ExtremeRoleId.Marlin, ExtremeRoleType.Crewmate);

        bool result = overrider.TryGetOverride(role, null!, player, out var status);

        Assert.True(result);
        Assert.Equal(PlayerStatus.DeadAssassinate, status);
    }

    [Fact]
    public void AssassinAssassinateStatusOverrider_TargetPlayer_Alive_ReturnsAssassinate()
    {
        byte marinId = 1;
        var overrider = new AssassinAssassinateStatusOverrider(marinId);
        var player = CreateMockPlayerInfo(marinId, isDead: false);
        var role = CreateMockRole(ExtremeRoleId.Marlin, ExtremeRoleType.Crewmate);

        bool result = overrider.TryGetOverride(role, null!, player, out var status);

        Assert.True(result);
        Assert.Equal(PlayerStatus.Assassinate, status);
    }

    [Fact]
    public void AssassinAssassinateStatusOverrider_OtherPlayer_AliveCrewmate_ReturnsTrueAndSurrender()
    {
        byte marinId = 1;
        var overrider = new AssassinAssassinateStatusOverrider(marinId);
        var player = CreateMockPlayerInfo(2, isDead: false);
        var role = CreateMockRole(ExtremeRoleId.Investigator, ExtremeRoleType.Crewmate);

        bool result = overrider.TryGetOverride(role, null!, player, out var status);

        Assert.True(result);
        Assert.Equal(PlayerStatus.Surrender, status);
    }

    [Fact]
    public void AssassinAssassinateStatusOverrider_OtherPlayer_Impostor_ReturnsFalse()
    {
        byte marinId = 1;
        var overrider = new AssassinAssassinateStatusOverrider(marinId);
        var player = CreateMockPlayerInfo(2, isDead: false);
        var role = CreateMockRole(ExtremeRoleId.Assassin, ExtremeRoleType.Impostor);

        bool result = overrider.TryGetOverride(role, null!, player, out var status);

        Assert.False(result);
        Assert.Equal(PlayerStatus.Surrender, status);
    }

    [Fact]
    public void MonikaMeetingResultStatusOverrider_WinnerPlayer_ReturnsLoveYou()
    {
        var winner = CreateMockPlayerInfo(1);
        var notSelect = CreateMockPlayerInfo(2);
        var overrider = new MonikaMeetingResultStatusOverrider(winner, notSelect);
        var player = CreateMockPlayerInfo(1);
        var role = CreateMockRole(ExtremeRoleId.Investigator, ExtremeRoleType.Crewmate);

        bool result = overrider.TryGetOverride(role, null!, player, out var status);

        Assert.True(result);
        Assert.Equal(PlayerStatus.LoveYou, status);
    }

    [Fact]
    public void MonikaMeetingResultStatusOverrider_NotSelectPlayer_ReturnsDeadAssassinate()
    {
        var winner = CreateMockPlayerInfo(1);
        var notSelect = CreateMockPlayerInfo(2);
        var overrider = new MonikaMeetingResultStatusOverrider(winner, notSelect);
        var player = CreateMockPlayerInfo(2);
        var role = CreateMockRole(ExtremeRoleId.Investigator, ExtremeRoleType.Crewmate);

        bool result = overrider.TryGetOverride(role, null!, player, out var status);

        Assert.True(result);
        Assert.Equal(PlayerStatus.DeadAssassinate, status);
    }

    [Fact]
    public void MonikaMeetingResultStatusOverrider_OtherPlayer_ReturnsFalseAndAlive()
    {
        var winner = CreateMockPlayerInfo(1);
        var notSelect = CreateMockPlayerInfo(2);
        var overrider = new MonikaMeetingResultStatusOverrider(winner, notSelect);
        var player = CreateMockPlayerInfo(3);
        var role = CreateMockRole(ExtremeRoleId.Investigator, ExtremeRoleType.Crewmate);

        bool result = overrider.TryGetOverride(role, null!, player, out var status);

        Assert.False(result);
        Assert.Equal(PlayerStatus.Alive, status);
    }

    [Fact]
    public void UmbrerBiohazardStatusOverrider_UmbrerRole_ReturnsFalse()
    {
        var overrider = new UmbrerBiohazardStatusOverrider();
        var player = CreateMockPlayerInfo(1, isDead: false);
        var role = CreateMockRole(ExtremeRoleId.Umbrer, ExtremeRoleType.Neutral);

        bool result = overrider.TryGetOverride(role, null!, player, out var status);

        Assert.False(result);
        Assert.Equal(PlayerStatus.Zombied, status);
    }

    [Fact]
    public void UmbrerBiohazardStatusOverrider_OtherRole_Alive_ReturnsTrueAndZombied()
    {
        var overrider = new UmbrerBiohazardStatusOverrider();
        var player = CreateMockPlayerInfo(1, isDead: false);
        var role = CreateMockRole(ExtremeRoleId.Investigator, ExtremeRoleType.Crewmate);

        bool result = overrider.TryGetOverride(role, null!, player, out var status);

        Assert.True(result);
        Assert.Equal(PlayerStatus.Zombied, status);
    }

    [Fact]
    public void UmbrerBiohazardStatusOverrider_OtherRole_Dead_ReturnsFalse()
    {
        var overrider = new UmbrerBiohazardStatusOverrider();
        var player = CreateMockPlayerInfo(1, isDead: true);
        var role = CreateMockRole(ExtremeRoleId.Investigator, ExtremeRoleType.Crewmate);

        bool result = overrider.TryGetOverride(role, null!, player, out var status);

        Assert.False(result);
        Assert.Equal(PlayerStatus.Zombied, status);
    }
}
