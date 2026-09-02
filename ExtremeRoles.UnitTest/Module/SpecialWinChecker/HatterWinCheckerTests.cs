using System.Collections.Generic;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SpecialWinChecker;

public sealed class HatterWinCheckerTests
{
    [Fact]
    public void IsWin_EqualOtherAndKillerAndHatter_ReturnsTrue()
    {
        var checker = new HatterWinChecker();
        var mockRole = new Mock<SingleRoleBase>();
        checker.AddAliveRole(1, mockRole.Object); // hatterAliveNum = 1

        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(1);
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>());
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(0);
        mockStats.SetupGet(s => s.TotalAlive).Returns(3); // killer = 1, hatter = 1, other = 3 - 1 - 1 = 1 (1 == 1 && 1 == 1)

        Assert.True(checker.IsWin(mockStats.Object));
    }

    [Fact]
    public void IsWin_KillerCountIncludesNeutralAndLiberal_CalculatesCorrectly()
    {
        var checker = new HatterWinChecker();
        var mockRole = new Mock<SingleRoleBase>();
        checker.AddAliveRole(1, mockRole.Object);
        checker.AddAliveRole(2, mockRole.Object); // hatterAliveNum = 2

        var neutralDict = new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>
        {
            { new NeutralSeparateTeamContainer.NeutralTeam(NeutralSeparateTeam.Jackal, 1), 1 }
        };

        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(0);
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(neutralDict);
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(1); // killer = 0 + 1 + 1 = 2
        mockStats.SetupGet(s => s.TotalAlive).Returns(6); // other = 6 - 2 - 2 = 2 (2 == 2 && 2 == 2)

        Assert.True(checker.IsWin(mockStats.Object));
    }

    [Fact]
    public void IsWin_OtherPlayerNumNotEqualsKiller_ReturnsFalse()
    {
        var checker = new HatterWinChecker();
        var mockRole = new Mock<SingleRoleBase>();
        checker.AddAliveRole(1, mockRole.Object); // hatterAliveNum = 1

        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(2); // killer = 2
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>());
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(0);
        mockStats.SetupGet(s => s.TotalAlive).Returns(4); // other = 4 - 2 - 1 = 1 (other 1 != killer 2)

        Assert.False(checker.IsWin(mockStats.Object));
    }

    [Fact]
    public void IsWin_OtherPlayerNumNotEqualsHatter_ReturnsFalse()
    {
        var checker = new HatterWinChecker();
        var mockRole = new Mock<SingleRoleBase>();
        checker.AddAliveRole(1, mockRole.Object);
        checker.AddAliveRole(2, mockRole.Object); // hatterAliveNum = 2

        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(1); // killer = 1
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>());
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(0);
        mockStats.SetupGet(s => s.TotalAlive).Returns(4); // other = 4 - 1 - 2 = 1 (other 1 != hatter 2)

        Assert.False(checker.IsWin(mockStats.Object));
    }
}
