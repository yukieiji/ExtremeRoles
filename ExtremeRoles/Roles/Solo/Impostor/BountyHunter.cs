using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public class BountyHunter : SingleRoleBase, IRoleUpdate, IRoleSpecialSetUp, IRoleResetMeeting
    {

        private byte targetId;

        private float targetTimer;
        private float changeTargetTime;

        private bool isShowArrow;
        private float targetArrowUpdateTimer;
        private float targetArrowUpdateTime = 0.0f;

        private float defaultKillCool;
        private float targetKillCool;
        private float noneTargetKillCool;

        private TextMeshPro targetTimerText = null;
        private Arrow targetArrow = null;

        private Dictionary<byte, PoolablePlayer> PlayerIcon = new Dictionary<byte, PoolablePlayer>();

        public enum BountyHunterOption
        {
            TargetUpdateTime,
            TargetKillCoolTime,
            NoneTargetKillCoolTime,
            IsShowArrow,
            ArrowUpdateCycle
        }

        public BountyHunter() : base(
            ExtremeRoleId.BountyHunter,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.BountyHunter.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public void ResetOnMeetingEnd()
        {
            this.KillCoolTime = this.defaultKillCool;
            this.setNewTarget();
            this.updateArrow();
        }

        public void ResetOnMeetingStart()
        {
            if (this.PlayerIcon.ContainsKey(this.targetId))
            {
                this.PlayerIcon[this.targetId].gameObject.SetActive(false);
            }
        }

        public override string GetFullDescription()
        {
            return string.Format(
                base.GetFullDescription(),
                this.targetKillCool,
                this.noneTargetKillCool,
                Player.GetPlayerControlById(this.targetId).Data.PlayerName);
        }

        public override bool TryRolePlayerKillTo(
            PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            if (targetPlayer.PlayerId == this.targetId)
            {
                this.KillCoolTime = this.targetKillCool;
                this.setNewTarget();
            }
            else
            {
                this.KillCoolTime = this.noneTargetKillCool;
            }

            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            CustomOption.Create(
                GetRoleOptionId((int)BountyHunterOption.TargetUpdateTime),
                string.Concat(
                    this.RoleName,
                    BountyHunterOption.TargetUpdateTime.ToString()),
                60f, 30.0f, 120f, 0.5f,
                parentOps, format: OptionUnit.Second);

            CustomOption.Create(
                GetRoleOptionId((int)BountyHunterOption.TargetKillCoolTime),
                string.Concat(
                    this.RoleName,
                    BountyHunterOption.TargetKillCoolTime.ToString()),
                5f, 1.0f, 60f, 0.5f,
                parentOps, format: OptionUnit.Second);

            CustomOption.Create(
                GetRoleOptionId((int)BountyHunterOption.NoneTargetKillCoolTime),
                string.Concat(
                    this.RoleName,
                    BountyHunterOption.NoneTargetKillCoolTime.ToString()),
                45f, 1.0f, 120f, 0.5f,
                parentOps, format: OptionUnit.Second);

            var arrowOption = CustomOption.Create(
                GetRoleOptionId((int)BountyHunterOption.IsShowArrow),
                string.Concat(
                    this.RoleName,
                    BountyHunterOption.IsShowArrow.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)BountyHunterOption.ArrowUpdateCycle),
                string.Concat(
                    this.RoleName,
                    BountyHunterOption.ArrowUpdateCycle.ToString()),
                60f, 1.0f, 120f, 0.5f,
                arrowOption, format: OptionUnit.Second);

        }

        protected override void RoleSpecificInit()
        {

            if (!this.HasOtherKillCool)
            {
                this.HasOtherKillCool = true;
                this.KillCoolTime = PlayerControl.GameOptions.KillCooldown;
            }

            this.defaultKillCool = this.KillCoolTime;

            var allOption = OptionHolder.AllOption;

            this.changeTargetTime = allOption[
                GetRoleOptionId((int)BountyHunterOption.TargetUpdateTime)].GetValue();
            this.targetKillCool = allOption[
                GetRoleOptionId((int)BountyHunterOption.TargetKillCoolTime)].GetValue();
            this.noneTargetKillCool = allOption[
                GetRoleOptionId((int)BountyHunterOption.NoneTargetKillCoolTime)].GetValue();
            this.isShowArrow = allOption[
                GetRoleOptionId((int)BountyHunterOption.IsShowArrow)].GetValue();
            if (this.isShowArrow)
            {
                this.targetArrowUpdateTime = allOption[
                    GetRoleOptionId((int)BountyHunterOption.ArrowUpdateCycle)].GetValue();
            }
            this.targetArrowUpdateTimer = 0;
            this.targetTimer = 0;
            this.targetId = byte.MaxValue;
        }

        public void IntroBeginSetUp()
        {
            return;
        }

        public void IntroEndSetUp()
        {
            this.PlayerIcon = Player.CreatePlayerIcon();
        }

        public void Update(PlayerControl rolePlayer)
        {
            this.targetTimer -= Time.deltaTime;

            if (this.targetTimerText == null)
            {
                this.targetTimerText = UnityEngine.Object.Instantiate(
                    HudManager.Instance.KillButton.cooldownTimerText);
                this.targetTimerText.alignment = TMPro.TextAlignmentOptions.Center;
            }

            if (this.PlayerIcon.Count == 0) { return; }

            this.targetTimerText.text = Mathf.CeilToInt(
                Mathf.Clamp(this.targetTimer, 0, this.changeTargetTime)).ToString();

            if (this.targetTimer <= 0)
            {
                this.setNewTarget();
            }
            if (this.isShowArrow)
            {
                this.targetArrowUpdateTimer -= Time.deltaTime;
                if (this.targetArrowUpdateTimer <= 0)
                {
                    this.updateArrow();
                }
                this.targetArrow.Update();
            }
        }

        private void setNewTarget()
        {
            this.targetTimer = this.changeTargetTime;
            if (this.targetId != byte.MaxValue)
            {
                this.PlayerIcon[this.targetId].gameObject.SetActive(false);
            }

            List<PlayerControl> allPlayer = PlayerControl.AllPlayerControls.ToArray().ToList();

            allPlayer = allPlayer.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();

            Vector3 bottomLeft = HudManager.Instance.UseButton.transform.localPosition;
            bottomLeft.x *= -1;
            bottomLeft += new Vector3(-0.375f, -0.25f, 0);

            foreach (var player in allPlayer)
            {
                if (player.Data.IsDead || player.Data.Disconnected) { continue; }
                
                SingleRoleBase role = ExtremeRoleManager.GameRole[player.PlayerId];
                
                if (role.IsImpostor() || role.FakeImposter || this.targetId == player.PlayerId) { continue; }

                this.targetId = player.PlayerId;
                this.PlayerIcon[this.targetId].gameObject.SetActive(true);
                this.PlayerIcon[this.targetId].transform.localScale = Vector3.one * 0.4f;
                this.PlayerIcon[this.targetId].transform.localPosition = bottomLeft;

                this.targetTimerText.transform.parent = this.PlayerIcon[this.targetId].transform;
                this.targetTimerText.transform.localPosition = new Vector3(0.0f, 0.0f, -100.0f);
                this.targetTimerText.transform.localScale = new Vector3(1.5f, 1.5f, 1.0f);
                this.targetTimerText.gameObject.SetActive(true);

                break;
            }

        }
        private void updateArrow()
        {

            if (this.targetArrow == null)
            {
                this.targetArrow = new Arrow(
                    Palette.ImpostorRed);
            }

            this.targetArrowUpdateTimer = this.targetArrowUpdateTime;
            this.targetArrow.UpdateTarget(
                Player.GetPlayerControlById(this.targetId).transform.position);
        }
    }
}
