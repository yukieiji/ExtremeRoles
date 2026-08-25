using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Ability.Behavior;

public class NullBehaviourTests
{
    [Fact]
    public void Constructor_InitializesDefaultValues()
    {
        var behavior = new NullBehaviour();
        Assert.Equal(0.0f, behavior.CoolTime);
        Assert.Equal("", behavior.Graphic.Text);
        Assert.Null(behavior.Graphic.Img);
    }

    [Fact]
    public void IsUse_ReturnsFalse()
    {
        var behavior = new NullBehaviour();
        Assert.False(behavior.IsUse());
    }

    [Fact]
    public void TryUseAbility_ReturnsFalseAndPreservesState()
    {
        var behavior = new NullBehaviour();
        bool result = behavior.TryUseAbility(0f, AbilityState.Ready, out var newState);

        Assert.False(result);
        Assert.Equal(AbilityState.Ready, newState);
    }

    [Fact]
    public void Update_ReturnsCurrentState()
    {
        var behavior = new NullBehaviour();
        Assert.Equal(AbilityState.Ready, behavior.Update(AbilityState.Ready));
        Assert.Equal(AbilityState.CoolDown, behavior.Update(AbilityState.CoolDown));
        Assert.Equal(AbilityState.None, behavior.Update(AbilityState.None));
    }

    [Fact]
    public void AbilityOffAndForceAbilityOffAndInitialize_DoNotThrow()
    {
        var behavior = new NullBehaviour();
        behavior.AbilityOff();
        behavior.ForceAbilityOff();
        behavior.Initialize(null!);
    }
}
