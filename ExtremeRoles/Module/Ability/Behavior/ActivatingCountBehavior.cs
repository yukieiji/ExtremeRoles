using System;

using UnityEngine;

using ExtremeRoles.Module.Ability.Behavior.Interface;

#nullable enable

namespace ExtremeRoles.Module.Ability.Behavior;

public sealed class ActivatingCountBehavior : BehaviorBase, ICountBehavior, IHideLogic, IActivatingBehavior
{
	public float ActiveTime { get; set; }
	public int AbilityCount { get; private set; }
	public bool CanAbilityActiving => this.canActivating.Invoke();

	private readonly Func<bool> canUse;
	private readonly Func<bool> ability;
	private readonly Action? forceAbilityOff;
	private readonly Action? abilityOff;
	private readonly Func<bool> canActivating;
	private readonly bool isReduceOnActive;

	private TMPro.TextMeshPro? abilityCountText = null;
	private string buttonTextFormat = ICountBehavior.DefaultButtonCountText;

	private bool isUpdate = false;
	private bool isActivating;

	public ActivatingCountBehavior(
		string text, Sprite img,
		Func<bool> canUse,
		Func<bool> ability,
		Func<bool>? canActivating = null,
		Action? abilityOff = null,
		Action? forceAbilityOff = null,
		bool isReduceOnActive = false) : base(text, img)
	{
		this.isReduceOnActive = isReduceOnActive;
		this.canActivating = canActivating ?? new Func<bool>(() => { return true; });

		this.ability = ability;
		this.canUse = canUse;

		this.abilityOff = abilityOff;
		this.forceAbilityOff = forceAbilityOff ?? abilityOff;
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

	public override void AbilityOff()
	{
		this.isActivating = false;
		if (!this.isReduceOnActive)
		{
			this.reduceAbilityCount();
		}
		abilityOff?.Invoke();
	}
	public override void ForceAbilityOff()
	{
		this.isActivating = false;
		forceAbilityOff?.Invoke();
	}

	public override bool IsUse()
		=> this.canUse.Invoke() && (this.AbilityCount > 0 || this.isActivating);

	public override AbilityState Update(AbilityState curState)
	{
		if (curState is AbilityState.Activating)
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

		if (this.ActiveTime > 0.0f)
		{
			newState = AbilityState.Activating;
		}

		if (this.isReduceOnActive)
		{
			this.reduceAbilityCount();
		}
		this.isActivating = true;

		return true;
	}

	public override void Initialize(ActionButton button)
	{
		this.abilityCountText = ICountBehavior.CreateCountText(button);
		updateAbilityCountText();
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

		this.abilityCountText.text = Tr.GetString(
			this.buttonTextFormat,
			this.AbilityCount);
	}
}
