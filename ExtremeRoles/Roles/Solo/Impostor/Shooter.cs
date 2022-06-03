using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public class Shooter : SingleRoleBase, IRoleResetMeeting, IRoleUpdate
    {
        public enum ShooterOption
        {
            CanCallMeeting,
            MaxShootNum,
            InitShootNum,
            MaxMeetingShootNum,
            ShootChargeTime,
            MaxChargeGiveNum,
            KillShootChargeTimeModd,
            ShootKillCoolPenalty
        }

        public int CurShootNum => this.curShootNum;
        public bool CanShoot => this.shootCounter < this.maxMeetingShootNum;

        private float defaultKillCool = 0.0f;
        private float killCoolPenalty = 0.0f;
        private float killShootChargeTimeModd = 0.0f;
        private float chargeTime = 0.0f;
        private float timer = float.MaxValue;
        private int maxShootNum = 0;
        private int curShootNum = 0;
        private int maxMeetingShootNum = 0;
        private int shootCounter = 0;

        private int chargeNum = 0;
        private int maxChargeNum = 0;

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

        public void Shoot()
        {
            this.curShootNum = this.curShootNum - 1;
            this.KillCoolTime = this.KillCoolTime + killCoolPenalty;
            this.shootCounter  = this.shootCounter + 1;
        }

        public void ResetOnMeetingEnd()
        {
            chargeInfoSetActive(true);
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
            if (rolePlayer.Data.IsDead || rolePlayer.Data.Disconnected)
            {
                this.curShootNum = 0;
                this.shootCounter = int.MaxValue;
                this.timer = this.chargeTime;
                return;
            }
            if (MeetingHud.Instance)
            {
                if (meetingShootText == null)
                {
                    meetingShootText = UnityEngine.Object.Instantiate(
                        HudManager.Instance.TaskText, MeetingHud.Instance.transform);
                    meetingShootText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                    meetingShootText.transform.position = Vector3.zero;
                    meetingShootText.transform.localPosition = new Vector3(-3.07f, 3.33f, -20f);
                    meetingShootText.transform.localScale *= 1.1f;
                    meetingShootText.color = Palette.White;
                    meetingShootText.gameObject.SetActive(false);
                }

                meetingShootText.text = string.Concat(
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
                    this.timer = this.chargeTime;
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
            CustomOptionBase parentOps)
        {
            CreateBoolOption(
                ShooterOption.CanCallMeeting,
                true, parentOps);

            var maxShootOps = CreateIntOption(
               ShooterOption.MaxShootNum,
               1, 1, 14, 1, parentOps,
               format: OptionUnit.Shot);

            var initShootOps = CreateIntDynamicOption(
                ShooterOption.InitShootNum,
                0, 0, 1, parentOps,
                format: OptionUnit.Shot);

            var maxMeetingShootOps = CreateIntDynamicOption(
                ShooterOption.MaxMeetingShootNum,
                1, 1, 1, parentOps,
                format: OptionUnit.Shot);

            CreateFloatOption(
                ShooterOption.ShootChargeTime,
                75.0f, 30.0f, 240.0f, 5.0f,
                parentOps, format: OptionUnit.Second);

            CreateIntOption(
               ShooterOption.MaxChargeGiveNum,
               14, 1, 14, 1, parentOps,
               format: OptionUnit.Shot);

            CreateFloatOption(
                ShooterOption.KillShootChargeTimeModd,
                7.5f, -30.0f, 30.0f, 0.5f,
                parentOps, format: OptionUnit.Second);

            CreateFloatOption(
                ShooterOption.ShootKillCoolPenalty,
                5.0f, 0.0f, 10.0f, 0.5f,
                parentOps, format: OptionUnit.Second);


            maxShootOps.SetUpdateOption(initShootOps);
            maxShootOps.SetUpdateOption(maxMeetingShootOps);

        }
        
        protected override void RoleSpecificInit()
        {
            var allOps = OptionHolder.AllOption;

            this.CanCallMeeting = allOps[
                GetRoleOptionId(ShooterOption.CanCallMeeting)].GetValue();

            this.maxShootNum = allOps[
                GetRoleOptionId(ShooterOption.MaxShootNum)].GetValue();
            this.curShootNum = allOps[
                GetRoleOptionId(ShooterOption.InitShootNum)].GetValue();
            this.maxMeetingShootNum = allOps[
                GetRoleOptionId(ShooterOption.MaxMeetingShootNum)].GetValue();
            this.chargeTime = allOps[
                GetRoleOptionId(ShooterOption.ShootChargeTime)].GetValue();
            this.maxChargeNum = allOps[
                GetRoleOptionId(ShooterOption.MaxChargeGiveNum)].GetValue();
            this.killShootChargeTimeModd = allOps[
                GetRoleOptionId(ShooterOption.KillShootChargeTimeModd)].GetValue();
            this.killCoolPenalty = allOps[
                GetRoleOptionId(ShooterOption.ShootKillCoolPenalty)].GetValue();

            if (!this.HasOtherKillCool)
            {
                this.HasOtherKillCool = true;
                this.KillCoolTime = PlayerControl.GameOptions.KillCooldown;
            }

            this.defaultKillCool = this.KillCoolTime;
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


        private void createText()
        {
            this.chargeTimerText = Object.Instantiate(
                HudManager.Instance.KillButton.cooldownTimerText,
                HudManager.Instance.KillButton.transform);
            this.chargeTimerText.transform.localPosition += new Vector3(-1.8f, -0.06f, 0);
            this.chargeTimerText.gameObject.SetActive(true);

            this.chargeInfoText = Object.Instantiate(
                HudManager.Instance.KillButton.cooldownTimerText,
                this.chargeTimerText.transform);
            this.chargeInfoText.enableWordWrapping = false;
            this.chargeInfoText.transform.localScale = Vector3.one * 0.5f;
            this.chargeInfoText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);
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
