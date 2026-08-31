using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Ability.Behavior;

[Collection("UnityMock")]
public class CountBehaviorTests
{
    public CountBehaviorTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void IsUse_ReturnsTrue_OnlyWhenCanUseIsTrueAndCountGreaterThanZero()
    {
        bool canUse = true;
        var behavior = new CountBehavior("Test", null!, () => canUse, () => true);

        Assert.False(behavior.IsUse());

        behavior.SetAbilityCount(2);
        Assert.True(behavior.IsUse());

        canUse = false;
        Assert.False(behavior.IsUse());
    }

    [Fact]
    public void TryUseAbility_NormalCase_ReducesCountAndSetsCoolDown()
    {
        var behavior = new CountBehavior("Test", null!, () => true, () => true);
        behavior.SetAbilityCount(2);

        bool result = behavior.TryUseAbility(0f, AbilityState.Ready, out var newState);

        Assert.True(result);
        Assert.Equal(1, behavior.AbilityCount);
        Assert.Equal(AbilityState.CoolDown, newState);
    }

    [Fact]
    public void TryUseAbility_AbnormalCases_ReturnsFalse()
    {
        var behavior = new CountBehavior("Test", null!, () => true, () => true);
        behavior.SetAbilityCount(0);

        Assert.False(behavior.TryUseAbility(0f, AbilityState.Ready, out var state1));
        Assert.Equal(AbilityState.Ready, state1);

        behavior.SetAbilityCount(1);
        Assert.False(behavior.TryUseAbility(1.0f, AbilityState.Ready, out var state2));
        Assert.Equal(AbilityState.Ready, state2);

        Assert.False(behavior.TryUseAbility(0f, AbilityState.CoolDown, out var state3));
        Assert.Equal(AbilityState.CoolDown, state3);

        var behaviorFail = new CountBehavior("Test", null!, () => true, () => false);
        behaviorFail.SetAbilityCount(1);
        Assert.False(behaviorFail.TryUseAbility(0f, AbilityState.Ready, out var state4));
        Assert.Equal(1, behaviorFail.AbilityCount);
    }

    [Fact]
    public void Update_WhenSetAbilityCount_FirstUpdateReturnsCoolDown_ThenBasedOnCount()
    {
        var behavior = new CountBehavior("Test", null!, () => true, () => true);
        behavior.SetAbilityCount(1);

        Assert.Equal(AbilityState.CoolDown, behavior.Update(AbilityState.Ready));
        Assert.Equal(AbilityState.Ready, behavior.Update(AbilityState.Ready));

        var behaviorZero = new CountBehavior("Test", null!, () => true, () => true);
        behaviorZero.SetAbilityCount(0);
        behaviorZero.Update(AbilityState.Ready);
        Assert.Equal(AbilityState.None, behaviorZero.Update(AbilityState.Ready));
    }

    [Fact]
    public void AbilityOffAndForceAbilityOff_TriggersCallbacks()
    {
        bool offCalled = false;
        bool forceOffCalled = false;
        var behavior = new CountBehavior(
            "Test", null!,
            () => true, () => true,
            abilityOff: () => offCalled = true,
            forceAbilityOff: () => forceOffCalled = true
        );

        behavior.AbilityOff();
        Assert.True(offCalled);

        behavior.ForceAbilityOff();
        Assert.True(forceOffCalled);
    }

    [Fact]
    public void HideAndShow_DoesNotThrowWhenTextIsNull()
    {
        var behavior = new CountBehavior("Test", null!, () => true, () => true);
        behavior.SetButtonTextFormat("{0}");
        behavior.Hide();
        behavior.Show();
    }
}
