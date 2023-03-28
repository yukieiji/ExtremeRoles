using System;

using UnityEngine;
using ExtremeRoles.Module.AbilityBehavior.Interface;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Module.AbilityBehavior;

public sealed class ReclickAbilityCountBehavior : AbilityBehaviorBase, ICountBehavior
{
    public int AbilityCount { get; private set; }

    private bool isUpdate = false;
    private Func<bool> ability;
    private Func<bool> canUse;
    private Func<bool> canActivating;
    private Action abilityOff;

    private bool isActive;

    private TMPro.TextMeshPro abilityCountText = null;
    private string buttonTextFormat = ICountBehavior.DefaultButtonCountText;

    public ReclickAbilityCountBehavior(
        string text, Sprite img,
        Func<bool> canUse,
        Func<bool> ability,
        Func<bool> canActivating = null,
        Action abilityOff = null) : base(text, img)
    {
        this.ability =  ability;
        this.canUse = canUse;

        this.abilityOff = abilityOff;
        this.canActivating = canActivating ?? new Func<bool>(() => { return true; });

        this.isActive = false;
    }

    public override void Initialize(ActionButton button)
    {
        var coolTimerText = button.cooldownTimerText;

        this.abilityCountText = UnityEngine.Object.Instantiate(
            coolTimerText, coolTimerText.transform.parent);
        this.abilityCountText.enableWordWrapping = false;
        this.abilityCountText.transform.localScale = Vector3.one * 0.5f;
        this.abilityCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);
        updateAbilityCountText();
    }

    public override void AbilityOff()
    {
        this.isActive = false;
        this.abilityOff?.Invoke();
    }

    public override void ForceAbilityOff()
    {
        this.AbilityOff();
    }

    public override bool IsCanAbilityActiving() => this.canActivating.Invoke();

    public override bool IsUse() => 
        (this.canUse.Invoke() && this.AbilityCount > 0) || this.isActive;

    public override bool TryUseAbility(
        float timer, AbilityState curState, out AbilityState newState)
    {
        newState = curState;

        switch (curState)
        {
            case AbilityState.Ready:
                if (timer <= 0.0f &&
                    this.ability.Invoke())
                {
                    newState = AbilityState.Activating;
                    this.isActive = true;
                    this.reduceAbilityCount();
                }
                else
                {
                    return false;
                }
                break;
            case AbilityState.Activating:
                if (this.isActive)
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

        if (this.isUpdate)
        {
            this.isUpdate = false;
            return AbilityState.CoolDown;
        }

        return
            this.AbilityCount > 0 ? curState : AbilityState.None;
    }

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

    private void reduceAbilityCount()
    {
        --this.AbilityCount;
        if (this.abilityCountText != null)
        {
            updateAbilityCountText();
        }
    }

    private void updateAbilityCountText()
    {
        this.abilityCountText.text = string.Format(
            Translation.GetString(this.buttonTextFormat),
            this.AbilityCount);
    }
}
