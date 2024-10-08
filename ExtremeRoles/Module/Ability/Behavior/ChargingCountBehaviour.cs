﻿using System;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability.Behavior.Interface;

#nullable enable

namespace ExtremeRoles.Module.Ability.Behavior;

public sealed class ChargingCountBehaviour : BehaviorBase, IChargingBehavior, ICountBehavior, IHideLogic
{
	public float ChargeGage { get; set; }
	public float ChargeTime { get; set; }

	public bool IsCharging => this.isCharging.Invoke();

	public int AbilityCount { get; private set; }

	public enum ReduceTiming
	{
		OnActive,
		OnCharge,
	}

	private bool isUpdate = false;
	private bool isCharge = false;
	private TMPro.TextMeshPro? abilityCountText = null;
	private string buttonTextFormat = ICountBehavior.DefaultButtonCountText;

	private readonly Func<bool, float, bool> isUse;
	private readonly Func<float, bool> ability;
	private readonly Func<bool> onCharge;
	private readonly Func<bool> isCharging;
	private readonly ReduceTiming reduceTiming;
	private readonly Action? forceAbilityOff;
	private readonly Action? abilityOff;

	public ChargingCountBehaviour(
		string text, Sprite img,
		Func<bool, float, bool> isUse,
		Func<float, bool> ability,
		Func<bool> onCharge,
		ReduceTiming reduceTiming,
		Func<bool>? isCharge = null,
		Action? abilityOff = null,
		Action? forceAbilityOff = null) : base(text, img)
	{
		this.isUse = isUse;
		this.ability = ability;
		this.onCharge = onCharge;
		this.isCharging = isCharge ?? new Func<bool>(() => { return true; });
		this.reduceTiming = reduceTiming;
		this.abilityOff = abilityOff;
		this.forceAbilityOff = forceAbilityOff;
	}

	public override void AbilityOff()
	{
		this.isCharge = false;
		this.abilityOff?.Invoke();
	}

	public override void ForceAbilityOff()
	{
		this.isCharge = false;
		this.forceAbilityOff?.Invoke();
	}

	public override void Initialize(ActionButton button)
	{
		this.abilityCountText = ICountBehavior.CreateCountText(button);
		updateAbilityCountText();
	}

	public override bool IsUse() =>
		(this.AbilityCount > 0 || this.isCharge) &&
		this.isUse.Invoke(this.isCharge, this.ChargeGage);

	public void SetAbilityCount(int newAbilityNum)
	{
		this.AbilityCount = newAbilityNum;
		this.isUpdate = true;
		updateAbilityCountText();
	}

	public void SetButtonTextFormat(string newTextFormat)
	{
		this.buttonTextFormat = newTextFormat;
	}

	public override bool TryUseAbility(float timer, AbilityState curState, out AbilityState newState)
	{
		newState = curState;
		switch (curState)
		{
			case AbilityState.Ready:
				if (timer > 0 ||
					this.AbilityCount <= 0 ||
					!this.onCharge.Invoke())
				{
					return false;
				}
				if (this.reduceTiming is ReduceTiming.OnCharge)
				{
					reduceAbilityCount();
				}
				this.isCharge = true;
				newState = AbilityState.Charging;
				break;
			case AbilityState.Charging:
				if ((
						(this.AbilityCount < 1) ||
						(this.reduceTiming is ReduceTiming.OnCharge && this.AbilityCount < 0)
					)  ||
					!this.ability.Invoke(this.ChargeGage))
				{
					return false;
				}
				if (this.reduceTiming is ReduceTiming.OnActive)
				{
					reduceAbilityCount();
				}
				this.isCharge = false;
				newState = AbilityState.CoolDown;
				break;
			default:
				return false;
		}
		return true;
	}

	public override AbilityState Update(AbilityState curState)
	{
		if (curState is AbilityState.Charging or AbilityState.Activating)
		{
			return curState;
		}

		if (this.isUpdate)
		{
			this.isUpdate = false;
			return AbilityState.CoolDown;
		}

		return
			this.AbilityCount > 0 ? curState : AbilityState.None;
	}

	private void updateAbilityCountText()
	{
		if (this.abilityCountText == null)
		{
			return;
		}

		this.abilityCountText.text = Tr.GetString(
			this.buttonTextFormat,
			this.AbilityCount);
	}
	private void reduceAbilityCount()
	{
		--this.AbilityCount;
		updateAbilityCountText();
	}

	public void Hide()
	{
		if (this.abilityCountText != null)
		{
			this.abilityCountText.gameObject.SetActive(false);
		}
	}

	public void Show()
	{
		if (this.abilityCountText != null)
		{
			this.abilityCountText.gameObject.SetActive(true);
		}
	}
}
