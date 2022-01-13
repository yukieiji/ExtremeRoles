using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public class PsychoKiller : SingleRoleBase, IRoleResetMeeting
    {

        private bool isResetMeeting;
        private float reduceRate;
        private float defaultKillCoolTime;
        private int combMax;

        private int combCount;

        public enum PsychoKillerOption
        {
            KillCoolReduceRate,
            CombMax,
            CombResetWhenMeeting
        }

        public PsychoKiller() : base(
            ExtremeRoleId.PsychoKiller,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.PsychoKiller.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        {}

        public void ResetOnMeetingEnd()
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
                this.combCount);
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
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId((int)PsychoKillerOption.KillCoolReduceRate),
                Design.ConcatString(
                    this.RoleName,
                    PsychoKillerOption.KillCoolReduceRate.ToString()),
                5, 1, 10, 1, parentOps,
                format: "unitPercentage");

            CustomOption.Create(
                GetRoleOptionId((int)PsychoKillerOption.CombMax),
                Design.ConcatString(
                    this.RoleName,
                    PsychoKillerOption.CombMax.ToString()),
                2, 1, 5, 1,
                parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)PsychoKillerOption.CombResetWhenMeeting),
                Design.ConcatString(
                    this.RoleName,
                    PsychoKillerOption.CombResetWhenMeeting.ToString()),
                true, parentOps);
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
                GetRoleOptionId((int)PsychoKillerOption.KillCoolReduceRate)].GetValue();
            this.isResetMeeting = allOption[
                GetRoleOptionId((int)PsychoKillerOption.CombResetWhenMeeting)].GetValue();
            this.combMax= allOption[
                GetRoleOptionId((int)PsychoKillerOption.CombMax)].GetValue();

            this.combCount = 1;
            this.defaultKillCoolTime = this.KillCoolTime;
        }
    }
}
