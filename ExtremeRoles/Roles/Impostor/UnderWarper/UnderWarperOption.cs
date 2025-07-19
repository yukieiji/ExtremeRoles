using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.UnderWarper
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption.AwakeKillCount),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption.VentLinkKillCout),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption.NoVentAnimeKillCout),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption.WallHackVent),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption.Range)
            );
        }
    }

    public class UnderWarperOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption.AwakeKillCount,
                1, 0, 5, 1,
                format: OptionUnit.Shot);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption.VentLinkKillCout,
                2, 0, 5, 1,
                format: OptionUnit.Shot);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption.NoVentAnimeKillCout,
                2, 0, 5, 1,
                format: OptionUnit.Shot);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption.WallHackVent,
                false);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.UnderWarper.UnderWarperOption.Range,
                2.75f, 0.75f, 10.0f, 0.25f);
        }
    }
}
