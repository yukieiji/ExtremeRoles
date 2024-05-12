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

	private bool isActivating;
	private bool isReduceOnActive;

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
			forceAbilityOff)
	{
		this.isReduceOnActive = isReduceOnActive;
		this.canActivating = canActivating ?? new Func<bool>(() => { return true; });
	}

	public override void AbilityOff()
	{
		this.isActivating = false;
		if (!this.isReduceOnActive)
		{
			this.ReduceAbilityCount();
		}
		base.AbilityOff();
	}
	public override void ForceAbilityOff()
	{
		this.isActivating = false;
		base.ForceAbilityOff();
	}

	public override bool IsUse()
		=> this.CanUse.Invoke() && (this.AbilityCount > 0 || this.isActivating);

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

		if (this.isReduceOnActive)
		{
			this.ReduceAbilityCount();
		}
		this.isActivating = true;

		return true;
	}
}
