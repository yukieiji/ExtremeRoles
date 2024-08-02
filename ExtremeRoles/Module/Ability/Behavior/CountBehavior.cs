using System;

using UnityEngine;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability.Behavior.Interface;

#nullable enable

namespace ExtremeRoles.Module.Ability.Behavior;

public class CountBehavior : BehaviorBase, ICountBehavior, IHideLogic
{
	public int AbilityCount { get; private set; }
	protected readonly Func<bool> CanUse;

	private bool isUpdate = false;

	private readonly Func<bool> ability;
	private readonly Action? forceAbilityOff;
	private readonly Action? abilityOff;

	private TMPro.TextMeshPro? abilityCountText = null;
	private string buttonTextFormat = ICountBehavior.DefaultButtonCountText;

	public CountBehavior(
		string text, Sprite img,
		Func<bool> canUse,
		Func<bool> ability,
		Action? abilityOff = null,
		Action? forceAbilityOff = null) : base(text, img)
	{
		this.ability = ability;
		this.CanUse = canUse;

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
		abilityOff?.Invoke();
	}

	public override void ForceAbilityOff()
	{
		forceAbilityOff?.Invoke();
	}

	public override bool IsUse()
		=> CanUse.Invoke() && AbilityCount > 0;

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

		ReduceAbilityCount();

		newState = AbilityState.CoolDown;

		return true;
	}

	public override AbilityState Update(AbilityState curState)
	{
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

	protected void ReduceAbilityCount()
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

		this.abilityCountText.text = Tr.GetString(
			this.buttonTextFormat,
			this.AbilityCount);
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
