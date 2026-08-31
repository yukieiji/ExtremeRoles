using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Ability.Behavior;


[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ActivatingCountBehaviorTests
{
    public ActivatingCountBehaviorTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void IsUse_ReturnsTrue_WhenCanUseIsTrueAndCountOrActivating()
    {
        bool canUse = true;
        var behavior = new ActivatingCountBehavior("Test", null!, () => canUse, () => true);

        Assert.False(behavior.IsUse());

        behavior.SetAbilityCount(1);
        Assert.True(behavior.IsUse());

        canUse = false;
        Assert.False(behavior.IsUse());
    }

    [Fact]
    public void TryUseAbility_WithActiveTime_NormalCase_SetsActivating()
    {
        var behavior = new ActivatingCountBehavior(
            "Test", null!,
            () => true, () => true,
            isReduceOnActive: true
        )
        {
            ActiveTime = 5.0f
        };
        behavior.SetAbilityCount(1);

        bool success = behavior.TryUseAbility(0f, AbilityState.Ready, out var newState);

        Assert.True(success);
        Assert.Equal(AbilityState.Activating, newState);
        Assert.Equal(0, behavior.AbilityCount);
    }

    [Fact]
    public void TryUseAbility_WithReduceOnActiveFalse_ReducesCountOnAbilityOff()
    {
        bool offCalled = false;
        var behavior = new ActivatingCountBehavior(
            "Test", null!,
            () => true, () => true,
            abilityOff: () => offCalled = true,
            isReduceOnActive: false
        )
        {
            ActiveTime = 5.0f
        };
        behavior.SetAbilityCount(1);

        bool success = behavior.TryUseAbility(0f, AbilityState.Ready, out var newState);

        Assert.True(success);
        Assert.Equal(1, behavior.AbilityCount);

        behavior.AbilityOff();
        Assert.True(offCalled);
        Assert.Equal(0, behavior.AbilityCount);
    }

    [Fact]
    public void TryUseAbility_AbnormalCases_ReturnsFalse()
    {
        var behavior = new ActivatingCountBehavior("Test", null!, () => true, () => true);
        behavior.SetAbilityCount(0);

        Assert.False(behavior.TryUseAbility(0f, AbilityState.Ready, out var state1));
        Assert.Equal(AbilityState.Ready, state1);

        behavior.SetAbilityCount(1);
        Assert.False(behavior.TryUseAbility(2.0f, AbilityState.Ready, out var state2));
        Assert.Equal(AbilityState.Ready, state2);
    }

    [Fact]
    public void ForceAbilityOff_ResetsActivatingAndInvokesCallback()
    {
        bool forceCalled = false;
        var behavior = new ActivatingCountBehavior(
            "Test", null!,
            () => true, () => true,
            forceAbilityOff: () => forceCalled = true
        );

        behavior.ForceAbilityOff();
        Assert.True(forceCalled);
    }

    [Fact]
    public void Update_WhenActivating_ReturnsActivating()
    {
        var behavior = new ActivatingCountBehavior("Test", null!, () => true, () => true);
        behavior.SetAbilityCount(1);

        Assert.Equal(AbilityState.Activating, behavior.Update(AbilityState.Activating));
    }

    [Fact]
    public void HideAndShow_DoesNotThrowWhenTextIsNull()
    {
        var behavior = new ActivatingCountBehavior("Test", null!, () => true, () => true);
        behavior.SetButtonTextFormat("{0}");
        behavior.Hide();
        behavior.Show();
    }
}
