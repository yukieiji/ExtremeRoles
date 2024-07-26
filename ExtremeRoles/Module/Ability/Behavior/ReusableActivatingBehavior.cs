using System;

using UnityEngine;

using ExtremeRoles.Module.Ability.Behavior.Interface;

#nullable enable

namespace ExtremeRoles.Module.Ability.Behavior;

public sealed class ReusableActivatingBehavior : ReusableBehavior, IActivatingBehavior
{
	private readonly Func<bool> canActivating;

	public float ActiveTime { get; set; }
	public bool CanAbilityActiving => this.canActivating.Invoke();

	public ReusableActivatingBehavior(
		string text, Sprite img,
		Func<bool> canUse,
		Func<bool> ability,
		Func<bool>? canActivating = null,
		Action? abilityOff = null,
		Action? forceAbilityOff = null) : base(
			text, img,
			canUse, ability,
			abilityOff, forceAbilityOff)
	{
		this.canActivating = canActivating ?? new Func<bool>(() => { return true; });
	}

	public override AbilityState Update(AbilityState curState)
	{
		if (curState is AbilityState.Activating)
		{
			return curState;
		}

		return base.Update(curState);
	}

	public override bool TryUseAbility(
		float timer, AbilityState curState, out AbilityState newState)
	{
		base.TryUseAbility(timer, curState, out newState);

		if (this.ActiveTime > 0.0f)
		{
			newState = AbilityState.Activating;
		}

		return true;
	}
}
