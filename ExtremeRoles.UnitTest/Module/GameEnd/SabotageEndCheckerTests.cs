using System;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;


[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class SabotageEndCheckerTests
{
    public SabotageEndCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void LifeSuppSystemEndChecker_TryCheckGameEnd_ReturnsTrueWhenCountdownNegative()
    {
        var mockSystem = new Mock<LifeSuppSystemType>(IntPtr.Zero);
        mockSystem.SetupGet(s => s.Countdown).Returns(-1.0f);

        LifeSuppSystemEndChecker checker = new LifeSuppSystemEndChecker(mockSystem.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal(GameOverReason.ImpostorsBySabotage, reason);
    }

    [Fact]
    public void LifeSuppSystemEndChecker_TryCheckGameEnd_ReturnsFalseWhenCountdownPositive()
    {
        var mockSystem = new Mock<LifeSuppSystemType>(IntPtr.Zero);
        mockSystem.SetupGet(s => s.Countdown).Returns(10.0f);

        LifeSuppSystemEndChecker checker = new LifeSuppSystemEndChecker(mockSystem.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }

    [Fact]
    public void LifeSuppSystemEndChecker_CleanUp_ResetsCountdown()
    {
        var mockSystem = new Mock<LifeSuppSystemType>(IntPtr.Zero);
        mockSystem.SetupSet(s => s.Countdown = 10000f).Verifiable();

        LifeSuppSystemEndChecker checker = new LifeSuppSystemEndChecker(mockSystem.Object);
        checker.CleanUp();

        mockSystem.VerifySet(s => s.Countdown = 10000f, Times.Once());
    }

    [Fact]
    public void TeroristTeroSabotageSystemEndChecker_TryCheckGameEnd_ReturnsTrueWhenExplosionTimerNegative()
    {
        var mockSystem = new Mock<ITeroristTeroSabotageSystem>();
        mockSystem.SetupGet(s => s.ExplosionTimer).Returns(-1.0f);

        TeroristTeroSabotageSystemEndChecker checker = new TeroristTeroSabotageSystemEndChecker(mockSystem.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.TeroristoTeroWithShip, reason);
    }

    [Fact]
    public void TeroristTeroSabotageSystemEndChecker_TryCheckGameEnd_ReturnsFalseWhenExplosionTimerPositive()
    {
        var mockSystem = new Mock<ITeroristTeroSabotageSystem>();
        mockSystem.SetupGet(s => s.ExplosionTimer).Returns(30.0f);

        TeroristTeroSabotageSystemEndChecker checker = new TeroristTeroSabotageSystemEndChecker(mockSystem.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }
}
