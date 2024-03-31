using System;

using UnityEngine;

using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability.Behavior.Interface;

#nullable enable

namespace ExtremeRoles.Module.Ability.Behavior;

public sealed class BatteryBehavior : BehaviorBase, IActivatingBehavior
{
	public float ActiveTime
	{
		get => this.innerActiveTime;
		set
		{
			this.maxCharge = value;
			this.currentCharge = value;
			this.chargeTimer = value;
			this.innerActiveTime = value;
		}
	}

	public bool CanAbilityActiving => this.canActivating.Invoke();

	private readonly Func<bool> ability;
	private readonly Func<bool> canUse;
	private readonly Func<bool> canActivating;
	private readonly Action? abilityOff;
	private readonly Action? forceAbilityOff;

	private bool isActive;

	private float maxCharge;
	private float chargeTimer;
	private float currentCharge;
	private float innerActiveTime;

	public BatteryBehavior(
		string text, Sprite img,
		Func<bool> canUse,
		Func<bool> ability,
		Func<bool>? canActivating = null,
		Action? abilityOff = null,
		Action? forceAbilityOff = null) : base(text, img)
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

	public override void AbilityOff()
	{
		this.isActive = false;
		this.currentCharge = this.maxCharge;
		this.chargeTimer = this.maxCharge;
		this.abilityOff?.Invoke();
		this.innerActiveTime = this.maxCharge;
	}

	public override void ForceAbilityOff()
	{
		currentCharge = Mathf.Clamp(
			chargeTimer, 0.1f, ActiveTime);
		isActive = false;
		forceAbilityOff?.Invoke();
	}

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
			this.chargeTimer = this.currentCharge;
			this.isActive = true;
			this.innerActiveTime = this.chargeTimer;

			newState = this.ActiveTime <= 0.0f ?
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
