using AmongUs.GameOptions;

using UnityEngine;
using TMPro;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class PsychoKiller :
	SingleRoleBase,
	IRoleUpdate,
	IRoleResetMeeting
{
	private TextMeshPro combCountText;

    private bool isResetMeeting;
    private float reduceRate;
    private float defaultKillCoolTime;

    private int combMax;
    private int combCount;

    public enum PsychoKillerOption
    {
        KillCoolReduceRate,
        CombMax,
        CombResetWhenMeeting,
		HasSelfKillTimer,
		SelfKillTimerTime,
		IsRestartWhenMeetingEnd,
		IsCombSyncSelfKillTimer,
		SelfKillTimerModRate,
	}

    public PsychoKiller() : base(
        ExtremeRoleId.PsychoKiller,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.PsychoKiller.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    {}

	public void Update(PlayerControl rolePlayer)
	{
		if (rolePlayer == null ||
			GameData.Instance == null ||
			CachedShipStatus.Instance == null ||
			!CachedShipStatus.Instance.enabled ||
			ExileController.Instance ||
			MeetingHud.Instance) { return; }

		if (this.combCountText == null)
		{
			createCombText();
		}

		updateCombText();

	}

	public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        this.KillCoolTime = this.defaultKillCoolTime;
        if (this.isResetMeeting)
        {
            this.combCount = 1;
        }
        else
        {
            if(this.combCount >= this.combMax)
            {
                this.combCount = this.combMax;
            }
        }
    }

    public override string GetFullDescription()
    {
        return string.Format(
            base.GetFullDescription(),
            this.combCount - 1);
    }


    public override bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        if (this.combMax >= this.combCount)
        {
            this.KillCoolTime = this.KillCoolTime * (
                (100f - (this.reduceRate * this.combCount)) / 100f);
            this.KillCoolTime = Mathf.Clamp(
                this.KillCoolTime, 0.1f, this.defaultKillCoolTime);
            ++this.combCount;
        }
        return true;
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        CreateIntOption(
            PsychoKillerOption.KillCoolReduceRate,
            5, 1, 15, 1, parentOps,
            format: OptionUnit.Percentage);

        CreateIntOption(
            PsychoKillerOption.CombMax,
            2, 1, 5, 1,
            parentOps);

        CreateBoolOption(
            PsychoKillerOption.CombResetWhenMeeting,
            true, parentOps);

		var hasSelfKillTimer = CreateBoolOption(
			PsychoKillerOption.HasSelfKillTimer,
			false, parentOps);
		CreateFloatOption(
			PsychoKillerOption.SelfKillTimerTime,
			30.0f, 5.0f, 120.0f, 0.5f,
			hasSelfKillTimer,
			format: OptionUnit.Second);
		CreateBoolOption(
			PsychoKillerOption.IsRestartWhenMeetingEnd,
			true, hasSelfKillTimer);
		var isCombSyncKillTimer = CreateBoolOption(
			PsychoKillerOption.IsCombSyncSelfKillTimer,
			false, hasSelfKillTimer);
		CreateIntOption(
			PsychoKillerOption.SelfKillTimerModRate,
			-25, -50, 50, 1, isCombSyncKillTimer,
			format: OptionUnit.Percentage);

	}

    protected override void RoleSpecificInit()
    {

        if (!this.HasOtherKillCool)
        {
            this.HasOtherKillCool = true;
            this.KillCoolTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
                FloatOptionNames.KillCooldown);
        }

        var allOption = OptionManager.Instance;

        this.reduceRate = allOption.GetValue<int>(
            GetRoleOptionId(PsychoKillerOption.KillCoolReduceRate));
        this.isResetMeeting = allOption.GetValue<bool>(
            GetRoleOptionId(PsychoKillerOption.CombResetWhenMeeting));
        this.combMax= allOption.GetValue<int>(
            GetRoleOptionId(PsychoKillerOption.CombMax));

        this.combCount = 1;
        this.defaultKillCoolTime = this.KillCoolTime;
    }

	private void createCombText()
	{
		if (FastDestroyableSingleton<HudManager>.Instance == null) { return; }

		var killButton = FastDestroyableSingleton<HudManager>.Instance.KillButton;

		this.combCountText = Object.Instantiate(
			killButton.cooldownTimerText,
			killButton.cooldownTimerText.transform.parent);

		updateCombText();

		this.combCountText.name = ExtremeAbilityButton.AditionalInfoName;
		this.combCountText.enableWordWrapping = false;
		this.combCountText.transform.localScale = Vector3.one * 0.5f;
		this.combCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);
		this.combCountText.gameObject.SetActive(true);
	}

	private void updateCombText()
	{
		this.combCountText.text = string.Format(
			"現在：{0}コンボ", this.combCount - 1);
	}
}
