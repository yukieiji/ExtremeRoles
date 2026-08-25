using System.Reflection;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection("UnityMock")]
public sealed class LiberalMoneyWinCheckerTests
{
    public LiberalMoneyWinCheckerTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    private static void SetProperty<T>(object target, string propertyName, T value)
    {
        PropertyInfo? prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        prop?.SetValue(target, value);
    }

    [Fact]
    public void TryCheckGameEnd_MoneyEqualsWinMoney_ReturnsTrue()
    {
        LiberalMoneyBankSystem system = (LiberalMoneyBankSystem)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(LiberalMoneyBankSystem));
        SetProperty(system, nameof(LiberalMoneyBankSystem.Money), 100f);
        SetProperty(system, nameof(LiberalMoneyBankSystem.WinMoney), 100f);

        LiberalMoneyWinChecker checker = new LiberalMoneyWinChecker(system);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.LiberalRevolution, reason);
    }

    [Fact]
    public void TryCheckGameEnd_MoneyLessThanWinMoney_ReturnsFalse()
    {
        LiberalMoneyBankSystem system = (LiberalMoneyBankSystem)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(LiberalMoneyBankSystem));
        SetProperty(system, nameof(LiberalMoneyBankSystem.Money), 50f);
        SetProperty(system, nameof(LiberalMoneyBankSystem.WinMoney), 100f);

        LiberalMoneyWinChecker checker = new LiberalMoneyWinChecker(system);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }
}
