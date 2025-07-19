using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Crewmate.Bait
{
    public readonly record struct BaitSpecificOption(
        int AwakeTaskGage,
        float DelayUntilForceReport,
        bool EnableBaitBenefit,
        float KillCoolReduceMulti,
        float ReduceTimer
    ) : IRoleSpecificOption;

    public class BaitOptionLoader : ISpecificOptionLoader<BaitSpecificOption>
    {
        public BaitSpecificOption Load(IOptionLoader loader)
        {
            return new BaitSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Bait.Option, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Bait.Option, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.DelayUntilForceReport),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Bait.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.EnableBaitBenefit),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Bait.Option, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.KillCoolReduceMulti),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Bait.Option, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.ReduceTimer)
            );
        }
    }

    public class BaitOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.AwakeTaskGage,
                70, 0, 100, 10,
                format: Module.CustomOption.Enums.OptionUnit.Percentage);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.DelayUntilForceReport,
                5.0f, 0.0f, 30.0f, 0.5f,
                format: Module.CustomOption.Enums.OptionUnit.Second);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.EnableBaitBenefit,
                true);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.KillCoolReduceMulti,
                2.0f, 1.1f, 5.0f, 0.1f,
                format: Module.CustomOption.Enums.OptionUnit.Multiplier);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Bait.Option.ReduceTimer,
                5.0f, 1.0f, 30.0f, 0.5f,
                format: Module.CustomOption.Enums.OptionUnit.Second);
        }
    }
}
