﻿using System;

using UnityEngine;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability.Behavior.Interface;

#nullable enable

namespace ExtremeRoles.Module.Ability.Behavior;

public sealed class ReclickCountBehavior : BehaviorBase, IActivatingBehavior, ICountBehavior
{
	public int AbilityCount { get; private set; }

	public float ActiveTime { get; set; }

	public bool CanAbilityActiving => this.canActivating.Invoke();

	private bool isUpdate = false;
	private readonly Func<bool> ability;
	private readonly Func<bool> canUse;
	private readonly Func<bool> canActivating;
	private readonly Action? abilityOff;

	private bool isActive;

	private TMPro.TextMeshPro? abilityCountText = null;
	private string buttonTextFormat = ICountBehavior.DefaultButtonCountText;

	public ReclickCountBehavior(
		string text, Sprite img,
		Func<bool> canUse,
		Func<bool> ability,
		Func<bool>? canActivating = null,
		Action? abilityOff = null) : base(text, img)
	{
		this.ability = ability;
		this.canUse = canUse;

		this.abilityOff = abilityOff;
		this.canActivating = canActivating ?? new Func<bool>(() => { return true; });

		isActive = false;
	}

	public override void Initialize(ActionButton button)
	{
		this.abilityCountText = ICountBehavior.CreateCountText(button);
		updateAbilityCountText();
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

	public override bool IsUse() =>
		canUse.Invoke() && AbilityCount > 0 || isActive;

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
					reduceAbilityCount();
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
		if (curState == AbilityState.Activating)
		{
			return curState;
		}

		if (isUpdate)
		{
			isUpdate = false;
			return AbilityState.CoolDown;
		}

		return
			AbilityCount > 0 ? curState : AbilityState.None;
	}

	public void SetAbilityCount(int newAbilityNum)
	{
		AbilityCount = newAbilityNum;
		isUpdate = true;
		updateAbilityCountText();
	}

	public void SetButtonTextFormat(string newTextFormat)
	{
		buttonTextFormat = newTextFormat;
	}

	private void reduceAbilityCount()
	{
		--AbilityCount;
		if (abilityCountText != null)
		{
			updateAbilityCountText();
		}
	}

	private void updateAbilityCountText()
	{
		if (this.abilityCountText == null)
		{
			return;
		}

		this.abilityCountText.text = string.Format(
			Translation.GetString(this.buttonTextFormat),
			this.AbilityCount);
	}
}
