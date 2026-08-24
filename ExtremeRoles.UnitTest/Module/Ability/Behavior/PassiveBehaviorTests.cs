using System;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Ability.Behavior;

[Collection("UnityMock")]
public class PassiveBehaviorTests
{
    public PassiveBehaviorTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    [Fact]
    public void Properties_SetAndGet_ReturnsExpectedValues()
    {
        var activeG = new ButtonGraphic("Active", null!);
        var deactiveG = new ButtonGraphic("Deactive", null!);

        var behavior = new PassiveBehavior(
            activeG, deactiveG,
            () => true, () => true
        )
        {
            ActiveTime = 10.0f
        };

        Assert.Equal(10.0f, behavior.ActiveTime);
        Assert.True(behavior.CanAbilityActiving);
    }

    [Fact]
    public void IsUse_ReturnsCanUseResult()
    {
        bool canUseVal = true;
        var behavior = new PassiveBehavior(
            new ButtonGraphic("A", null!), new ButtonGraphic("D", null!),
            () => canUseVal, () => true
        );

        Assert.True(behavior.IsUse());

        canUseVal = false;
        Assert.False(behavior.IsUse());
    }

    [Fact]
    public void TryUseAbility_TogglesActiveStateAndGraphic()
    {
        var activeG = new ButtonGraphic("Active", null!);
        var deactiveG = new ButtonGraphic("Deactive", null!);

        var behavior = new PassiveBehavior(
            activeG, deactiveG,
            () => true, () => true
        );
        behavior.SetCoolTime(15.0f);
        behavior.ActiveTime = 5.0f;

        // Toggle ON
        bool success1 = behavior.TryUseAbility(0f, AbilityState.Ready, out var state1);
        Assert.True(success1);
        Assert.Equal(AbilityState.CoolDown, state1);
        Assert.Equal("Deactive", behavior.Graphic.Text);
        Assert.Equal(5.0f, behavior.CoolTime); // Set to activeTime

        // Toggle OFF (via second use)
        bool success2 = behavior.TryUseAbility(0f, AbilityState.Ready, out var state2);
        Assert.True(success2);
        Assert.Equal(AbilityState.CoolDown, state2);
        Assert.Equal("Active", behavior.Graphic.Text);
        Assert.Equal(15.0f, behavior.CoolTime); // Restored to baseCoolTime
    }

    [Fact]
    public void TryUseAbility_AbnormalCases_ReturnsFalse()
    {
        var behavior = new PassiveBehavior(
            new ButtonGraphic("A", null!), new ButtonGraphic("D", null!),
            () => true, () => false // ability fails
        );

        Assert.False(behavior.TryUseAbility(0f, AbilityState.Ready, out var state1));
        Assert.Equal(AbilityState.Ready, state1);

        Assert.False(behavior.TryUseAbility(1.0f, AbilityState.Ready, out var state2));
        Assert.Equal(AbilityState.Ready, state2);
    }

    [Fact]
    public void ForceAbilityOff_ResetsGraphicAndCoolTimeAndInvokesCallback()
    {
        bool offCalled = false;
        var behavior = new PassiveBehavior(
            new ButtonGraphic("A", null!), new ButtonGraphic("D", null!),
            () => true, () => true,
            abilityOff: () => offCalled = true
        );
        behavior.SetCoolTime(10.0f);
        behavior.ActiveTime = 5.0f;

        // Toggle ON first
        behavior.TryUseAbility(0f, AbilityState.Ready, out _);
        Assert.Equal("D", behavior.Graphic.Text);

        // Force OFF
        behavior.ForceAbilityOff();
        Assert.True(offCalled);
        Assert.Equal("A", behavior.Graphic.Text);
        Assert.Equal(10.0f, behavior.CoolTime);
    }

    [Fact]
    public void Update_WhenActiveAndCanActivatingBecomesFalse_ForcesOff()
    {
        bool canActivating = true;
        var behavior = new PassiveBehavior(
            new ButtonGraphic("A", null!), new ButtonGraphic("D", null!),
            () => true, () => true,
            canActivating: () => canActivating
        );

        behavior.TryUseAbility(0f, AbilityState.Ready, out _); // active = true

        canActivating = false;
        var newState = behavior.Update(AbilityState.Ready);

        Assert.Equal(AbilityState.CoolDown, newState);
        Assert.Equal("A", behavior.Graphic.Text);
    }

    [Fact]
    public void InitializeAndAbilityOff_DoNotThrow()
    {
        var behavior = new PassiveBehavior(
            new ButtonGraphic("A", null!), new ButtonGraphic("D", null!),
            () => true, () => true
        );
        behavior.Initialize(null!);
        behavior.AbilityOff();
    }
}
