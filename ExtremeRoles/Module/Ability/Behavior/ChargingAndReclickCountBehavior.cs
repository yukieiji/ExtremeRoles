using System;

using UnityEngine;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability.Behavior.Interface;

#nullable enable

namespace ExtremeRoles.Module.Ability.Behavior;

public sealed class ChargingAndReclickCountBehavior(
	string text, Sprite img,
	Func<bool, float, bool> canUse,
	Func<bool> onCharge,
	Func<float, bool> ability,
	Func<bool>? canActivating = null,
	Func<bool>? isCharge = null,
	Action? abilityOff = null,
	bool reduceOnCharge = false) :
		BehaviorBase(text, img),
		IChargingBehavior,
		IActivatingBehavior,
		ICountBehavior,
		IReclickBehavior,
		IHideLogic
{
	public int AbilityCount { get; private set; }

	public float ActiveTime { get; set; }

	public bool CanAbilityActiving => this.canActivating.Invoke();

	public float ChargeGage { get; set; }
	public float ChargeTime { get; set; }

	public bool IsCharging => this.isCharging.Invoke();

	private readonly Func<float, bool> ability = ability;
	private readonly Func<bool, float, bool> canUse = canUse;
	private readonly Func<bool> canActivating = canActivating ?? new Func<bool>(() => true);

	private readonly Action? abilityOff = abilityOff;

	private readonly Func<bool> onCharge = onCharge;
	private readonly Func<bool> isCharging = isCharge ?? new Func<bool>(() => true);

	private bool isUpdate = false;
	private bool isCharge = false;
	private bool isActive = false;

	private bool reduceOnCharge = reduceOnCharge;

	private TMPro.TextMeshPro? abilityCountText = null;
	private string buttonTextFormat = ICountBehavior.DefaultButtonCountText;
	private const AbilityState reclickStatus = AbilityState.CoolDown;

	public override void Initialize(ActionButton button)
	{
		this.abilityCountText = ICountBehavior.CreateCountText(button);
		updateAbilityCountText();
	}

	public override void AbilityOff()
	{
		this.isActive = false;
		this.isCharge = false;
		this.abilityOff?.Invoke();
	}

	public override void ForceAbilityOff()
	{
		AbilityOff();
	}

	public override bool IsUse() =>
		(this.AbilityCount > 0 || this.isCharge || this.isActive) &&
		this.canUse.Invoke(this.isCharge, this.ChargeGage);

	public override bool TryUseAbility(
		float timer, AbilityState curState, out AbilityState newState)
	{
		newState = curState;

		switch (curState)
		{
			case AbilityState.Ready:
				if (timer <= 0.0f &&
					this.onCharge.Invoke())
				{
					if (this.reduceOnCharge)
					{
						reduceAbilityCount();
					}
					this.isCharge = true;
					newState = AbilityState.Charging;
				}
				else
				{
					return false;
				}
				break;
			case AbilityState.Charging:
				if ((
						(this.AbilityCount < 1) ||
						(this.reduceOnCharge && this.AbilityCount < 0)
					) ||
					!this.ability.Invoke(this.ChargeGage))
				{
					return false;
				}
				if (!this.reduceOnCharge)
				{
					reduceAbilityCount();
				}
				this.isCharge = false;
				this.isActive = true;
				newState = AbilityState.Activating;
				break;
			case AbilityState.Activating:
				if (this.isActive &&
					IReclickBehavior.CanReClick(timer, this.ActiveTime))
				{
					newState = reclickStatus;
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
		if (curState is AbilityState.Charging or AbilityState.Activating)
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
