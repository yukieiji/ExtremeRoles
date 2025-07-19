using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Crewmate.Neet
{
    public readonly record struct NeetSpecificOption(
        bool CanCallMeeting,
        bool CanRepairSabotage,
        bool HasTask,
        bool IsNeutral
    ) : IRoleSpecificOption;

    public class NeetOptionLoader : ISpecificOptionLoader<NeetSpecificOption>
    {
        public NeetSpecificOption Load(IOptionLoader loader)
        {
            return new NeetSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption.CanCallMeeting),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption.CanRepairSabotage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption.HasTask),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetOption.IsNeutral)
            );
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
