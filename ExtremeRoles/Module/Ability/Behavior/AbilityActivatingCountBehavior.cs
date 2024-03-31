using System;

using UnityEngine;

using ExtremeRoles.Module.Ability.Behavior.Interface;

#nullable enable

namespace ExtremeRoles.Module.Ability.Behavior;

public sealed class ActivatingCountBehavior : CountBehavior, IActivatingBehavior
{
	public float ActiveTime { get; set; }

	public bool CanAbilityActiving => this.canActivating.Invoke();
	private Func<bool> canActivating;

	public ActivatingCountBehavior(
		string text, Sprite img,
		Func<bool> canUse,
		Func<bool> ability,
		Func<bool>? canActivating = null,
		Action? abilityOff = null,
		Action? forceAbilityOff = null,
		bool isReduceOnActive = false) : base(
			text, img, canUse,
			ability, abilityOff,
			forceAbilityOff,
			isReduceOnActive)
	{
		this.canActivating = canActivating ?? new Func<bool>(() => { return true; });
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
