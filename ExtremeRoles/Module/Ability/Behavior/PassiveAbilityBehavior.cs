using System;

namespace ExtremeRoles.Module.Ability.Behavior;

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

		this.ability = ability;
		this.canUse = canUse;

		this.abilityOff = abilityOff;
		this.canActivating = canActivating ?? new Func<bool>(() => { return true; });

		isActive = false;
	}

	public override void SetActiveTime(float newTime)
	{
		base.SetActiveTime(newTime);
		baseActiveTime = newTime;
	}

	public override void SetCoolTime(float newTime)
	{
		base.SetCoolTime(newTime);
		baseCoolTime = newTime;
	}

	public override void Initialize(ActionButton button)
	{
		return;
	}

	public override void AbilityOff()
	{ }

	public override void ForceAbilityOff()
	{
		isActive = false;
		abilityOff?.Invoke();
		SetGraphic(activeGraphic);
		base.SetCoolTime(baseCoolTime);
	}

	public override bool IsCanAbilityActiving() => canActivating.Invoke();

	public override bool IsUse() => canUse.Invoke();

	public override bool TryUseAbility(
		float timer, AbilityState curState, out AbilityState newState)
	{
		newState = curState;

		if (timer > 0 || curState != AbilityState.Ready)
		{
			return false;
		}

		if (isActive)
		{
			ForceAbilityOff();
		}
		else
		{
			if (!ability.Invoke())
			{
				return false;
			}
			SetGraphic(deactiveGraphic);
			isActive = true;
		}

		base.SetCoolTime(isActive ? baseActiveTime : baseCoolTime);

		newState = AbilityState.CoolDown;

		return true;
	}

	public override AbilityState Update(AbilityState curState)
	{
		if (isActive && !IsCanAbilityActiving())
		{
			ForceAbilityOff();
			curState = AbilityState.CoolDown;
		}
		return curState;
	}
}
