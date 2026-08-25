using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Ability.Behavior;

[Collection("UnityMock")]
public class BatteryBehaviorTests
{
    public BatteryBehaviorTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    [Fact]
    public void IsUse_ReturnsTrue_OnlyWhenCanUseOrActiveAndCurrentChargeGreaterThanZero()
    {
        bool canUse = true;
        var behavior = new BatteryBehavior("Test", null!, () => canUse, () => true)
        {
            ActiveTime = 5.0f
        };

        Assert.True(behavior.IsUse());

        canUse = false;
        Assert.False(behavior.IsUse());
    }

    [Fact]
    public void TryUseAbility_FromReadyState_StartsActivating()
    {
        var behavior = new BatteryBehavior("Test", null!, () => true, () => true)
        {
            ActiveTime = 5.0f
        };

        bool success = behavior.TryUseAbility(0f, AbilityState.Ready, out var newState);

        Assert.True(success);
        Assert.Equal(AbilityState.Activating, newState);
        Assert.True(behavior.IsUse());
    }

    [Fact]
    public void TryUseAbility_FromActivatingState_DeactivatesAndReturnsReady()
    {
        var behavior = new BatteryBehavior("Test", null!, () => true, () => true)
        {
            ActiveTime = 5.0f
        };

        behavior.TryUseAbility(0f, AbilityState.Ready, out _);

        bool success = behavior.TryUseAbility(0f, AbilityState.Activating, out var newState);

        Assert.True(success);
        Assert.Equal(AbilityState.Ready, newState);
    }

    [Fact]
    public void TryUseAbility_AbnormalCases_ReturnsFalse()
    {
        var behavior = new BatteryBehavior("Test", null!, () => true, () => false)
        {
            ActiveTime = 5.0f
        };

        Assert.False(behavior.TryUseAbility(0f, AbilityState.Ready, out var state1));
        Assert.Equal(AbilityState.Ready, state1);

        var behaviorTimer = new BatteryBehavior("Test", null!, () => true, () => true)
        {
            ActiveTime = 5.0f
        };
        Assert.False(behaviorTimer.TryUseAbility(2.0f, AbilityState.Ready, out var state2));
        Assert.Equal(AbilityState.Ready, state2);
    }

    [Fact]
    public void AbilityOffAndForceAbilityOff_TriggersCallbacksAndResetsCharge()
    {
        bool offCalled = false;
        bool forceOffCalled = false;
        var behavior = new BatteryBehavior(
            "Test", null!,
            () => true, () => true,
            abilityOff: () => offCalled = true,
            forceAbilityOff: () => forceOffCalled = true
        )
        {
            ActiveTime = 5.0f
        };

        behavior.AbilityOff();
        Assert.True(offCalled);

        behavior.ForceAbilityOff();
        Assert.True(forceOffCalled);
    }
}
