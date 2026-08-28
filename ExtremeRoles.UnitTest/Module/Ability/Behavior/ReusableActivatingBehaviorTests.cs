using ExtremeRoles.UnitTest.Mocks;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Ability.Behavior;

public class ReusableActivatingBehaviorTests : SerialTestBase, IClassFixture<SerialFixture>
{
    public ReusableActivatingBehaviorTests(SerialFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void TryUseAbility_WithActiveTime_SetsActivatingState()
    {
        var behavior = new ReusableActivatingBehavior(
            "Test", null!,
            () => true, () => true
        )
        {
            ActiveTime = 3.0f
        };

        bool success = behavior.TryUseAbility(0f, AbilityState.Ready, out var newState);

        Assert.True(success);
        Assert.Equal(AbilityState.Activating, newState);
    }

    [Fact]
    public void TryUseAbility_WithoutActiveTime_SetsCoolDownState()
    {
        var behavior = new ReusableActivatingBehavior(
            "Test", null!,
            () => true, () => true
        )
        {
            ActiveTime = 0.0f
        };

        bool success = behavior.TryUseAbility(0f, AbilityState.Ready, out var newState);

        Assert.True(success);
        Assert.Equal(AbilityState.CoolDown, newState);
    }

    [Fact]
    public void TryUseAbility_AbnormalCase_ReturnsFalse()
    {
        var behavior = new ReusableActivatingBehavior(
            "Test", null!,
            () => true, () => false
        )
        {
            ActiveTime = 3.0f
        };

        bool success = behavior.TryUseAbility(0f, AbilityState.Ready, out var newState);

        Assert.False(success);
        Assert.Equal(AbilityState.Ready, newState);
    }

    [Fact]
    public void Update_WhenActivating_ReturnsActivating()
    {
        var behavior = new ReusableActivatingBehavior(
            "Test", null!,
            () => true, () => true
        );

        Assert.Equal(AbilityState.Activating, behavior.Update(AbilityState.Activating));
        Assert.Equal(AbilityState.Ready, behavior.Update(AbilityState.Ready));
    }
}