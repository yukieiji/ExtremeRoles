using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Neutral.Yandere;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SpecialWinChecker;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class YandereWinCheckerTests
{
    private static YandereRole CreateYandereRole(PlayerControl? loverPlayer)
    {
        var yandere = (YandereRole)RuntimeHelpers.GetUninitializedObject(typeof(YandereRole));
        var backing = typeof(YandereRole).GetField("<OneSidedLover>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backing?.SetValue(yandere, loverPlayer);
        return yandere;
    }

    private sealed class DummySingleRole : SingleRoleBase
    {
        public DummySingleRole(ExtremeRoleId roleId, ExtremeRoleType team, bool canKill = false)
        {
            var core = new RoleCore(roleId, team, Color.white, roleId.ToString());
            var field = typeof(SingleRoleBase).GetField("<Core>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(this, core);

            var canKillField = typeof(SingleRoleBase).GetField("<CanKillRole>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            canKillField?.SetValue(this, canKill);
        }

        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) { }
        protected override void RoleSpecificInit() { }
    }

    public YandereWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupGameDataMock();
        SetupAmongUsClientAndShipState();
    }

    private static void SetupAmongUsClientAndShipState()
    {
        MockSetupHelper.SetupMockExtremeRolePlugin();
        if (ExtremeRolesPlugin.ShipState == null)
        {
            var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(nameof(ExtremeRolesPlugin.ShipState), BindingFlags.Public | BindingFlags.Static);
            shipStateProp?.SetValue(null, new ExtremeShipStatus());
        }

        var mockClient = MockSetupHelper.SetupAmongUsClientMock();
        var mockWriter = new Mock<Hazel.MessageWriter>(IntPtr.Zero);
        mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<Hazel.SendOption>(), It.IsAny<int>())).Returns(mockWriter.Object);

        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.NetId).Returns(1u);
    }

    [Fact]
    public void IsWin_NoYandere_ReturnsFalse()
    {
        var checker = new YandereWinChecker();
        var mockStats = new Mock<IPlayerStatistics>();

        Assert.False(checker.IsWin(mockStats.Object));
    }

    [Fact]
    public void IsWin_OneSidedLoverNullOrDead_ReturnsFalse()
    {
        var checker = new YandereWinChecker();
        var yandereRole = CreateYandereRole(null);
        checker.AddAliveRole(1, yandereRole);

        var mockStats = new Mock<IPlayerStatistics>();
        Assert.False(checker.IsWin(mockStats.Object));
    }

    [Fact]
    public void IsWin_TooManyImpostors_ReturnsFalse()
    {
        ExtremeRoleManager.GameRole.Clear();

        byte yandereId = 1;
        byte loverId = 2;

        var mockLoverInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockLoverInfo.SetupGet(p => p.PlayerId).Returns(loverId);
        mockLoverInfo.SetupGet(p => p.IsDead).Returns(false);

        var mockGameData = MockSetupHelper.SetupGameDataMock();
        mockGameData.Setup(g => g.GetPlayerById(loverId)).Returns(mockLoverInfo.Object);

        var mockLoverPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockLoverPlayer.SetupGet(p => p.PlayerId).Returns(loverId);
        mockLoverPlayer.SetupGet(p => p.Data).Returns(mockLoverInfo.Object);

        var yandereRole = CreateYandereRole(mockLoverPlayer.Object);
        var loverRole = new DummySingleRole(ExtremeRoleId.Sheriff, ExtremeRoleType.Crewmate);

        ExtremeRoleManager.GameRole[yandereId] = yandereRole;
        ExtremeRoleManager.GameRole[loverId] = loverRole;

        var checker = new YandereWinChecker();
        checker.AddAliveRole(yandereId, yandereRole);

        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TotalAlive).Returns(4);
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(2); // 2 Impostors alive, 0 assassin, 0 lover imp -> 2 > 0 -> false
        mockStats.SetupGet(s => s.AssassinAlive).Returns(0);
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>());
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(0);

        Assert.False(checker.IsWin(mockStats.Object));
    }
}
