using System;

namespace ExtremeRoles.Module.AbilityBehavior;

public sealed class PassiveAbilityBehavior : AbilityBehaviorBase
{
    private Func<bool> ability;
    private Func<bool> canUse;
    private Func<bool> canActivating;
    private Action abilityOff;

    private bool isActive;

    private float baseCoolTime;
    private float baseActiveTime;

    private ButtonGraphic activeGraphic;
    private ButtonGraphic deactiveGraphic;

    public PassiveAbilityBehavior(
        ButtonGraphic activeGraphic,
        ButtonGraphic deactiveGraphic,
        Func<bool> canUse,
        Func<bool> ability,
        Func<bool> canActivating = null,
        Action abilityOff = null) : base(activeGraphic)
    {
        this.activeGraphic = activeGraphic;
        this.deactiveGraphic = deactiveGraphic;

        this.ability =  ability;
        this.canUse = canUse;

        this.abilityOff = abilityOff;
        this.canActivating = canActivating ?? new Func<bool>(() => { return true; });

        this.isActive = false;
    }

    public override void SetActiveTime(float newTime)
    {
        base.SetActiveTime(newTime);
        this.baseActiveTime = newTime;
    }

    public override void SetCoolTime(float newTime)
    {
        base.SetCoolTime(newTime);
        this.baseCoolTime = newTime;
    }

    public override void Initialize(ActionButton button)
    {
        return;
    }

    public override void AbilityOff()
    { }

    public override void ForceAbilityOff()
    {
        this.isActive = false;
        this.abilityOff?.Invoke();
        this.SetGraphic(this.activeGraphic);
    }

    public override bool IsCanAbilityActiving() => this.canActivating.Invoke();

    public override bool IsUse()  => this.canUse.Invoke();

    public override bool TryUseAbility(
        float timer, AbilityState curState, out AbilityState newState)
    {
        newState = curState;

        if (timer > 0 || curState != AbilityState.Ready)
        {
            return false;
        }

        if (this.isActive)
        {
            this.ForceAbilityOff();
        }
        else
        {
            if (!this.ability.Invoke())
            {
                return false;
            }
            this.SetGraphic(this.deactiveGraphic);
            this.isActive = true;
        }

        base.SetCoolTime(this.isActive ? this.baseActiveTime : this.baseCoolTime);
        
        newState = AbilityState.CoolDown;

        return true;
    }

    public override AbilityState Update(AbilityState curState)
    {
        return curState;
    }
}
