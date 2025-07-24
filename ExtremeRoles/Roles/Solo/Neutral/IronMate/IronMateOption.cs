using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Neutral.IronMate.IronMateRole;

namespace ExtremeRoles.Roles.Solo.Neutral.IronMate
{
    public readonly record struct IronMateSpecificOption(
        int BlockNum,
        float SlowTime,
        float SlowMod,
        float PlayerShowTime,
        float DeadBodyShowTimeOnAfterPlayer
    ) : IRoleSpecificOption;

    public class IronMateOptionLoader : ISpecificOptionLoader<IronMateSpecificOption>
    {
        public IronMateSpecificOption Load(IOptionLoader loader)
        {
            return new IronMateSpecificOption(
                loader.GetValue<Option, int>(
                    Option.BlockNum),
                loader.GetValue<Option, float>(
                    Option.SlowTime),
                loader.GetValue<Option, float>(
                    Option.SlowMod),
                loader.GetValue<Option, float>(
                    Option.PlayerShowTime),
                loader.GetValue<Option, float>(
                    Option.DeadBodyShowTimeOnAfterPlayer)
            );
        }
    }

    public class IronMateOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                Option.BlockNum,
                1, 0, 10, 1);

            factory.CreateFloatOption(
                Option.SlowTime,
                10.0f, 0.0f, 30.0f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                Option.SlowMod,
                0.7f, 0.1f, 1.0f, 0.1f,
                format: OptionUnit.Multiplier);

            factory.CreateFloatOption(
                Option.PlayerShowTime,
                10f, 0.0f, 30.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                Option.DeadBodyShowTimeOnAfterPlayer,
                10f, 0.0f, 30.0f, 0.1f,
                format: OptionUnit.Second);
        }
    }
}
