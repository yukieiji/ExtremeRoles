using System;
using UnityEngine;

namespace ExtremeRoles.Module.Ability.AbilityBehavior;

public sealed class ReclickAbilityBehavior : AbilityBehaviorBase
{
	private Func<bool> ability;
	private Func<bool> canUse;
	private Func<bool> canActivating;
	private Action abilityOff;

	private bool isActive;

	public ReclickAbilityBehavior(
		string text, Sprite img,
		Func<bool> canUse,
		Func<bool> ability,
		Func<bool> canActivating = null,
		Action abilityOff = null) : base(text, img)
	{
		this.ability = ability;
		this.canUse = canUse;

		this.abilityOff = abilityOff;
		this.canActivating = canActivating ?? new Func<bool>(() => { return true; });

		isActive = false;
	}

	public override void Initialize(ActionButton button)
	{
		return;
	}

	public override void AbilityOff()
	{
		isActive = false;
		abilityOff?.Invoke();
	}

	public override void ForceAbilityOff()
	{
		AbilityOff();
	}

	public override bool IsCanAbilityActiving() => canActivating.Invoke();

	public override bool IsUse() => canUse.Invoke() || isActive;

	public override bool TryUseAbility(
		float timer, AbilityState curState, out AbilityState newState)
	{
		newState = curState;

		switch (curState)
		{
			case AbilityState.Ready:
				if (timer <= 0.0f &&
					ability.Invoke())
				{
					newState = AbilityState.Activating;
					isActive = true;
				}
				else
				{
					return false;
				}
				break;
			case AbilityState.Activating:
				if (isActive &&
					timer <= ActiveTime - 0.25f)
				{
					newState = AbilityState.CoolDown;
				}
				else
				{
					return false;
				}
				break;
			default:
				return false;
		}
		return true;
	}

	public override AbilityState Update(AbilityState curState)
	{
		return curState;
	}
}
