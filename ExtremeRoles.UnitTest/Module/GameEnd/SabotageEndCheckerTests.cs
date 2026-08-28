using System;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

public sealed class SabotageEndCheckerTests
{
    public SabotageEndCheckerTests()
    {
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
        var mockSoundProvider = new Mock<ISoundProvider>();
        TeroristTeroSabotageSystem system = new TeroristTeroSabotageSystem(default, false, mockSoundProvider.Object);

        System.Reflection.PropertyInfo? prop = typeof(TeroristTeroSabotageSystem).GetProperty(nameof(TeroristTeroSabotageSystem.ExplosionTimer), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(system, -1.0f);
        }
        else
        {
            System.Reflection.FieldInfo? field = typeof(TeroristTeroSabotageSystem).GetField("<ExplosionTimer>k__BackingField", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(system, -1.0f);
        }

        TeroristTeroSabotageSystemEndChecker checker = new TeroristTeroSabotageSystemEndChecker(system);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.TeroristoTeroWithShip, reason);
    }

    [Fact]
    public void TeroristTeroSabotageSystemEndChecker_TryCheckGameEnd_ReturnsFalseWhenExplosionTimerPositive()
    {
        var mockSoundProvider = new Mock<ISoundProvider>();
        TeroristTeroSabotageSystem system = new TeroristTeroSabotageSystem(default, false, mockSoundProvider.Object);

        System.Reflection.PropertyInfo? prop = typeof(TeroristTeroSabotageSystem).GetProperty(nameof(TeroristTeroSabotageSystem.ExplosionTimer), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(system, 30.0f);
        }
        else
        {
            System.Reflection.FieldInfo? field = typeof(TeroristTeroSabotageSystem).GetField("<ExplosionTimer>k__BackingField", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(system, 30.0f);
        }

        TeroristTeroSabotageSystemEndChecker checker = new TeroristTeroSabotageSystemEndChecker(system);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }
}
