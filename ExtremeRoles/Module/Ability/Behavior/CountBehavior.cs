using System;

using UnityEngine;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability.Behavior.Interface;

#nullable enable

namespace ExtremeRoles.Module.Ability.Behavior;

public class CountBehavior : BehaviorBase, ICountBehavior
{
	public int AbilityCount { get; private set; }

	private bool isReduceOnActive;

	private bool isUpdate = false;
	private readonly Func<bool> ability;
	private readonly Func<bool> canUse;
	private readonly Action? forceAbilityOff;
	private readonly Action? abilityOff;

	private TMPro.TextMeshPro? abilityCountText = null;
	private string buttonTextFormat = ICountBehavior.DefaultButtonCountText;

	public CountBehavior(
		string text, Sprite img,
		Func<bool> canUse,
		Func<bool> ability,
		Action? abilityOff = null,
		Action? forceAbilityOff = null,
		bool isReduceOnActive = false) : base(text, img)
	{
		this.ability = ability;
		this.canUse = canUse;
		this.isReduceOnActive = isReduceOnActive;

		this.abilityOff = abilityOff;
		this.forceAbilityOff = forceAbilityOff ?? abilityOff;
	}

	public void SetCountText(string text)
	{
		buttonTextFormat = text;
	}

	public override void Initialize(ActionButton button)
	{
		this.abilityCountText = ICountBehavior.CreateCountText(button);
		updateAbilityCountText();
	}

	public override void AbilityOff()
	{
		if (!isReduceOnActive)
		{
			reduceAbilityCount();
		}
		abilityOff?.Invoke();
	}

	public override void ForceAbilityOff()
	{
		forceAbilityOff?.Invoke();
	}

	public override bool IsUse()
		=> canUse.Invoke() && AbilityCount > 0;

	public override bool TryUseAbility(
		float timer, AbilityState curState, out AbilityState newState)
	{
		newState = curState;

		if (timer > 0 ||
			curState != AbilityState.Ready ||
			AbilityCount <= 0)
		{
			return false;
		}

		if (!ability.Invoke())
		{
			return false;
		}

		if (isReduceOnActive)
		{
			reduceAbilityCount();
		}

		newState = AbilityState.CoolDown;

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
