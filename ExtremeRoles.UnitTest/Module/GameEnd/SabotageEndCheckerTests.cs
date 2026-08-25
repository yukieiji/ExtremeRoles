using System;
using System.Reflection;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection("UnityMock")]
public sealed class SabotageEndCheckerTests
{
    public SabotageEndCheckerTests()
    {
        MockSetupHelper.SetupCommonMocks();
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
    public void LifeSuppSystemEndChecker_CleanUp_ResetsCountdown()
    {
        var mockSystem = new Mock<LifeSuppSystemType>(IntPtr.Zero);
        mockSystem.SetupSet(s => s.Countdown = 10000f).Verifiable();

        LifeSuppSystemEndChecker checker = new LifeSuppSystemEndChecker(mockSystem.Object);
        checker.CleanUp();

        mockSystem.VerifySet(s => s.Countdown = 10000f, Times.Once());
    }

    [Fact]
    public void CriticalSystemEndChecker_TryCheckGameEnd_ReturnsTrueWhenCountdownNegative()
    {
        var mockSystem = new Mock<ICriticalSabotage>();
        mockSystem.SetupGet(s => s.Countdown).Returns(-1.0f);

        CriticalSystemEndChecker checker = new CriticalSystemEndChecker(mockSystem.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal(GameOverReason.ImpostorsBySabotage, reason);
    }

    [Fact]
    public void CriticalSystemEndChecker_CleanUp_ClearsSabotage()
    {
        var mockSystem = new Mock<ICriticalSabotage>();
        mockSystem.Setup(s => s.ClearSabotage()).Verifiable();

        CriticalSystemEndChecker checker = new CriticalSystemEndChecker(mockSystem.Object);
        checker.CleanUp();

        mockSystem.Verify(s => s.ClearSabotage(), Times.Once());
    }

    [Fact]
    public void TeroristTeroSabotageSystemEndChecker_TryCheckGameEnd_ReturnsTrueWhenExplosionTimerNegative()
    {
        TeroristTeroSabotageSystem system = (TeroristTeroSabotageSystem)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(TeroristTeroSabotageSystem));
        PropertyInfo? prop = typeof(TeroristTeroSabotageSystem).GetProperty(nameof(TeroristTeroSabotageSystem.ExplosionTimer), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(system, -1.0f);
        }
        else
        {
            FieldInfo? field = typeof(TeroristTeroSabotageSystem).GetField("<ExplosionTimer>k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(system, -1.0f);
        }

        TeroristTeroSabotageSystemEndChecker checker = new TeroristTeroSabotageSystemEndChecker(system);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.TeroristoTeroWithShip, reason);
    }

    [Fact]
    public void TeroristTeroSabotageSystemEndChecker_CleanUp_CallsClear()
    {
        TeroristTeroSabotageSystem system = (TeroristTeroSabotageSystem)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(TeroristTeroSabotageSystem));

        TeroristTeroSabotageSystemEndChecker checker = new TeroristTeroSabotageSystemEndChecker(system);
        checker.CleanUp();
    }
}
