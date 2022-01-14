using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Neet : SingleRoleBase
    {
        public enum NeetOption
        {
            CanCallMeeting,
            CanRepairSabotage,
            HasTask,
            IsNeutral
        }

        public Neet() : base(
            ExtremeRoleId.Neet,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Neet.ToString(),
            ColorPalette.NeetSilver,
            false, false, false,
            false, false, false,
            false, false, false)
        { }

        public override string GetFullDescription()
        {
            if (this.IsNeutral())
            {
                return Translation.GetString(
                    string.Format("{0}{1}", this.Id, "NeutralFullDescription"));
            }

            return base.GetFullDescription();
        }

        protected override void CreateSpecificOption(CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId((int)NeetOption.CanCallMeeting),
                Design.ConcatString(
                    this.RoleName,
                    NeetOption.CanCallMeeting.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleOptionId((int)NeetOption.CanRepairSabotage),
                Design.ConcatString(
                    this.RoleName,
                    NeetOption.CanRepairSabotage.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleOptionId((int)NeetOption.HasTask),
                Design.ConcatString(
                    this.RoleName,
                    NeetOption.HasTask.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleOptionId((int)NeetOption.IsNeutral),
                Design.ConcatString(
                    this.RoleName,
                    NeetOption.IsNeutral.ToString()),
                false, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionHolder.AllOption;

            this.CanCallMeeting = allOption[
                GetRoleOptionId((int)NeetOption.CanCallMeeting)].GetValue();
            this.CanRepairSabotage = allOption[
                GetRoleOptionId((int)NeetOption.CanRepairSabotage)].GetValue();
            this.HasTask = allOption[
                GetRoleOptionId((int)NeetOption.HasTask)].GetValue();

            if (allOption[
                GetRoleOptionId((int)NeetOption.IsNeutral)].GetValue())
            {
                this.Team = ExtremeRoleType.Neutral;
            }

        }
    }
}
