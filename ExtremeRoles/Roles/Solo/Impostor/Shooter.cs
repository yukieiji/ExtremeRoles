﻿using System;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Performance.Il2Cpp;

using TMPro;

using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Shooter :
    SingleRoleBase,
    IRoleMeetingButtonAbility,
    IRoleReportHook,
    IRoleResetMeeting,
    IRoleAwake<RoleTypes>
{
    public enum ShooterOption
    {
        AwakeKillNum,
        AwakeImpNum,
        IsInitAwake,
        NoneAwakeWhenShoot,
        ShootKillCoolPenalty,
        CanCallMeeting,
        CanShootSelfCallMeeting,
        MaxShootNum,
        InitShootNum,
        MaxMeetingShootNum,
        ShootChargeTime,
        ShootKillNum
    }

    public bool IsAwake => this.isAwake;

    public RoleTypes NoneAwakeRole => RoleTypes.Impostor;
	public Sprite AbilityImage => HudManager.Instance.KillButton.graphic.sprite;

	private bool isAwake = false;

    private int curKillCount = 0;
    private int awakeKillCount = 0;
    private int awakeImpNum = 0;

    private bool isAwakedHasOtherVision;
    private bool isAwakedHasOtherKillCool;
    private bool isAwakedHasOtherKillRange;

    private float killCoolPenalty = 0.0f;

    private float chargeTime = 0.0f;
    private int chargeKillNum = 0;

    private float timer = float.MaxValue;
    private int maxShootNum = 0;
    private int curShootNum = 0;
    private int maxMeetingShootNum = 0;
    private int shootCounter = 0;

    private bool isNoneAwakeWhenShoot = false;
    private bool canShootThisMeeting = false;
    private bool canShootSelfCallMeeting = false;

    private bool awakedCallMeeting = false;

    private TextMeshPro chargeInfoText = null;
    private TextMeshPro chargeTimerText = null;
    private TextMeshPro meetingShootText = null;

    public Shooter(): base(
        ExtremeRoleId.Shooter,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.Shooter.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    { }

    public string GetFakeOptionString() => "";


    public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance)
    {
        byte target = instance.TargetPlayerId;

        return
            this.curShootNum <= 0 ||
            !(
                this.shootCounter < this.maxMeetingShootNum &&
                this.canShootThisMeeting
            ) ||
            target == 253 ||
            ExtremeRoleManager.GameRole[target].Id == ExtremeRoleId.Assassin;
    }

    public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
		=> IRoleMeetingButtonAbility.DefaultButtonMod(instance, abilityButton, "shooterKill");

    public Action CreateAbilityAction(PlayerVoteArea instance)
    {

        byte target = instance.TargetPlayerId;

        void shooterKill()
        {
            if (instance.AmDead) { return; }
            Shoot();
            PlayerControl localPlayer = PlayerControl.LocalPlayer;

            if (BodyGuard.IsBlockMeetingKill &&
                BodyGuard.TryRpcKillGuardedBodyGuard(
                    localPlayer.PlayerId, target))
            {
                rpcPlayKillSound();
                return;
            }

            Player.RpcUncheckMurderPlayer(
                localPlayer.PlayerId,
                target, byte.MinValue);

            rpcPlayKillSound();
        }

        return shooterKill;
    }

    private static void rpcPlayKillSound()
    {
        Sound.RpcPlaySound(Sound.Type.Kill);
    }

    public void HookReportButton(
        PlayerControl rolePlayer, NetworkedPlayerInfo reporter)
    {
        this.canShootThisMeeting = true;
        if (rolePlayer.PlayerId == reporter.PlayerId)
        {
            this.canShootThisMeeting = this.canShootSelfCallMeeting;
        }
    }

    public void HookBodyReport(
        PlayerControl rolePlayer, NetworkedPlayerInfo reporter, NetworkedPlayerInfo reportBody)
    {
        this.canShootThisMeeting = true;
    }
    public void Shoot()
    {
        API.Extension.State.RoleState.AddKillCoolOffset(
            this.killCoolPenalty);
        this.curShootNum = this.curShootNum - 1;
        this.shootCounter  = this.shootCounter + 1;
        if (this.isNoneAwakeWhenShoot)
        {
            this.isAwake = false;
            this.curKillCount = 0;
            this.HasOtherVision = false;
            this.HasOtherKillCool = false;
            this.HasOtherKillRange = false;
            this.CanCallMeeting = true;
        }
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        chargeInfoSetActive(true);
        this.canShootThisMeeting = true;
    }

    public void ResetOnMeetingStart()
    {
        this.shootCounter = 0;
        if (meetingShootText != null)
        {
            meetingShootText.gameObject.SetActive(false);
        }
        chargeInfoSetActive(false);
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (ShipStatus.Instance == null ||
            GameData.Instance == null) { return; }

        if (!this.isAwake)
        {
            meetingInfoSetActive(false);
            chargeInfoSetActive(false);

            int impNum = 0;

            foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (ExtremeRoleManager.GameRole[player.PlayerId].IsImpostor() &&
                    (!player.IsDead && !player.Disconnected))
                {
                    ++impNum;
                }
            }

            if (this.awakeImpNum >= impNum &&
                this.curKillCount >= this.awakeKillCount)
            {
                this.isAwake = true;
                this.HasOtherVision = this.isAwakedHasOtherVision;
                this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
                this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
                this.CanCallMeeting = this.awakedCallMeeting;
                this.curKillCount = 0;
            }
            return;
        }
        if (rolePlayer.Data.IsDead || rolePlayer.Data.Disconnected)
        {
            this.curShootNum = 0;
            this.shootCounter = int.MaxValue;
            this.timer = this.chargeTime;
            chargeInfoSetActive(false);
            return;
        }
        if (MeetingHud.Instance)
        {
            if (meetingShootText == null)
            {
                meetingShootText = UnityEngine.Object.Instantiate(
                    HudManager.Instance.TaskPanel.taskText,
                    MeetingHud.Instance.transform);
                meetingShootText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                meetingShootText.transform.position = Vector3.zero;
                meetingShootText.transform.localPosition = new Vector3(
                    -2.85f, 3.15f, -20f);
                meetingShootText.transform.localScale *= 0.9f;
                meetingShootText.color = Palette.White;
                meetingShootText.gameObject.SetActive(false);
            }

            meetingShootText.text =Tr.GetString(
				"shooterShootStatus",
                this.curShootNum, this.maxShootNum,
                this.maxMeetingShootNum - this.shootCounter);
            meetingInfoSetActive(true);
            chargeInfoSetActive(false);
        }
        else
        {
            meetingInfoSetActive(false);
        }

        if (rolePlayer.CanMove)
        {
            if (this.timer > 0.0f &&
                this.curShootNum < this.maxShootNum)
            {
                this.timer -= Time.deltaTime;
            }

            if (this.timer <= 0.0f &&
                this.curKillCount >= this.chargeKillNum)
            {
                this.curShootNum = Math.Clamp(
                    this.curShootNum + 1, 0, this.maxShootNum);
                this.curKillCount = 0;
				this.timer = this.chargeTime;
            }
        }

        if (this.chargeInfoText == null || this.chargeTimerText == null)
        {
            createText();
        }
        updateText();
    }

    public override string GetColoredRoleName(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetColoredRoleName();
        }
        else
        {
            return Design.ColoedString(
                Palette.ImpostorRed, Tr.GetString(RoleTypes.Impostor.ToString()));
        }
    }
    public override string GetFullDescription()
    {
        if (IsAwake)
        {
            return Tr.GetString(
                $"{this.Id}FullDescription");
        }
        else
        {
            return Tr.GetString(
                $"{RoleTypes.Impostor}FullDescription");
        }
    }

    public override string GetImportantText(bool isContainFakeTask = true)
    {
        if (IsAwake)
        {
            return base.GetImportantText(isContainFakeTask);

        }
        else
        {
            return string.Concat(
			[
                TranslationController.Instance.GetString(
                    StringNames.ImpostorTask, Array.Empty<Il2CppSystem.Object>()),
                "\r\n",
                Palette.ImpostorRed.ToTextColor(),
                TranslationController.Instance.GetString(
                    StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()),
                "</color>"
            ]);
        }
    }

    public override string GetIntroDescription()
    {
        if (IsAwake)
        {
            return base.GetIntroDescription();
        }
        else
        {
            return Design.ColoedString(
                Palette.ImpostorRed,
                PlayerControl.LocalPlayer.Data.Role.Blurb);
        }
    }

    public override Color GetNameColor(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetNameColor(isTruthColor);
        }
        else
        {
            return Palette.ImpostorRed;
        }
    }

    public override bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        this.curKillCount = this.curKillCount + 1;
        return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateBoolOption(
            ShooterOption.IsInitAwake,
            false);
        factory.CreateIntOption(
            ShooterOption.AwakeKillNum,
            1, 0, 5, 1,
            format: OptionUnit.Shot);
        factory.CreateIntOption(
            ShooterOption.AwakeImpNum,
            1, 1, GameSystem.MaxImposterNum, 1);

        factory.CreateBoolOption(
            ShooterOption.NoneAwakeWhenShoot,
            true);
        factory.CreateFloatOption(
            ShooterOption.ShootKillCoolPenalty,
            5.0f, 0.0f, 30.0f, 0.5f,
            format: OptionUnit.Second);

        var meetingOps = factory.CreateBoolOption(
            ShooterOption.CanCallMeeting,
            true);

        factory.CreateBoolOption(
            ShooterOption.CanShootSelfCallMeeting,
            true, meetingOps,
            invert: true);

        var maxShootOps = factory.CreateIntOption(
           ShooterOption.MaxShootNum,
           1, 1, 14, 1,
           format: OptionUnit.Shot);

        var initShootOps = factory.CreateIntDynamicOption(
            ShooterOption.InitShootNum,
            0, 0, 1,
            format: OptionUnit.Shot,
            tempMaxValue: 14);

        var maxMeetingShootOps = factory.CreateIntDynamicOption(
            ShooterOption.MaxMeetingShootNum,
            1, 1, 1,
            format: OptionUnit.Shot,
            tempMaxValue: 14);

        factory.CreateFloatOption(
            ShooterOption.ShootChargeTime,
            90.0f, 30.0f, 120.0f, 5.0f,
            format: OptionUnit.Second);
        factory.CreateIntOption(
            ShooterOption.ShootKillNum,
            1, 0, 5, 1,
            format: OptionUnit.Shot);

        maxShootOps.AddWithUpdate(initShootOps);
        maxShootOps.AddWithUpdate(maxMeetingShootOps);

    }

    protected override void RoleSpecificInit()
    {
        var cate = this.Loader;

        this.isAwake = cate.GetValue<ShooterOption, bool>(
            ShooterOption.IsInitAwake);

        this.awakeKillCount = cate.GetValue<ShooterOption, int>(
            ShooterOption.AwakeKillNum);
        this.awakeImpNum = cate.GetValue<ShooterOption, int>(
            ShooterOption.AwakeImpNum);

        this.isNoneAwakeWhenShoot = cate.GetValue<ShooterOption, bool>(
            ShooterOption.NoneAwakeWhenShoot);

        this.awakedCallMeeting = cate.GetValue<ShooterOption, bool>(
            ShooterOption.CanCallMeeting);
        this.canShootSelfCallMeeting = cate.GetValue<ShooterOption, bool>(
            ShooterOption.CanShootSelfCallMeeting);

        this.maxShootNum = cate.GetValue<ShooterOption, int>(
            ShooterOption.MaxShootNum);
        this.curShootNum = cate.GetValue<ShooterOption, int>(
            ShooterOption.InitShootNum);
        this.maxMeetingShootNum = cate.GetValue<ShooterOption, int>(
            ShooterOption.MaxMeetingShootNum);
        this.chargeTime = cate.GetValue<ShooterOption, float>(
            ShooterOption.ShootChargeTime);
        this.chargeKillNum = cate.GetValue<ShooterOption, int>(
            ShooterOption.ShootKillNum);
        this.killCoolPenalty = cate.GetValue<ShooterOption, float>(
            ShooterOption.ShootKillCoolPenalty);

        this.isNoneAwakeWhenShoot =

        this.isAwake = this.isAwake ||
            (
                this.awakeKillCount <= 0 &&
                this.awakeImpNum >= GameOptionsManager.Instance.CurrentGameOptions.GetInt(
                    Int32OptionNames.NumImpostors)
            );

        this.isAwakedHasOtherVision = false;
        this.isAwakedHasOtherKillCool = true;
        this.isAwakedHasOtherKillRange = false;

        if (this.HasOtherVision)
        {
            this.HasOtherVision = false;
            this.isAwakedHasOtherVision = true;
        }

        if (this.HasOtherKillCool)
        {
            this.HasOtherKillCool = false;
        }

        if (this.HasOtherKillRange)
        {
            this.HasOtherKillRange = false;
            this.isAwakedHasOtherKillRange = true;
        }

        if (this.isAwake)
        {
            this.HasOtherVision = this.isAwakedHasOtherVision;
            this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
            this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
            this.CanCallMeeting = this.awakedCallMeeting;
        }

        this.timer = this.chargeTime;
        this.curKillCount = 0;
    }

    private void chargeInfoSetActive(bool active)
    {
        if (this.chargeTimerText != null)
        {
            this.chargeTimerText.gameObject.SetActive(active);
        }
        if (this.chargeInfoText != null)
        {
            this.chargeInfoText.gameObject.SetActive(active);
        }
    }

    private void meetingInfoSetActive(bool active)
    {
        if (meetingShootText != null)
        {
            meetingShootText.gameObject.SetActive(active);
        }
    }


    private void createText()
    {

        HudManager hudManager = HudManager.Instance;

        this.chargeTimerText = UnityEngine.Object.Instantiate(
            hudManager.KillButton.cooldownTimerText,
            hudManager.KillButton.transform.parent);

        this.chargeTimerText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        this.chargeTimerText.transform.localPosition =
            hudManager.UseButton.transform.localPosition + new Vector3(-2.0f, -0.125f, 0);
        this.chargeTimerText.gameObject.SetActive(true);

        this.chargeInfoText = UnityEngine.Object.Instantiate(
            hudManager.KillButton.cooldownTimerText,
            this.chargeTimerText.transform);
        this.chargeInfoText.enableWordWrapping = false;
        this.chargeInfoText.transform.localScale = Vector3.one * 0.5f;
        this.chargeInfoText.transform.localPosition += new Vector3(-0.05f, 0.6f, 0);
        this.chargeInfoText.gameObject.SetActive(true);
    }

    private void updateText()
    {
        if (this.chargeTimerText != null)
        {
            this.chargeTimerText.text = $"{Mathf.CeilToInt(this.timer)}";
        }

        if (this.chargeInfoText != null)
        {
            this.chargeInfoText.text = Tr.GetString(
				"shooterChargeInfo",
                this.curShootNum,
				this.maxShootNum);
        }
    }
}
