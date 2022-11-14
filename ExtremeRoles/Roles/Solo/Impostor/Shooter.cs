using System;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

using TMPro;

namespace ExtremeRoles.Roles.Solo.Impostor
{
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

        private int chargeNum = 0;
        private int maxChargeNum = 0;

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
                !(this.shootCounter < this.maxMeetingShootNum && this.canShootThisMeeting) || 
                target == 253 ||
                ExtremeRoleManager.GameRole[target].Id == ExtremeRoleId.Assassin;
        }

        public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
        {
            abilityButton.name = $"shooterKill_{instance.TargetPlayerId}";
            var controllerHighlight = abilityButton.transform.FindChild("ControllerHighlight");
            if (controllerHighlight != null)
            {
                controllerHighlight.localScale *= new Vector2(1.25f, 1.25f);
            }
        }

        public Action CreateAbilityAction(PlayerVoteArea instance)
        {

            byte target = instance.TargetPlayerId;

            void shooterKill()
            {
                if (instance.AmDead) { return; }
                Shoot();
                PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

                if (Crewmate.BodyGuard.TryGetShiledPlayerId(
                    target, out byte bodyGuard) &&
                    Crewmate.BodyGuard.RpcTryKillBodyGuard(
                        localPlayer.PlayerId, bodyGuard))
                {
                    rpcPlayKillSound();
                    return;
                }

                Helper.Player.RpcUncheckMurderPlayer(
                    localPlayer.PlayerId,
                    target, byte.MinValue);

                rpcPlayKillSound();
            }

            return shooterKill;
        }

        private static void rpcPlayKillSound()
        {
            Sound.RpcPlaySound(Sound.SoundType.Kill);
        }

        public void SetSprite(SpriteRenderer render)
        {
            render.sprite = FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.sprite;
            render.transform.localScale *= new Vector2(0.75f, 0.75f);
        }

        public void HookReportButton(
            PlayerControl rolePlayer, GameData.PlayerInfo reporter)
        {
            this.canShootThisMeeting = true;
            if (rolePlayer.PlayerId == reporter.PlayerId)
            {
                this.canShootThisMeeting = this.canShootSelfCallMeeting;
            }
        }

        public void HookBodyReport(
            PlayerControl rolePlayer, GameData.PlayerInfo reporter, GameData.PlayerInfo reportBody)
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
                this.HasOtherVison = false;
                this.HasOtherKillCool = false;
                this.HasOtherKillRange = false;
                this.CanCallMeeting = true;
            }
        }

        public void ResetOnMeetingEnd()
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
            if (CachedShipStatus.Instance == null ||
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
                    this.HasOtherVison = this.isAwakedHasOtherVision;
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
                        FastDestroyableSingleton<HudManager>.Instance.TaskText,
                        MeetingHud.Instance.transform);
                    meetingShootText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                    meetingShootText.transform.position = Vector3.zero;
                    meetingShootText.transform.localPosition = new Vector3(-2.85f, 3.15f, -20f);
                    meetingShootText.transform.localScale *= 0.9f;
                    meetingShootText.color = Palette.White;
                    meetingShootText.gameObject.SetActive(false);
                }

                meetingShootText.text = string.Format(
                    Translation.GetString("shooterShootStatus"),
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
                    this.chargeNum < this.maxChargeNum &&
                    this.curShootNum < this.maxShootNum)
                {
                    this.timer -= Time.deltaTime;
                }

                if (this.timer <= 0.0f && 
                    this.curKillCount >= this.chargeKillNum)
                {
                    this.curShootNum = System.Math.Clamp(
                        this.curShootNum + 1, 0, this.maxShootNum);
                    this.chargeNum = this.chargeNum + 1;
                    this.curKillCount = 0;
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
                    Palette.ImpostorRed, Translation.GetString(RoleTypes.Impostor.ToString()));
            }
        }
        public override string GetFullDescription()
        {
            if (IsAwake)
            {
                return Translation.GetString(
                    $"{this.Id}FullDescription");
            }
            else
            {
                return Translation.GetString(
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
                return string.Concat(new string[]
                {
                    FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.ImpostorTask, Array.Empty<Il2CppSystem.Object>()),
                    "\r\n",
                    Palette.ImpostorRed.ToTextColor(),
                    FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()),
                    "</color>"
                });
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
                    CachedPlayerControl.LocalPlayer.Data.Role.Blurb);
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
            IOption parentOps)
        {
            CreateBoolOption(
                ShooterOption.IsInitAwake,
                false, parentOps);
            CreateIntOption(
                ShooterOption.AwakeKillNum,
                2, 0, 5, 1,
                parentOps,
                format: OptionUnit.Shot);
            CreateIntOption(
                ShooterOption.AwakeImpNum,
                1, 1, OptionHolder.MaxImposterNum, 1,
                parentOps);

            CreateBoolOption(
                ShooterOption.NoneAwakeWhenShoot,
                true, parentOps);
            CreateFloatOption(
                ShooterOption.ShootKillCoolPenalty,
                5.0f, 0.0f, 10.0f, 0.5f,
                parentOps, format: OptionUnit.Second);

            var meetingOps = CreateBoolOption(
                ShooterOption.CanCallMeeting,
                true, parentOps);

            CreateBoolOption(
                ShooterOption.CanShootSelfCallMeeting,
                true, meetingOps,
                invert: true,
                enableCheckOption: parentOps);

            var maxShootOps = CreateIntOption(
               ShooterOption.MaxShootNum,
               1, 1, 14, 1, parentOps,
               format: OptionUnit.Shot);

            var initShootOps = CreateIntDynamicOption(
                ShooterOption.InitShootNum,
                0, 0, 1, parentOps,
                format: OptionUnit.Shot,
                tempMaxValue: 14);

            var maxMeetingShootOps = CreateIntDynamicOption(
                ShooterOption.MaxMeetingShootNum,
                1, 1, 1, parentOps,
                format: OptionUnit.Shot,
                tempMaxValue: 14);

            CreateFloatOption(
                ShooterOption.ShootChargeTime,
                75.0f, 30.0f, 120.0f, 5.0f,
                parentOps, format: OptionUnit.Second);
            CreateIntOption(
                ShooterOption.ShootKillNum,
                2, 0, 5, 1,
                parentOps,
                format: OptionUnit.Shot);

            maxShootOps.SetUpdateOption(initShootOps);
            maxShootOps.SetUpdateOption(maxMeetingShootOps);

        }
        
        protected override void RoleSpecificInit()
        {
            var allOps = OptionHolder.AllOption;

            this.isAwake = allOps[
                GetRoleOptionId(ShooterOption.IsInitAwake)].GetValue();

            this.awakeKillCount = allOps[
                GetRoleOptionId(ShooterOption.AwakeKillNum)].GetValue();
            this.awakeImpNum = allOps[
                GetRoleOptionId(ShooterOption.AwakeImpNum)].GetValue();

            this.isNoneAwakeWhenShoot = allOps[
                GetRoleOptionId(ShooterOption.NoneAwakeWhenShoot)].GetValue();

            this.awakedCallMeeting = allOps[
                GetRoleOptionId(ShooterOption.CanCallMeeting)].GetValue();
            this.canShootSelfCallMeeting = allOps[
                GetRoleOptionId(ShooterOption.CanShootSelfCallMeeting)].GetValue();

            this.maxShootNum = allOps[
                GetRoleOptionId(ShooterOption.MaxShootNum)].GetValue();
            this.curShootNum = allOps[
                GetRoleOptionId(ShooterOption.InitShootNum)].GetValue();
            this.maxMeetingShootNum = allOps[
                GetRoleOptionId(ShooterOption.MaxMeetingShootNum)].GetValue();
            this.chargeTime = allOps[
                GetRoleOptionId(ShooterOption.ShootChargeTime)].GetValue();
            this.chargeKillNum = allOps[
                GetRoleOptionId(ShooterOption.ShootKillNum)].GetValue();
            this.killCoolPenalty = allOps[
                GetRoleOptionId(ShooterOption.ShootKillCoolPenalty)].GetValue();

            this.isNoneAwakeWhenShoot = 

            this.isAwake = this.isAwake ||
                (
                    this.awakeKillCount <= 0 &&
                    this.awakeImpNum >= PlayerControl.GameOptions.NumImpostors
                );

            this.isAwakedHasOtherVision = false;
            this.isAwakedHasOtherKillCool = true;
            this.isAwakedHasOtherKillRange = false;

            if (this.HasOtherVison)
            {
                this.HasOtherVison = false;
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
                this.HasOtherVison = this.isAwakedHasOtherVision;
                this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
                this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
                this.CanCallMeeting = this.awakedCallMeeting;
            }

            this.timer = this.chargeTime;
            this.chargeNum = 0;
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

            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;

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
                this.chargeInfoText.text = string.Format(
                    Helper.Translation.GetString("shooterChargeInfo"),
                    this.curShootNum, this.maxShootNum);
            }
        }
    }
}
