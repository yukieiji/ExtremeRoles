using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using static ExtremeRoles.Roles.Solo.Crewmate.Bait.BaitRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Bait
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
                loader.GetValue<Option, int>(
                    Option.AwakeTaskGage),
                loader.GetValue<Option, float>(
                    Option.DelayUntilForceReport),
                loader.GetValue<Option, bool>(
                    Option.EnableBaitBenefit),
                loader.GetValue<Option, float>(
                    Option.KillCoolReduceMulti),
                loader.GetValue<Option, float>(
                    Option.ReduceTimer)
            );
        }
    }

    public class BaitOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                Option.AwakeTaskGage,
                70, 0, 100, 10,
                format: Module.CustomOption.Enums.OptionUnit.Percentage);
            factory.CreateFloatOption(
                Option.DelayUntilForceReport,
                5.0f, 0.0f, 30.0f, 0.5f,
                format: Module.CustomOption.Enums.OptionUnit.Second);
            factory.CreateBoolOption(
                Option.EnableBaitBenefit,
                true);
            factory.CreateFloatOption(
                Option.KillCoolReduceMulti,
                2.0f, 1.1f, 5.0f, 0.1f,
                format: Module.CustomOption.Enums.OptionUnit.Multiplier);
            factory.CreateFloatOption(
                Option.ReduceTimer,
                5.0f, 1.0f, 30.0f, 0.5f,
                format: Module.CustomOption.Enums.OptionUnit.Second);
        }
    }
}
