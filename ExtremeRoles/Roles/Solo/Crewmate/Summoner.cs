using System;

using UnityEngine;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.AbilityBehavior.Interface;
using ExtremeRoles.Module.AbilityBehavior;

using UnityObject = UnityEngine.Object;
using ExtremeRoles.Module.ButtonAutoActivator;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Summoner :
    SingleRoleBase,
    IRoleAutoBuildAbility
{
	public sealed class SummonerAbilityBehavior :
		AbilityBehaviorBase, ICountBehavior
	{
		public int AbilityCount { get; private set; }
		public bool IsReduceAbilityCount { private get; set; } = false;

		private Func<bool> ability;
		private Func<bool> canUse;
		private TextMeshPro? abilityCountText = null;
		private string buttonTextFormat = ICountBehavior.DefaultButtonCountText;

		public SummonerAbilityBehavior(
			string text, Sprite img,
			Func<bool> canUse,
			Func<bool> ability) : base(text, img)
		{
			this.ability = ability;
			this.canUse = canUse;
		}

		public void SetCountText(string text)
		{
			this.buttonTextFormat = text;
		}

		public override void Initialize(ActionButton button)
		{
			var coolTimerText = button.cooldownTimerText;

			this.abilityCountText = UnityObject.Instantiate(
				coolTimerText, coolTimerText.transform.parent);
			this.abilityCountText.enableWordWrapping = false;
			this.abilityCountText.transform.localScale = Vector3.one * 0.5f;
			this.abilityCountText.transform.localPosition +=
				new Vector3(-0.05f, 0.65f, 0);
			updateAbilityCountText();
		}

		public override void AbilityOff()
		{ }

		public override void ForceAbilityOff()
		{ }

		public override bool IsCanAbilityActiving() => true;

		public override bool IsUse()
			=> this.canUse.Invoke() && this.AbilityCount > 0;

		public override bool TryUseAbility(
			float timer, AbilityState curState, out AbilityState newState)
		{
			newState = curState;

			if (timer > 0 ||
				curState != AbilityState.Ready ||
				this.AbilityCount <= 0)
			{
				return false;
			}

			if (!this.ability.Invoke())
			{
				return false;
			}

			if (this.IsReduceAbilityCount)
			{
				this.reduceAbilityCount();
				this.IsReduceAbilityCount = false;
			}

			newState = this.ActiveTime <= 0.0f ?
				AbilityState.CoolDown : AbilityState.Activating;

			return true;
		}

		public override AbilityState Update(AbilityState curState)
		{
			if (curState == AbilityState.Activating)
			{
				return curState;
			}

			return
				this.AbilityCount > 0 ? curState : AbilityState.None;
		}

		public void SetAbilityCount(int newAbilityNum)
		{
			this.AbilityCount = newAbilityNum;
			updateAbilityCountText();
		}

		public void SetButtonTextFormat(string newTextFormat)
		{
			this.buttonTextFormat = newTextFormat;
		}

		private void reduceAbilityCount()
		{
			--this.AbilityCount;
			updateAbilityCountText();
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


	public ExtremeAbilityButton? Button { get; set; }

    public enum DelusionerOption
    {
        Range,
    }
	private GameData.PlayerInfo? targetData;


    private float range;

    public Summoner() : base(
        ExtremeRoleId.Summoner,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Summoner.ToString(),
        ColorPalette.DelusionerPink,
        false, true, false, false)
    { }

    public void CreateAbility()
    {
		this.Button = new ExtremeAbilityButton(
			new SummonerAbilityBehavior(
				Translation.GetString("SetPortal"),
				null,
				IsAbilityUse, UseAbility),
			new RoleButtonActivator(),
			KeyCode.F);
		this.Button.SetLabelToCrewmate();
	}

    public string GetFakeOptionString() => "";

    public bool IsAbilityUse()
    {
        this.targetData = null;

        PlayerControl target = Player.GetClosestPlayerInRange(
            CachedPlayerControl.LocalPlayer, this,
            this.range);
        if (target == null) { return false; }

		this.targetData = target.Data;

        return IRoleAbility.IsCommonUse();
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
		if (this.targetData != null &&
			this.targetData.IsDead)
		{
			this.targetData = null;
		}
    }

    public bool UseAbility()
    {

        return true;
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {

        CreateFloatOption(
            DelusionerOption.Range,
            2.5f, 0.0f, 7.5f, 0.1f,
            parentOps);

        this.CreateAbilityCountOption(
            parentOps, 3, 25);

    }

    protected override void RoleSpecificInit()
    {
        var allOpt = OptionManager.Instance;
        this.range = allOpt.GetValue<float>(
            GetRoleOptionId(DelusionerOption.Range));
    }
}
