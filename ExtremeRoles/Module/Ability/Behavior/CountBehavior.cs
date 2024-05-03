using System;

using UnityEngine;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability.Behavior.Interface;

namespace ExtremeRoles.Module.Ability.Behavior;

public sealed class CountBehavior : BehaviorBase, ICountBehavior
{
	public int AbilityCount { get; private set; }

	private bool isReduceOnActive;

	private bool isUpdate = false;
	private Func<bool> ability;
	private Func<bool> canUse;
	private Func<bool> canActivating;
	private Action forceAbilityOff;
	private Action abilityOff;

	private TMPro.TextMeshPro abilityCountText = null;
	private string buttonTextFormat = ICountBehavior.DefaultButtonCountText;

	public CountBehavior(
		string text, Sprite img,
		Func<bool> canUse,
		Func<bool> ability,
		Func<bool> canActivating = null,
		Action abilityOff = null,
		Action forceAbilityOff = null,
		bool isReduceOnActive = false) : base(text, img)
	{
		this.ability = ability;
		this.canUse = canUse;
		this.isReduceOnActive = isReduceOnActive;

		this.abilityOff = abilityOff;
		this.forceAbilityOff = forceAbilityOff ?? abilityOff;

		this.canActivating = canActivating ?? new Func<bool>(() => { return true; });
	}

	public void SetCountText(string text)
	{
		buttonTextFormat = text;
	}

	public override void Initialize(ActionButton button)
	{
		var coolTimerText = button.cooldownTimerText;

		abilityCountText = UnityEngine.Object.Instantiate(
			coolTimerText, coolTimerText.transform.parent);
		abilityCountText.enableWordWrapping = false;
		abilityCountText.transform.localScale = Vector3.one * 0.5f;
		abilityCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);
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

	public override bool IsCanAbilityActiving() => canActivating.Invoke();

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

		newState = ActiveTime <= 0.0f ?
			AbilityState.CoolDown : AbilityState.Activating;

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
		abilityCountText.text = string.Format(
			Translation.GetString(buttonTextFormat),
			AbilityCount);
	}
}
