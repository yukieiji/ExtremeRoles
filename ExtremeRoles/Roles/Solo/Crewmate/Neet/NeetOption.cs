using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using static ExtremeRoles.Roles.Solo.Crewmate.Neet.NeetRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Neet
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
                loader.GetValue<NeetOption, bool>(
                    NeetOption.CanCallMeeting),
                loader.GetValue<NeetOption, bool>(
                    NeetOption.CanRepairSabotage),
                loader.GetValue<NeetOption, bool>(
                    NeetOption.HasTask),
                loader.GetValue<NeetOption, bool>(
                    NeetOption.IsNeutral)
            );
        }
    }

    public class NeetOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                NeetOption.CanCallMeeting,
                false);
            factory.CreateBoolOption(
                NeetOption.CanRepairSabotage,
                false);

            var neutralOps = factory.CreateBoolOption(
                NeetOption.IsNeutral,
                false);
            factory.CreateBoolOption(
                NeetOption.HasTask,
                false, neutralOps,
                invert: true);
        }
    }
}
