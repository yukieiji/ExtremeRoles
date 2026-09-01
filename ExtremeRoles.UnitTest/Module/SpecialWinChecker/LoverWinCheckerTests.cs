using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SpecialWinChecker;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class LoverWinCheckerTests
{
    private sealed class DummySingleRole : SingleRoleBase
    {
        public DummySingleRole(ExtremeRoleId roleId, ExtremeRoleType team, bool hasTask = true)
        {
            var core = new RoleCore(roleId, team, Color.white, roleId.ToString());
            var field = typeof(SingleRoleBase).GetField("<Core>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(this, core);

            var taskField = typeof(SingleRoleBase).GetField("<HasTask>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            taskField?.SetValue(this, hasTask);
        }

        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) { }
        protected override void RoleSpecificInit() { }
    }

    public LoverWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void IsWin_NoAliveLover_ReturnsFalse()
    {
        var checker = new LoverWinChecker();
        var mockStats = new Moq.Mock<IPlayerStatistics>();

        // aliveLover is empty
        Assert.False(checker.IsWin(mockStats.Object));
    }

    [Fact]
    public void IsWin_SingleTeamCrewmate_ReturnsFalse()
    {
        var checker = new LoverWinChecker();

        var mockRole1 = new DummySingleRole(ExtremeRoleId.Sheriff, ExtremeRoleType.Crewmate, true);
        var mockRole2 = new DummySingleRole(ExtremeRoleId.Investigator, ExtremeRoleType.Crewmate, true);

        checker.AddAliveRole(1, mockRole1);
        checker.AddAliveRole(2, mockRole2);

        var mockStats = new Moq.Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TotalAlive).Returns(2);

        Assert.False(checker.IsWin(mockStats.Object));
    }

    [Fact]
    public void IsWin_SingleTeamImpostor_ReturnsFalse()
    {
        var checker = new LoverWinChecker();

        var mockRole1 = new DummySingleRole(ExtremeRoleId.Assassin, ExtremeRoleType.Impostor, false);
        var mockRole2 = new DummySingleRole(ExtremeRoleId.Bait, ExtremeRoleType.Impostor, false);

        checker.AddAliveRole(1, mockRole1);
        checker.AddAliveRole(2, mockRole2);

        var mockStats = new Moq.Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TotalAlive).Returns(2);

        Assert.False(checker.IsWin(mockStats.Object));
    }

    [Fact]
    public void IsWin_AliveNumLessThanOthersAlive_ReturnsFalse()
    {
        var checker = new LoverWinChecker();

        var mockRole1 = new DummySingleRole(ExtremeRoleId.Sheriff, ExtremeRoleType.Crewmate, true);
        var mockRole2 = new DummySingleRole(ExtremeRoleId.Jackal, ExtremeRoleType.Neutral, true);

        checker.AddAliveRole(1, mockRole1);
        checker.AddAliveRole(2, mockRole2);

        var mockStats = new Moq.Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TotalAlive).Returns(5); // totalAlive - aliveNum = 5 - 2 = 3. aliveNum (2) < 3 -> false

        Assert.False(checker.IsWin(mockStats.Object));
    }
}
