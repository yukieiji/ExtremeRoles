using System.Collections.Generic;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SpecialWinChecker;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class YandereWinCheckerTests
{
    public YandereWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void IsWin_NoYandereOrNoOneSidedLover_ReturnsFalse()
    {
        var checker = new YandereWinChecker();
        var mockStats = new Mock<IPlayerStatistics>();

        // aliveYandere is empty
        Assert.False(checker.IsWin(mockStats.Object));
    }
}
