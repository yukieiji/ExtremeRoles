using System.Collections.Generic;
using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SpecialWinChecker;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class KidsWinCheckerTests
{
    public KidsWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void IsWin_NoDelinquentWithWinCheckEnable_ReturnsFalse()
    {
        var checker = new KidsWinChecker();
        var mockStats = new Mock<IPlayerStatistics>();

        // aliveDelinquent is empty
        Assert.False(checker.IsWin(mockStats.Object));
    }
}
