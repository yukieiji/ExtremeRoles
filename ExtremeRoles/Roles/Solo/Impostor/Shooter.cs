using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class Shooter : SingleRoleBase, IRoleMeetingButtonAbility, IRoleReportHock, IRoleResetMeeting, IRoleUpdate
    {
        public enum ShooterOption
        {
            CanCallMeeting,
            CanShootSelfCallMeeting,
            MaxShootNum,
            InitShootNum,
            MaxMeetingShootNum,
            ShootChargeTime,
            CurShootNumChargePenalty,
            KillShootChargeTimeModd,
            MaxChargeGiveNum,
            ShootKillCoolPenalty,
            ShootShootChargePenalty,
        }

        private float defaultKillCool = 0.0f;
        private float killCoolPenalty = 0.0f;
        private float killShootChargeTimeModd = 0.0f;
        private float chargeTime = 0.0f;
        private float shootChargePenalty = 0.0f;
        private float curShootNumPenalty = 0.0f;
        private float timer = float.MaxValue;
        private int maxShootNum = 0;
        private int curShootNum = 0;
        private int maxMeetingShootNum = 0;
        private int shootCounter = 0;

        private int chargeNum = 0;
        private int maxChargeNum = 0;

        private bool canShootThisMeeting = false;
        private bool canShootSelfCallMeeting = false;

        private TMPro.TextMeshPro chargeInfoText = null;
        private TMPro.TextMeshPro chargeTimerText = null;
        private TMPro.TextMeshPro meetingShootText = null;

        public Shooter(): base(
            ExtremeRoleId.Shooter,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Shooter.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        {}

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

        public System.Action CreateAbilityAction(PlayerVoteArea instance)
        {

            byte target = instance.TargetPlayerId;

            void shooterKill()
            {
                if (instance.AmDead) { return; }
                Shoot();
                PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

                if (Crewmate.BodyGuard.TryGetShiledPlayerId(
                    target, out byte bodyGuard) ||
                    Crewmate.BodyGuard.RpcTryKillBodyGuard(
                        localPlayer.PlayerId, bodyGuard))
                {
                    rpcPlayKillSound();
                    return;
                }

                RPCOperator.Call(
                    localPlayer.NetId,
                    RPCOperator.Command.UncheckedMurderPlayer,
                    new List<byte> { localPlayer.PlayerId, target, 0 });
                RPCOperator.UncheckedMurderPlayer(
                   localPlayer.PlayerId,
                    target, 0);
                rpcPlayKillSound();
            }

            return shooterKill;
        }

        private static void rpcPlayKillSound()
        {
            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.PlaySound,
                new List<byte> { (byte)RPCOperator.SoundType.Kill });
            RPCOperator.PlaySound((byte)RPCOperator.SoundType.Kill);
        }

        public void SetSprite(SpriteRenderer render)
        {
            render.sprite = FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.sprite;
            render.transform.localScale *= new Vector2(0.75f, 0.75f);
        }

        public void HockReportButton(
            PlayerControl rolePlayer, GameData.PlayerInfo reporter)
        {
            this.canShootThisMeeting = true;
            if (rolePlayer.PlayerId == reporter.PlayerId)
            {
                this.canShootThisMeeting = this.canShootSelfCallMeeting;
            }
        }

        public void HockBodyReport(
            PlayerControl rolePlayer, GameData.PlayerInfo reporter, GameData.PlayerInfo reportBody)
        {
            this.canShootThisMeeting = true;
        }
        public void Shoot()
        {
            this.curShootNum = this.curShootNum - 1;
            this.KillCoolTime = this.KillCoolTime + killCoolPenalty;
            this.shootCounter  = this.shootCounter + 1;
            this.timer = this.timer + this.shootChargePenalty;
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
                    Helper.Translation.GetString("shooterShootStatus"),
                    this.curShootNum, this.maxShootNum,
                    this.maxMeetingShootNum - this.shootCounter);
                meetingShootText.gameObject.SetActive(true);
                chargeInfoSetActive(false);
            }
            else
            {
                if (meetingShootText != null)
                {
                    meetingShootText.gameObject.SetActive(false);
                }
            }

            if (rolePlayer.CanMove)
            {
                if (this.chargeNum < this.maxChargeNum &&
                    this.curShootNum < this.maxShootNum)
                {
                    this.timer -= Time.deltaTime;
                }

                if (this.timer < 0.0f)
                {
                    this.timer = this.chargeTime + (this.curShootNum * this.curShootNumPenalty);
                    this.curShootNum = System.Math.Clamp(
                        this.curShootNum + 1, 0, this.maxShootNum);
                    this.chargeNum = this.chargeNum + 1;
                }
            }

            if (this.chargeInfoText == null || this.chargeTimerText == null)
            {
                createText();
            }
            updateText();
        }


        public override bool TryRolePlayerKillTo(
            PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            this.KillCoolTime = this.defaultKillCool;
            this.timer = this.timer + this.killShootChargeTimeModd;
            return true;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
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
                75.0f, 30.0f, 240.0f, 5.0f,
                parentOps, format: OptionUnit.Second);

            CreateFloatOption(
               ShooterOption.CurShootNumChargePenalty,
               0.0f, 0.0f, 20.0f, 0.5f,
               parentOps, format: OptionUnit.Second);

            CreateFloatOption(
                ShooterOption.KillShootChargeTimeModd,
                7.5f, -30.0f, 30.0f, 0.5f,
                parentOps, format: OptionUnit.Second);

            CreateIntOption(
               ShooterOption.MaxChargeGiveNum,
               14, 1, 14, 1, parentOps,
               format: OptionUnit.Shot);

            CreateFloatOption(
                ShooterOption.ShootKillCoolPenalty,
                5.0f, 0.0f, 10.0f, 0.5f,
                parentOps, format: OptionUnit.Second);

            CreateFloatOption(
               ShooterOption.ShootShootChargePenalty,
               0.0f, 0.0f, 20.0f, 0.5f,
               parentOps, format: OptionUnit.Second);


            maxShootOps.SetUpdateOption(initShootOps);
            maxShootOps.SetUpdateOption(maxMeetingShootOps);

        }
        
        protected override void RoleSpecificInit()
        {
            var allOps = OptionHolder.AllOption;

            this.CanCallMeeting = allOps[
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
            this.curShootNumPenalty = allOps[
                GetRoleOptionId(ShooterOption.CurShootNumChargePenalty)].GetValue();
            this.maxChargeNum = allOps[
                GetRoleOptionId(ShooterOption.MaxChargeGiveNum)].GetValue();
            this.killShootChargeTimeModd = allOps[
                GetRoleOptionId(ShooterOption.KillShootChargeTimeModd)].GetValue();
            this.killCoolPenalty = allOps[
                GetRoleOptionId(ShooterOption.ShootKillCoolPenalty)].GetValue();
            this.shootChargePenalty = allOps[
                GetRoleOptionId(ShooterOption.ShootShootChargePenalty)].GetValue();

            if (!this.HasOtherKillCool)
            {
                this.HasOtherKillCool = true;
                this.KillCoolTime = PlayerControl.GameOptions.KillCooldown;
            }

            this.defaultKillCool = this.KillCoolTime;
            this.timer = this.chargeTime + (this.curShootNum * this.curShootNumPenalty);
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


        private void createText()
        {

            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;

            this.chargeTimerText = Object.Instantiate(
                hudManager.KillButton.cooldownTimerText,
                hudManager.KillButton.transform.parent);

            this.chargeTimerText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            this.chargeTimerText.transform.localPosition = 
                hudManager.UseButton.transform.localPosition + new Vector3(-2.0f, -0.125f, 0);
            this.chargeTimerText.gameObject.SetActive(true);

            this.chargeInfoText = Object.Instantiate(
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
