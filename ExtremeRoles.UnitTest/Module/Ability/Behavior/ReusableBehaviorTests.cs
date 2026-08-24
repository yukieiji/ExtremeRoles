using System;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Ability.Behavior;

[Collection("UnityMock")]
public class ReusableBehaviorTests
{
    public ReusableBehaviorTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    [Fact]
    public void IsUse_ReturnsCanUseResult()
    {
        bool canUseValue = true;
        var behavior = new ReusableBehavior("Test", null!, () => canUseValue, () => true);

        Assert.True(behavior.IsUse());

        canUseValue = false;
        Assert.False(behavior.IsUse());
    }

    [Fact]
    public void TryUseAbility_NormalCase_ReturnsTrueAndCoolDownState()
    {
        bool abilityCalled = false;
        var behavior = new ReusableBehavior("Test", null!, () => true, () =>
        {
            abilityCalled = true;
            return true;
        });

        bool success = behavior.TryUseAbility(0f, AbilityState.Ready, out var newState);

        Assert.True(success);
        Assert.True(abilityCalled);
        Assert.Equal(AbilityState.CoolDown, newState);
    }

    [Fact]
    public void TryUseAbility_AbnormalCases_ReturnsFalse()
    {
        var behavior = new ReusableBehavior("Test", null!, () => true, () => true);

        // Timer > 0
        Assert.False(behavior.TryUseAbility(1.0f, AbilityState.Ready, out var state1));
        Assert.Equal(AbilityState.Ready, state1);

        // State not Ready
        Assert.False(behavior.TryUseAbility(0f, AbilityState.CoolDown, out var state2));
        Assert.Equal(AbilityState.CoolDown, state2);

        // Ability function returns false
        var behaviorFail = new ReusableBehavior("Test", null!, () => true, () => false);
        Assert.False(behaviorFail.TryUseAbility(0f, AbilityState.Ready, out var state3));
        Assert.Equal(AbilityState.Ready, state3);
    }

    [Fact]
    public void AbilityOff_And_ForceAbilityOff_TriggersCallbacks()
    {
        bool abilityOffCalled = false;
        bool forceOffCalled = false;

        var behavior = new ReusableBehavior(
            "Test", null!,
            () => true,
            () => true,
            abilityOff: () => abilityOffCalled = true,
            forceAbilityOff: () => forceOffCalled = true
        );

        behavior.AbilityOff();
        Assert.True(abilityOffCalled);

        behavior.ForceAbilityOff();
        Assert.True(forceOffCalled);
    }

    [Fact]
    public void ForceAbilityOff_FallsBackToAbilityOff_WhenNull()
    {
        bool abilityOffCalled = false;

        var behavior = new ReusableBehavior(
            "Test", null!,
            () => true,
            () => true,
            abilityOff: () => abilityOffCalled = true
        );

        behavior.ForceAbilityOff();
        Assert.True(abilityOffCalled);
    }

    [Fact]
    public void Update_ReturnsCurrentState()
    {
        var behavior = new ReusableBehavior("Test", null!, () => true, () => true);
        Assert.Equal(AbilityState.Ready, behavior.Update(AbilityState.Ready));
    }
}
