using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Ability.Behavior;

[Collection("UnityMock")]
public class ChargingAndReclickCountBehaviorTests
{
    public ChargingAndReclickCountBehaviorTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void IsUse_ReturnsTrue_WhenCountOrChargeOrActiveAndCanUse()
    {
        bool canUseVal = true;
        var behavior = new ChargingAndReclickCountBehavior(
            "Test", null!,
            (isCharge, gage) => canUseVal,
            () => true,
            gage => true
        );

        Assert.False(behavior.IsUse());

        behavior.SetAbilityCount(1);
        Assert.True(behavior.IsUse());

        canUseVal = false;
        Assert.False(behavior.IsUse());
    }

    [Fact]
    public void TryUseAbility_NormalLifecycle_ReadyToChargingToActivatingToCoolDown()
    {
        var behavior = new ChargingAndReclickCountBehavior(
            "Test", null!,
            (isCharge, gage) => true,
            () => true,
            gage => true,
            reduceOnCharge: false
        )
        {
            ActiveTime = 10.0f
        };
        behavior.SetAbilityCount(1);

        // Ready -> Charging
        bool success1 = behavior.TryUseAbility(0f, AbilityState.Ready, out var state1);
        Assert.True(success1);
        Assert.Equal(AbilityState.Charging, state1);
        Assert.Equal(1, behavior.AbilityCount);

        // Charging -> Activating
        bool success2 = behavior.TryUseAbility(0f, AbilityState.Charging, out var state2);
        Assert.True(success2);
        Assert.Equal(AbilityState.Activating, state2);
        Assert.Equal(0, behavior.AbilityCount);

        // Activating -> CoolDown (Reclick)
        bool success3 = behavior.TryUseAbility(0f, AbilityState.Activating, out var state3);
        Assert.True(success3);
        Assert.Equal(AbilityState.CoolDown, state3);
    }

    [Fact]
    public void TryUseAbility_ReduceOnCharge_ReducesCountOnReadyState()
    {
        var behavior = new ChargingAndReclickCountBehavior(
            "Test", null!,
            (isCharge, gage) => true,
            () => true,
            gage => true,
            reduceOnCharge: true
        );
        behavior.SetAbilityCount(1);

        behavior.TryUseAbility(0f, AbilityState.Ready, out var state1);
        Assert.Equal(AbilityState.Charging, state1);
        Assert.Equal(0, behavior.AbilityCount);
    }

    [Fact]
    public void TryUseAbility_AbnormalCases_ReturnsFalse()
    {
        var behavior = new ChargingAndReclickCountBehavior(
            "Test", null!,
            (isCharge, gage) => true,
            () => false,
            gage => true
        );
        behavior.SetAbilityCount(1);

        Assert.False(behavior.TryUseAbility(0f, AbilityState.Ready, out var state1));
        Assert.Equal(AbilityState.Ready, state1);

        Assert.False(behavior.TryUseAbility(2.0f, AbilityState.Ready, out var state2));
        Assert.Equal(AbilityState.Ready, state2);

        Assert.False(behavior.TryUseAbility(0f, AbilityState.None, out var state3));
        Assert.Equal(AbilityState.None, state3);
    }

    [Fact]
    public void AbilityOffAndForceAbilityOff_ResetsFlagsAndTriggersCallback()
    {
        bool offCalled = false;
        var behavior = new ChargingAndReclickCountBehavior(
            "Test", null!,
            (isCharge, gage) => true,
            () => true,
            gage => true,
            abilityOff: () => offCalled = true
        );

        behavior.AbilityOff();
        Assert.True(offCalled);

        offCalled = false;
        behavior.ForceAbilityOff();
        Assert.True(offCalled);
    }

    [Fact]
    public void Update_WhenChargingOrActivating_ReturnsCurrentState()
    {
        var behavior = new ChargingAndReclickCountBehavior(
            "Test", null!,
            (isCharge, gage) => true,
            () => true,
            gage => true
        );
        behavior.SetAbilityCount(1);

        Assert.Equal(AbilityState.Charging, behavior.Update(AbilityState.Charging));
        Assert.Equal(AbilityState.Activating, behavior.Update(AbilityState.Activating));
    }

    [Fact]
    public void HideAndShow_DoesNotThrowWhenTextIsNull()
    {
        var behavior = new ChargingAndReclickCountBehavior(
            "Test", null!,
            (isCharge, gage) => true,
            () => true,
            gage => true
        );
        behavior.SetButtonTextFormat("{0}");
        behavior.Hide();
        behavior.Show();
    }
}
