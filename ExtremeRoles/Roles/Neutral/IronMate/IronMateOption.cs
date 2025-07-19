using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Neutral.IronMate
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.IronMate.Option, int>(
                    ExtremeRoles.Roles.Solo.Neutral.IronMate.Option.BlockNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.IronMate.Option, float>(
                    ExtremeRoles.Roles.Solo.Neutral.IronMate.Option.SlowTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.IronMate.Option, float>(
                    ExtremeRoles.Roles.Solo.Neutral.IronMate.Option.SlowMod),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.IronMate.Option, float>(
                    ExtremeRoles.Roles.Solo.Neutral.IronMate.Option.PlayerShowTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.IronMate.Option, float>(
                    ExtremeRoles.Roles.Solo.Neutral.IronMate.Option.DeadBodyShowTimeOnAfterPlayer)
            );
        }
    }

    public class IronMateOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.IronMate.Option.BlockNum,
                1, 0, 10, 1);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Neutral.IronMate.Option.SlowTime,
                10.0f, 0.0f, 30.0f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Neutral.IronMate.Option.SlowMod,
                0.7f, 0.1f, 1.0f, 0.1f,
                format: OptionUnit.Multiplier);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Neutral.IronMate.Option.PlayerShowTime,
                10f, 0.0f, 30.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Neutral.IronMate.Option.DeadBodyShowTimeOnAfterPlayer,
                10f, 0.0f, 30.0f, 0.1f,
                format: OptionUnit.Second);
        }
    }
}
