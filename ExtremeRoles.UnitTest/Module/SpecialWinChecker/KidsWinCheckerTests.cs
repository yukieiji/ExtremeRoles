using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SpecialWinChecker;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class KidsWinCheckerTests
{
    private static Delinquent CreateDelinquent(bool winCheckEnable, float range, int gameControlId = 1)
    {
        var delinquent = (Delinquent)RuntimeHelpers.GetUninitializedObject(typeof(Delinquent));

        var isWinCheckField = typeof(Delinquent).GetField("isWinCheck", BindingFlags.NonPublic | BindingFlags.Instance);
        isWinCheckField?.SetValue(delinquent, winCheckEnable);

        var rangeField = typeof(Delinquent).GetField("<Range>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        rangeField?.SetValue(delinquent, range);

        var gcField = typeof(Delinquent).GetField("<GameControlId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        gcField?.SetValue(delinquent, gameControlId);

        return delinquent;
    }

    public KidsWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        SetupGameModeManager();
    }

    private static void SetupGameModeManager()
    {
        var manager = ExtremeGameModeManager.Instance;
        if (manager == null)
        {
            var mockManager = new Mock<ExtremeGameModeManager>();
            var prop = typeof(ExtremeGameModeManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            prop?.SetValue(null, mockManager.Object);
        }
    }

    [Fact]
    public void IsWin_NoDelinquentWithWinCheckEnable_ReturnsFalse()
    {
        var checker = new KidsWinChecker();
        var mockDelinquent = CreateDelinquent(false, 5f);
        checker.AddAliveRole(1, mockDelinquent);

        var mockStats = new Mock<IPlayerStatistics>();
        Assert.False(checker.IsWin(mockStats.Object));
    }

    [Fact]
    public void IsWin_PlayerControlNotFound_ReturnsFalse()
    {
        var checker = new KidsWinChecker();
        var mockDelinquent = CreateDelinquent(true, 5f);
        checker.AddAliveRole(1, mockDelinquent);

        var mockStats = new Mock<IPlayerStatistics>();
        Assert.False(checker.IsWin(mockStats.Object));
    }
}
