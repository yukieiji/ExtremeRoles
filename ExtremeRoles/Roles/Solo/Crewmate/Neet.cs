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
                    $"{this.Id}NeutralFullDescription");
            }

            return base.GetFullDescription();
        }

        protected override void CreateSpecificOption(CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId(NeetOption.CanCallMeeting),
                string.Concat(
                    this.RoleName,
                    NeetOption.CanCallMeeting.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleOptionId(NeetOption.CanRepairSabotage),
                string.Concat(
                    this.RoleName,
                    NeetOption.CanRepairSabotage.ToString()),
                false, parentOps);
            
            var neutralOps = CustomOption.Create(
                GetRoleOptionId(NeetOption.IsNeutral),
                string.Concat(
                    this.RoleName,
                    NeetOption.IsNeutral.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleOptionId(NeetOption.HasTask),
                string.Concat(
                    this.RoleName,
                    NeetOption.HasTask.ToString()),
                false, neutralOps,
                invert: true,
                enableCheckOption: parentOps);
        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionHolder.AllOption;

            this.CanCallMeeting = allOption[
                GetRoleOptionId(NeetOption.CanCallMeeting)].GetValue();
            this.CanRepairSabotage = allOption[
                GetRoleOptionId(NeetOption.CanRepairSabotage)].GetValue();
            this.HasTask = allOption[
                GetRoleOptionId(NeetOption.HasTask)].GetValue();

            if (allOption[
                GetRoleOptionId(NeetOption.IsNeutral)].GetValue())
            {
                this.Team = ExtremeRoleType.Neutral;
            }

        }
    }
}
