using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Neet : SingleRoleBase
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
            CreateBoolOption(
                NeetOption.CanCallMeeting,
                false, parentOps);
            CreateBoolOption(
                NeetOption.CanRepairSabotage,
                false, parentOps);
            
            var neutralOps = CreateBoolOption(
                NeetOption.IsNeutral,
                false, parentOps);
            CreateBoolOption(
                NeetOption.HasTask,
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
