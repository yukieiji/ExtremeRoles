using System;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Ability.Behavior;

[Collection("UnityMock")]
public class ChargingAndActivatingCountBehaviourTests
{
    public ChargingAndActivatingCountBehaviourTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    [Fact]
    public void Properties_SetAndGet_ReturnsExpectedValues()
    {
        var behavior = new ChargingAndActivatingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => true,
            ChargingAndActivatingCountBehaviour.ReduceTiming.OnCharge
        )
        {
            ChargeGage = 0.5f,
            ChargeTime = 3.0f,
            ActiveTime = 4.0f
        };

        Assert.Equal(0.5f, behavior.ChargeGage);
        Assert.Equal(3.0f, behavior.ChargeTime);
        Assert.Equal(4.0f, behavior.ActiveTime);
        Assert.True(behavior.IsCharging);
        Assert.True(behavior.CanAbilityActiving);
    }

    [Fact]
    public void IsUse_ReturnsTrue_WhenCountOrChargeOrActiveAndCanUse()
    {
        bool canUseParam = true;
        var behavior = new ChargingAndActivatingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => canUseParam,
            gage => true,
            () => true,
            ChargingAndActivatingCountBehaviour.ReduceTiming.OnCharge
        );

        Assert.False(behavior.IsUse()); // count 0

        behavior.SetAbilityCount(1);
        Assert.True(behavior.IsUse());

        canUseParam = false;
        Assert.False(behavior.IsUse());
    }

    [Fact]
    public void TryUseAbility_ReduceOnCharge_TransitionsFromReadyToChargingToActivating()
    {
        var behavior = new ChargingAndActivatingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => true,
            ChargingAndActivatingCountBehaviour.ReduceTiming.OnCharge
        )
        {
            ActiveTime = 5.0f
        };
        behavior.SetAbilityCount(2);

        // Ready -> Charging
        bool success1 = behavior.TryUseAbility(0f, AbilityState.Ready, out var state1);
        Assert.True(success1);
        Assert.Equal(AbilityState.Charging, state1);
        Assert.Equal(1, behavior.AbilityCount); // Reduced on charge

        // Charging -> Activating
        bool success2 = behavior.TryUseAbility(0f, AbilityState.Charging, out var state2);
        Assert.True(success2);
        Assert.Equal(AbilityState.Activating, state2);
    }

    [Fact]
    public void TryUseAbility_ReduceOnActive_ReducesCountWhenTransitioningToActive()
    {
        var behavior = new ChargingAndActivatingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => true,
            ChargingAndActivatingCountBehaviour.ReduceTiming.OnActive
        )
        {
            ActiveTime = 0.0f // CoolDown state
        };
        behavior.SetAbilityCount(1);

        // Ready -> Charging
        behavior.TryUseAbility(0f, AbilityState.Ready, out var state1);
        Assert.Equal(AbilityState.Charging, state1);
        Assert.Equal(1, behavior.AbilityCount);

        // Charging -> CoolDown (since ActiveTime <= 0)
        behavior.TryUseAbility(0f, AbilityState.Charging, out var state2);
        Assert.Equal(AbilityState.CoolDown, state2);
        Assert.Equal(0, behavior.AbilityCount);
    }

    [Fact]
    public void TryUseAbility_ReduceOnActiveDone_ReducesCountOnAbilityOff()
    {
        var behavior = new ChargingAndActivatingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => true,
            ChargingAndActivatingCountBehaviour.ReduceTiming.OnActiveDone
        )
        {
            ActiveTime = 5.0f
        };
        behavior.SetAbilityCount(1);

        behavior.TryUseAbility(0f, AbilityState.Ready, out _);
        behavior.TryUseAbility(0f, AbilityState.Charging, out _);
        Assert.Equal(1, behavior.AbilityCount);

        behavior.AbilityOff();
        Assert.Equal(0, behavior.AbilityCount);
    }

    [Fact]
    public void TryUseAbility_AbnormalCases_ReturnsFalse()
    {
        var behavior = new ChargingAndActivatingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => false, // ability fails
            () => false, // onCharge fails
            ChargingAndActivatingCountBehaviour.ReduceTiming.OnCharge
        );
        behavior.SetAbilityCount(1);

        // onCharge fails
        Assert.False(behavior.TryUseAbility(0f, AbilityState.Ready, out var state1));
        Assert.Equal(AbilityState.Ready, state1);

        // Invalid state (e.g., CoolDown)
        Assert.False(behavior.TryUseAbility(0f, AbilityState.CoolDown, out var state2));
        Assert.Equal(AbilityState.CoolDown, state2);
    }

    [Fact]
    public void ForceAbilityOff_InvokesCallbackAndResetsFlags()
    {
        bool forceCalled = false;
        var behavior = new ChargingAndActivatingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => true,
            ChargingAndActivatingCountBehaviour.ReduceTiming.OnCharge,
            forceAbilityOff: () => forceCalled = true
        );

        behavior.ForceAbilityOff();
        Assert.True(forceCalled);
    }

    [Fact]
    public void Update_WhenChargingOrActivating_ReturnsCurrentState()
    {
        var behavior = new ChargingAndActivatingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => true,
            ChargingAndActivatingCountBehaviour.ReduceTiming.OnCharge
        );
        behavior.SetAbilityCount(1);

        Assert.Equal(AbilityState.Charging, behavior.Update(AbilityState.Charging));
        Assert.Equal(AbilityState.Activating, behavior.Update(AbilityState.Activating));
    }

    [Fact]
    public void HideAndShow_DoesNotThrowWhenTextIsNull()
    {
        var behavior = new ChargingAndActivatingCountBehaviour(
            "Test", null!,
            (isCharge, gage) => true,
            gage => true,
            () => true,
            ChargingAndActivatingCountBehaviour.ReduceTiming.OnCharge
        );
        behavior.SetButtonTextFormat("{0}");
        behavior.Hide();
        behavior.Show();
    }
}
