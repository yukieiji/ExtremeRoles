using System;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.Ability.Behavior;

public class ReusableBehavior : BehaviorBase
{
	private readonly Func<bool> ability;
	private readonly Func<bool> canUse;
	private readonly Action? forceAbilityOff;
	private readonly Action? abilityOff;

	public ReusableBehavior(
		string text, Sprite img,
		Func<bool> canUse,
		Func<bool> ability,
		Action? abilityOff = null,
		Action? forceAbilityOff = null) : base(text, img)
	{
		this.ability = ability;
		this.canUse = canUse;

		this.abilityOff = abilityOff;
		this.forceAbilityOff = forceAbilityOff ?? abilityOff;
	}

	public override void Initialize(ActionButton button)
	{
		return;
	}

	public override void AbilityOff()
	{
		abilityOff?.Invoke();
	}

	public override void ForceAbilityOff()
	{
		forceAbilityOff?.Invoke();
	}

	public override bool IsUse() => this.canUse.Invoke();

	public override bool TryUseAbility(
		float timer, AbilityState curState, out AbilityState newState)
	{
		newState = curState;

		if (timer > 0 || curState != AbilityState.Ready)
		{
			return false;
		}

		if (!ability.Invoke())
		{
			return false;
		}

		newState = AbilityState.CoolDown;

		return true;
	}

	public override AbilityState Update(AbilityState curState)
	{
		return curState;
	}
}
