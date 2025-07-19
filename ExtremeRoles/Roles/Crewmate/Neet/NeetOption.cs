using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Crewmate.Neet
{
    public class NeetSpecificOption : IRoleSpecificOption
    {
        public bool CanCallMeeting { get; set; }
        public bool CanRepairSabotage { get; set; }
        public bool HasTask { get; set; }
        public bool IsNeutral { get; set; }
    }

    public class NeetOptionLoader : ISpecificOptionLoader<NeetSpecificOption>
    {
        public NeetSpecificOption Load(IOptionLoader loader)
        {
            return new NeetSpecificOption
            {
                CanCallMeeting = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption.CanCallMeeting),
                CanRepairSabotage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption.CanRepairSabotage),
                HasTask = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption.HasTask),
                IsNeutral = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption.IsNeutral)
            };
        }
    }

    public class NeetOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption.CanCallMeeting,
                false);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption.CanRepairSabotage,
                false);

            var neutralOps = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption.IsNeutral,
                false);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption.HasTask,
                false, neutralOps,
                invert: true);
        }
    }
}
