using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SpecialWinChecker;

public sealed class MastermindWinCheckerTests
{
	[Fact]
	public void Reason_IsMastermindAlive()
	{
		var checker = new MastermindWinChecker();
		Assert.Equal(RoleGameOverReason.MastermindDeathGame, checker.Reason);
	}

	[Fact]
	public void IsWin_TotalAliveThreeOrLessWithAliveMastermind_ReturnsTrue()
	{
		var checker = new MastermindWinChecker();
		var mockRole = new Mock<SingleRoleBase>();
		checker.AddAliveRole(1, mockRole.Object);

		var mockStats = new Mock<IPlayerStatistics>();
		mockStats.SetupGet(s => s.TotalAlive).Returns(3);

		Assert.True(checker.IsWin(mockStats.Object));
	}

	[Fact]
	public void IsWin_TotalAliveMoreThanThree_ReturnsFalse()
	{
		var checker = new MastermindWinChecker();
		var mockRole = new Mock<SingleRoleBase>();
		checker.AddAliveRole(1, mockRole.Object);

		var mockStats = new Mock<IPlayerStatistics>();
		mockStats.SetupGet(s => s.TotalAlive).Returns(4);

		Assert.False(checker.IsWin(mockStats.Object));
	}

	[Fact]
	public void IsWin_NoAliveMastermind_ReturnsFalse()
	{
		var checker = new MastermindWinChecker();
		var mockStats = new Mock<IPlayerStatistics>();
		mockStats.SetupGet(s => s.TotalAlive).Returns(2);

		Assert.False(checker.IsWin(mockStats.Object));
	}
}
