using System.Reflection;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection("UnityMock")]
public sealed class LiberalMoneyWinCheckerTests
{
    public LiberalMoneyWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    private static void SetProperty<T>(object target, string propertyName, T value)
    {
        PropertyInfo? prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        prop?.SetValue(target, value);
    }

    [Fact]
    public void TryCheckGameEnd_MoneyEqualsWinMoney_ReturnsTrue()
    {
        var mockOption = new Mock<ILiberalOptionLoader>();
        mockOption.Setup(o => o.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.WinMoney)).Returns(100);

        LiberalMoneyBankSystem system = new LiberalMoneyBankSystem(mockOption.Object);
        SetProperty(system, nameof(LiberalMoneyBankSystem.Money), 100f);

        LiberalMoneyWinChecker checker = new LiberalMoneyWinChecker(system);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.LiberalRevolution, reason);
    }

    [Fact]
    public void TryCheckGameEnd_MoneyLessThanWinMoney_ReturnsFalse()
    {
        var mockOption = new Mock<ILiberalOptionLoader>();
        mockOption.Setup(o => o.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.WinMoney)).Returns(100);

        LiberalMoneyBankSystem system = new LiberalMoneyBankSystem(mockOption.Object);
        SetProperty(system, nameof(LiberalMoneyBankSystem.Money), 50f);

        LiberalMoneyWinChecker checker = new LiberalMoneyWinChecker(system);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }
}
