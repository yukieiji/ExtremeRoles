using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperRole;

namespace ExtremeRoles.Roles.Solo.Impostor.UnderWarper
{
    public readonly record struct UnderWarperSpecificOption(
        int AwakeKillCount,
        int VentLinkKillCout,
        int NoVentAnimeKillCout,
        bool WallHackVent,
        float Range
    ) : IRoleSpecificOption;

    public class UnderWarperOptionLoader : ISpecificOptionLoader<UnderWarperSpecificOption>
    {
        public UnderWarperSpecificOption Load(IOptionLoader loader)
        {
            return new UnderWarperSpecificOption(
                loader.GetValue<UnderWarperOption, int>(
                    UnderWarperOption.AwakeKillCount),
                loader.GetValue<UnderWarperOption, int>(
                    UnderWarperOption.VentLinkKillCout),
                loader.GetValue<UnderWarperOption, int>(
                    UnderWarperOption.NoVentAnimeKillCout),
                loader.GetValue<UnderWarperOption, bool>(
                    UnderWarperOption.WallHackVent),
                loader.GetValue<UnderWarperOption, float>(
                    UnderWarperOption.Range)
            );
        }
    }

    public class UnderWarperOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                UnderWarperOption.AwakeKillCount,
                1, 0, 5, 1,
                format: OptionUnit.Shot);
            factory.CreateIntOption(
                UnderWarperOption.VentLinkKillCout,
                2, 0, 5, 1,
                format: OptionUnit.Shot);
            factory.CreateIntOption(
                UnderWarperOption.NoVentAnimeKillCout,
                2, 0, 5, 1,
                format: OptionUnit.Shot);
            factory.CreateBoolOption(
                UnderWarperOption.WallHackVent,
                false);
            factory.CreateFloatOption(
                UnderWarperOption.Range,
                2.75f, 0.75f, 10.0f, 0.25f);
        }
    }
}
