using System;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Ability.Behavior;

[Collection("UnityMock")]
public class ChargingCountBehaviourTests
{
    public ChargingCountBehaviourTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    [Fact]
    public void Properties_SetAndGet_ReturnsExpectedValues()
    {
        var behavior = new ChargingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => true,
            ChargingCountBehaviour.ReduceTiming.OnCharge
        )
        {
            ChargeGage = 1.0f,
            ChargeTime = 2.5f
        };

        Assert.Equal(1.0f, behavior.ChargeGage);
        Assert.Equal(2.5f, behavior.ChargeTime);
        Assert.True(behavior.IsCharging);
    }

    [Fact]
    public void IsUse_ReturnsTrue_WhenCountOrChargeAndCanUse()
    {
        bool canUseVal = true;
        var behavior = new ChargingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => canUseVal,
            gage => true,
            () => true,
            ChargingCountBehaviour.ReduceTiming.OnCharge
        );

        Assert.False(behavior.IsUse());

        behavior.SetAbilityCount(1);
        Assert.True(behavior.IsUse());

        canUseVal = false;
        Assert.False(behavior.IsUse());
    }

    [Fact]
    public void TryUseAbility_ReduceOnCharge_TransitionsFromReadyToChargingToCoolDown()
    {
        var behavior = new ChargingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => true,
            ChargingCountBehaviour.ReduceTiming.OnCharge
        );
        behavior.SetAbilityCount(2);

        // Ready -> Charging
        bool success1 = behavior.TryUseAbility(0f, AbilityState.Ready, out var state1);
        Assert.True(success1);
        Assert.Equal(AbilityState.Charging, state1);
        Assert.Equal(1, behavior.AbilityCount);

        // Charging -> CoolDown
        bool success2 = behavior.TryUseAbility(0f, AbilityState.Charging, out var state2);
        Assert.True(success2);
        Assert.Equal(AbilityState.CoolDown, state2);
    }

    [Fact]
    public void TryUseAbility_ReduceOnActive_ReducesCountOnChargingFinish()
    {
        var behavior = new ChargingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => true,
            ChargingCountBehaviour.ReduceTiming.OnActive
        );
        behavior.SetAbilityCount(1);

        behavior.TryUseAbility(0f, AbilityState.Ready, out var state1);
        Assert.Equal(AbilityState.Charging, state1);
        Assert.Equal(1, behavior.AbilityCount);

        behavior.TryUseAbility(0f, AbilityState.Charging, out var state2);
        Assert.Equal(AbilityState.CoolDown, state2);
        Assert.Equal(0, behavior.AbilityCount);
    }

    [Fact]
    public void TryUseAbility_AbnormalCases_ReturnsFalse()
    {
        var behavior = new ChargingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => false, // onCharge fails
            ChargingCountBehaviour.ReduceTiming.OnCharge
        );
        behavior.SetAbilityCount(1);

        Assert.False(behavior.TryUseAbility(0f, AbilityState.Ready, out var state1));
        Assert.Equal(AbilityState.Ready, state1);

        Assert.False(behavior.TryUseAbility(1.0f, AbilityState.Ready, out var state2));
        Assert.Equal(AbilityState.Ready, state2);
    }

    [Fact]
    public void AbilityOffAndForceAbilityOff_TriggersCallbacksAndResetsFlags()
    {
        bool offCalled = false;
        bool forceCalled = false;
        var behavior = new ChargingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => true,
            ChargingCountBehaviour.ReduceTiming.OnCharge,
            abilityOff: () => offCalled = true,
            forceAbilityOff: () => forceCalled = true
        );

        behavior.AbilityOff();
        Assert.True(offCalled);

        behavior.ForceAbilityOff();
        Assert.True(forceCalled);
    }

    [Fact]
    public void Update_WhenChargingOrActivating_ReturnsCurrentState()
    {
        var behavior = new ChargingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => true,
            ChargingCountBehaviour.ReduceTiming.OnCharge
        );
        behavior.SetAbilityCount(1);

        Assert.Equal(AbilityState.Charging, behavior.Update(AbilityState.Charging));
        Assert.Equal(AbilityState.Activating, behavior.Update(AbilityState.Activating));
    }

    [Fact]
    public void HideAndShow_DoesNotThrowWhenTextIsNull()
    {
        var behavior = new ChargingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => true,
            ChargingCountBehaviour.ReduceTiming.OnCharge
        );
        behavior.SetButtonTextFormat("{0}");
        behavior.Hide();
        behavior.Show();
    }
}
