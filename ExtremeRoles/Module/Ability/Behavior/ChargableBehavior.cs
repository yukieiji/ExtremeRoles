using ExtremeRoles.Performance;
using System;

using UnityEngine;

namespace ExtremeRoles.Module.Ability.Behavior;

public sealed class ChargableBehavior : BehaviorBase
{
	private Func<bool> ability;
	private Func<bool> canUse;
	private Func<bool> canActivating;
	private Action abilityOff;
	private Action forceAbilityOff;

	private bool isActive;

	private float maxCharge;
	private float chargeTimer;
	private float currentCharge;

	public ChargableBehavior(
		string text, Sprite img,
		Func<bool> canUse,
		Func<bool> ability,
		Func<bool> canActivating = null,
		Action abilityOff = null,
		Action forceAbilityOff = null) : base(text, img)
	{
		this.ability = ability;
		this.canUse = canUse;

		this.abilityOff = abilityOff;
		this.forceAbilityOff = forceAbilityOff ?? abilityOff;
		this.canActivating = canActivating ?? new Func<bool>(() => { return true; });

		isActive = false;
	}

	public override void Initialize(ActionButton button)
	{
		return;
	}

	public override void SetActiveTime(float time)
	{
		maxCharge = time;
		currentCharge = time;
		chargeTimer = time;
		base.SetActiveTime(time);
	}

	public override void AbilityOff()
	{
		isActive = false;
		currentCharge = maxCharge;
		chargeTimer = maxCharge;
		abilityOff?.Invoke();
		base.SetActiveTime(maxCharge);
	}

	public override void ForceAbilityOff()
	{
		currentCharge = Mathf.Clamp(
			chargeTimer, 0.1f, ActiveTime);
		isActive = false;
		forceAbilityOff?.Invoke();
	}

	public override bool IsCanAbilityActiving() => canActivating.Invoke();

	public override bool IsUse() =>
		(canUse.Invoke() || isActive) && currentCharge > 0.0f;

	public override bool TryUseAbility(
		float timer, AbilityState curState, out AbilityState newState)
	{
		newState = curState;
		bool result = false;
		if (curState == AbilityState.Activating)
		{
			ForceAbilityOff();
			newState = AbilityState.Ready;
			isActive = false;
			result = true;
		}
		else if (
			timer <= 0f &&
			curState == AbilityState.Ready &&
			ability.Invoke())
		{
			chargeTimer = currentCharge;
			isActive = true;
			base.SetActiveTime(chargeTimer);
			newState = ActiveTime <= 0.0f ?
				AbilityState.CoolDown : AbilityState.Activating;
			result = true;
		}
		return result;
	}

	public override AbilityState Update(AbilityState curState)
	{
		if (isActive)
		{
			chargeTimer -= Time.deltaTime;
		}
		if (CachedPlayerControl.LocalPlayer.PlayerControl.AllTasksCompleted())
		{
			currentCharge = maxCharge;
		}
		return curState;
	}
}
