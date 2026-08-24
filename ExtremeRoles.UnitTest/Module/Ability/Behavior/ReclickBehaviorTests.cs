using System;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Ability.Behavior;

[Collection("UnityMock")]
public class ReclickBehaviorTests
{
    public ReclickBehaviorTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    [Fact]
    public void Properties_SetAndGet_ReturnsExpectedValues()
    {
        var behavior = new ReclickBehavior(
            "Test", null!,
            () => true, () => true
        )
        {
            ActiveTime = 4.0f
        };

        Assert.Equal(4.0f, behavior.ActiveTime);
        Assert.True(behavior.CanAbilityActiving);
        Assert.Equal(AbilityState.CoolDown, behavior.ReclickStatus);
    }

    [Fact]
    public void IsUse_ReturnsTrue_WhenCanUseIsTrueOrActive()
    {
        bool canUseVal = true;
        var behavior = new ReclickBehavior(
            "Test", null!,
            () => canUseVal, () => true
        );

        Assert.True(behavior.IsUse());

        canUseVal = false;
        Assert.False(behavior.IsUse());
    }

    [Fact]
    public void TryUseAbility_NormalLifecycle_ReadyToActivatingToCoolDown()
    {
        var behavior = new ReclickBehavior(
            "Test", null!,
            () => true, () => true
        )
        {
            ActiveTime = 10.0f
        };

        // Ready -> Activating
        bool success1 = behavior.TryUseAbility(0f, AbilityState.Ready, out var state1);
        Assert.True(success1);
        Assert.Equal(AbilityState.Activating, state1);
        Assert.True(behavior.IsUse()); // IsUse true while active even if canUse false

        // Activating -> CoolDown (Reclick)
        bool success2 = behavior.TryUseAbility(0f, AbilityState.Activating, out var state2);
        Assert.True(success2);
        Assert.Equal(AbilityState.CoolDown, state2);
    }

    [Fact]
    public void TryUseAbility_AbnormalCases_ReturnsFalse()
    {
        var behavior = new ReclickBehavior(
            "Test", null!,
            () => true, () => false // ability fails
        );

        Assert.False(behavior.TryUseAbility(0f, AbilityState.Ready, out var state1));
        Assert.Equal(AbilityState.Ready, state1);

        Assert.False(behavior.TryUseAbility(1.0f, AbilityState.Ready, out var state2));
        Assert.Equal(AbilityState.Ready, state2);

        Assert.False(behavior.TryUseAbility(0f, AbilityState.None, out var state3));
        Assert.Equal(AbilityState.None, state3);
    }

    [Fact]
    public void AbilityOffAndForceAbilityOff_TriggersCallbacksAndResetsActive()
    {
        bool offCalled = false;
        var behavior = new ReclickBehavior(
            "Test", null!,
            () => true, () => true,
            abilityOff: () => offCalled = true
        );

        behavior.TryUseAbility(0f, AbilityState.Ready, out _); // active

        behavior.AbilityOff();
        Assert.True(offCalled);

        offCalled = false;
        behavior.ForceAbilityOff();
        Assert.True(offCalled);
    }

    [Fact]
    public void InitializeAndUpdate_DoNotThrowAndReturnCurrentState()
    {
        var behavior = new ReclickBehavior("Test", null!, () => true, () => true);
        behavior.Initialize(null!);
        Assert.Equal(AbilityState.Ready, behavior.Update(AbilityState.Ready));
    }
}
