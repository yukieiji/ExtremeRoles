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

        private TextMeshPro targetTimerText;

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
            ExtremeRoleId.PsychoKiller,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.PsychoKiller.ToString(),
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
            this.PlayerIcon[this.targetId].gameObject.SetActive(false);
        }

        public override bool TryRolePlayerKillTo(
            PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            if (targetPlayer.PlayerId == this.targetId)
            {
                this.KillCoolTime = this.targetKillCool;
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
                Design.ConcatString(
                    this.RoleName,
                    BountyHunterOption.TargetUpdateTime.ToString()),
                60f, 30.0f, 120f, 0.5f,
                parentOps, format: "unitSeconds");

            CustomOption.Create(
                GetRoleOptionId((int)BountyHunterOption.TargetKillCoolTime),
                Design.ConcatString(
                    this.RoleName,
                    BountyHunterOption.TargetKillCoolTime.ToString()),
                5f, 1.0f, 60f, 0.5f,
                parentOps, format: "unitSeconds");

            CustomOption.Create(
                GetRoleOptionId((int)BountyHunterOption.NoneTargetKillCoolTime),
                Design.ConcatString(
                    this.RoleName,
                    BountyHunterOption.NoneTargetKillCoolTime.ToString()),
                45f, 1.0f, 120f, 0.5f,
                parentOps, format: "unitSeconds");

            var arrowOption = CustomOption.Create(
                GetRoleOptionId((int)BountyHunterOption.IsShowArrow),
                Design.ConcatString(
                    this.RoleName,
                    BountyHunterOption.IsShowArrow.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)BountyHunterOption.ArrowUpdateCycle),
                Design.ConcatString(
                    this.RoleName,
                    BountyHunterOption.ArrowUpdateCycle.ToString()),
                60f, 30.0f, 120f, 0.5f,
                arrowOption);

        }

        protected override void RoleSpecificInit()
        {

            if (!this.HasOtherKillCool)
            {
                this.HasOtherKillCool = true;
                this.KillCoolTime = PlayerControl.GameOptions.KillCooldown;
            }

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
           
        }

        public void IntroBeginSetUp()
        {
            return;
        }

        public void IntroEndSetUp()
        {
            this.PlayerIcon = Player.CreatePlayerIcon();
            this.setNewTarget();
        }

        public void Update(PlayerControl rolePlayer)
        {
            this.targetTimer -= Time.deltaTime;
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
            }
        }

        private void setNewTarget()
        {
            this.targetTimer = this.changeTargetTime;
            this.PlayerIcon[this.targetId].gameObject.SetActive(false);

            List<PlayerControl> allPlayer = PlayerControl.AllPlayerControls.ToArray().ToList();

            allPlayer = allPlayer.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();

            Vector3 bottomLeft = HudManager.Instance.UseButton.transform.localPosition;
            bottomLeft.x *= -1;
            bottomLeft += new Vector3(-0.25f, -0.25f, 0);

            foreach (var player in allPlayer)
            {
                if (player.Data.IsDead || player.Data.Disconnected) { continue; }

                SingleRoleBase role = ExtremeRoleManager.GameRole[player.PlayerId];
                if (role.IsImposter() || role.FakeImposter) { continue; }

                this.targetId = player.PlayerId;
                this.PlayerIcon[this.targetId].gameObject.SetActive(true);
                this.PlayerIcon[this.targetId].transform.localScale = Vector3.one * 0.25f;
                this.PlayerIcon[this.targetId].transform.localPosition = bottomLeft + Vector3.right;
                
                break;
            }

        }
        private void updateArrow()
        {
            this.targetArrowUpdateTimer = this.targetArrowUpdateTime;
        }
    }
}
