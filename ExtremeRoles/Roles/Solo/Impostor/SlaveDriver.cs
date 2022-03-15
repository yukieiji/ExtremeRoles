using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public class SlaveDriver : SingleRoleBase, IRoleUpdate, IRoleResetMeeting, IRoleMurderPlayerHock
    {
        private bool isResetMeeting;
        private float reduceRate;
        private float defaultKillCoolTime;

        private List<byte> specialAttackedPlayerIdList;

        private byte rolePlayerId;
        private byte specialAttackPlayerId;

        private float timeLimit;
        private float timer;

        public enum SlaveDriverOption
        {
            TaskProgressRange,
            KillCoolReduceRate,
            SpecialAttackTimer,
            SpecialAttackKillCoolReduceRate,
        }

        public SlaveDriver() : base(
            ExtremeRoleId.SlaveDriver,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.SlaveDriver.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }


        public override bool TryRolePlayerKillTo(
            PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            
            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            CustomOption.Create(
                GetRoleOptionId((int)SlaveDriverOption.TaskProgressRange),
                string.Concat(
                    this.RoleName,
                    SlaveDriverOption.TaskProgressRange.ToString()),
                10, 5, 20, 1, parentOps,
                format: "unitPercentage");

            CustomOption.Create(
                GetRoleOptionId((int)SlaveDriverOption.KillCoolReduceRate),
                string.Concat(
                    this.RoleName,
                    SlaveDriverOption.KillCoolReduceRate.ToString()),
                25, 1, 50, 1, parentOps,
                format: "unitPercentage");

            CustomOption.Create(
                GetRoleOptionId((int)SlaveDriverOption.SpecialAttackTimer),
                string.Concat(
                    this.RoleName,
                    SlaveDriverOption.SpecialAttackTimer.ToString()),
                60f, 30f, 120f, 0.5f, parentOps,
                format: "unitSeconds");

            CustomOption.Create(
                GetRoleOptionId((int)SlaveDriverOption.SpecialAttackKillCoolReduceRate),
                string.Concat(
                    this.RoleName,
                    SlaveDriverOption.SpecialAttackKillCoolReduceRate.ToString()),
                50, 25, 75, 1, parentOps,
                format: "unitPercentage");
        }

        protected override void RoleSpecificInit()
        {

            if (!this.HasOtherKillCool)
            {
                this.HasOtherKillCool = true;
                this.KillCoolTime = PlayerControl.GameOptions.KillCooldown;
            }

            var allOption = OptionHolder.AllOption;

            this.reduceRate = allOption[
                GetRoleOptionId((int)SlaveDriverOption.KillCoolReduceRate)].GetValue();

            this.defaultKillCoolTime = this.KillCoolTime;
        }

        public void HockMuderPlayer(PlayerControl source, PlayerControl target)
        {
            if (source.PlayerId == this.rolePlayerId &&
                target.PlayerId == this.specialAttackPlayerId)
            {

            }
        }

        public void ResetOnMeetingEnd()
        {
            return;
        }

        public void ResetOnMeetingStart()
        {
            this.KillCoolTime = this.defaultKillCoolTime;
        }

        public void Update(PlayerControl rolePlayer)
        {
            throw new System.NotImplementedException();
        }
    }
}
