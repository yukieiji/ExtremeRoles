using UnityEngine;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.CustomOption.Factory;



namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class PsychoKiller :
	SingleRoleBase,
	IRoleUpdate,
	IRoleResetMeeting,
	ITryKillTo
{
	private TextMeshPro combCountText;
	private TextMeshPro timerText;

    private bool isResetMeeting;
    private float reduceRate;
    private float defaultKillCoolTime;

	private bool hasSelfTimer;
	private bool isStartTimer = false;
	private float timer = float.MaxValue;

	private float timerModRate;
	private float defaultTimer;
	private bool isForceRestartWhenMeetingEnd;
	private bool isDiactiveUntilKillWhenMeetingEnd;

    private int combMax;
    private int combCount;

    public enum PsychoKillerOption
    {
        KillCoolReduceRate,
        CombMax,
        CombResetWhenMeeting,
		HasSelfKillTimer,
		SelfKillTimerTime,
		IsForceRestartWhenMeetingEnd,
		IsDiactiveUntilKillWhenMeetingEnd,
		SelfKillTimerModRate,
	}

    public PsychoKiller() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.PsychoKiller),
        true, false, true, true)
    {}

	public void Update(PlayerControl rolePlayer)
	{
		if (!GameProgressSystem.IsTaskPhase ||
			rolePlayer.Data.IsDead ||
			!this.isStartTimer)
		{
			if (this.hasSelfTimer)
			{
				resetTimer();
			}
			if (this.timerText != null)
			{
				this.timerText.gameObject.SetActive(false);
			}
			return;
		}

		if (this.combCountText == null)
		{
			createCombText();
		}

		updateCombText();

		if (!this.hasSelfTimer) { return; }

		if (this.timerText == null)
		{
			createTimerText();
		}

		this.timerText.gameObject.SetActive(true);
		this.timerText.text = $"{Mathf.CeilToInt(this.timer)}";
		this.timer -= Time.deltaTime;

		if (this.timer > 0.0f) { return; }

		this.timer = float.MaxValue;
		// 自爆！！
		Player.RpcUncheckMurderPlayer(
			rolePlayer.PlayerId,
			rolePlayer.PlayerId,
			byte.MaxValue);
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
		if (this.hasSelfTimer)
		{
			this.isStartTimer =
			(
				this.isForceRestartWhenMeetingEnd ||
				(
					!this.isDiactiveUntilKillWhenMeetingEnd && this.isStartTimer
				)
			);
			resetTimer();
		}
    }

    public void ResetOnMeetingStart()
    {
		if (this.hasSelfTimer)
		{
			this.timer = float.MaxValue;
		}
        this.KillCoolTime = this.defaultKillCoolTime;
        if (this.isResetMeeting)
        {
            this.combCount = 0;
        }
        else
        {
			clampCombCount();
		}
    }

    public override string GetFullDescription()
    {
        return string.Format(
            base.GetFullDescription(),
            this.combCount);
    }


    public bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
		if (this.combCount < this.combMax)
		{
			++this.combCount;
			clampCombCount();

			this.KillCoolTime = this.KillCoolTime * (
				(100f - (this.reduceRate * this.combCount)) / 100f);
			this.KillCoolTime = Mathf.Clamp(
				this.KillCoolTime, 0.1f, this.defaultKillCoolTime);
		}

		if (this.hasSelfTimer)
		{
			this.isStartTimer = true;
			resetTimer();
		}
        return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateNewIntOption(
            PsychoKillerOption.KillCoolReduceRate,
            5, 1, 15, 1,
            format: OptionUnit.Percentage);

        factory.CreateNewIntOption(
            PsychoKillerOption.CombMax,
            2, 1, 5, 1);

        factory.CreateNewBoolOption(
            PsychoKillerOption.CombResetWhenMeeting,
            true);

		var hasSelfKillTimer = factory.CreateNewBoolOption(
			PsychoKillerOption.HasSelfKillTimer,
			false);
		factory.CreateFloatOption(
			PsychoKillerOption.SelfKillTimerTime,
			30.0f, 5.0f, 120.0f, 0.5f,
			hasSelfKillTimer,
			format: OptionUnit.Second);
		var timerOpt = factory.CreateBoolOption(
			PsychoKillerOption.IsForceRestartWhenMeetingEnd,
			false, hasSelfKillTimer);
		factory.CreateBoolOption(
			PsychoKillerOption.IsDiactiveUntilKillWhenMeetingEnd,
			false, timerOpt,
			invert: true);
		factory.CreateIntOption(
			PsychoKillerOption.SelfKillTimerModRate,
			0, -50, 50, 1, hasSelfKillTimer,
			format: OptionUnit.Percentage);
	}

    protected override void RoleSpecificInit()
    {

        if (!this.HasOtherKillCool)
        {
            this.HasOtherKillCool = true;
            this.KillCoolTime = Player.DefaultKillCoolTime;
        }

        var cate = this.Loader;

        this.reduceRate = cate.GetValue<PsychoKillerOption, int>(
            PsychoKillerOption.KillCoolReduceRate);
        this.isResetMeeting = cate.GetValue<PsychoKillerOption, bool>(
            PsychoKillerOption.CombResetWhenMeeting);
        this.combMax= cate.GetValue<PsychoKillerOption, int>(
            PsychoKillerOption.CombMax);

		this.hasSelfTimer = cate.GetValue<PsychoKillerOption, bool>(
			PsychoKillerOption.HasSelfKillTimer);
		this.defaultTimer = cate.GetValue<PsychoKillerOption, float>(
			PsychoKillerOption.SelfKillTimerTime);
		this.isForceRestartWhenMeetingEnd = cate.GetValue<PsychoKillerOption, bool>(
			PsychoKillerOption.IsForceRestartWhenMeetingEnd);
		this.isDiactiveUntilKillWhenMeetingEnd = cate.GetValue<PsychoKillerOption, bool>(
			PsychoKillerOption.IsDiactiveUntilKillWhenMeetingEnd);

		this.timerModRate = (100.0f - (float)cate.GetValue<PsychoKillerOption, int>(
			PsychoKillerOption.SelfKillTimerModRate)) / 100.0f;
		if (this.hasSelfTimer)
		{
			this.timer = this.defaultTimer;
		}

		this.combCount = 0;
        this.defaultKillCoolTime = this.KillCoolTime;
    }

	private void createCombText()
	{
		if (HudManager.Instance == null) { return; }

		this.combCountText = ICountBehavior.CreateCountText(
			HudManager.Instance.KillButton);
		this.combCountText.name = ExtremeAbilityButton.AditionalInfoName;
		this.combCountText.gameObject.SetActive(true);
	}

	private void createTimerText()
	{
		if (HudManager.Instance == null) { return; }

		var hudManager = HudManager.Instance;
		var killButton = HudManager.Instance.KillButton;

		this.timerText = Object.Instantiate(
			killButton.cooldownTimerText,
			killButton.transform.parent);
		this.timerText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
		this.timerText.transform.localPosition =
			hudManager.UseButton.transform.localPosition + new Vector3(-2.0f, -0.125f, 0);
		this.timerText.gameObject.SetActive(true);

		var timerInfoText = Object.Instantiate(
			hudManager.KillButton.cooldownTimerText,
			this.timerText.transform);
		timerInfoText.enableWordWrapping = false;
		timerInfoText.transform.localScale = Vector3.one * 0.5f;
		timerInfoText.transform.localPosition += new Vector3(-0.05f, 0.6f, 0);
		timerInfoText.gameObject.SetActive(true);

		timerInfoText.text = Tr.GetString("untilSelfKill");
	}

	private void updateCombText()
	{
		this.combCountText.text = Tr.GetString("curCombNum", this.combCount);
	}

	private void resetTimer()
	{
		this.timer = this.defaultTimer * Mathf.Pow(this.timerModRate, this.combCount);
	}

	private void clampCombCount()
	{
		this.combCount = Mathf.Clamp(this.combCount, 0, this.combMax);
	}
}
