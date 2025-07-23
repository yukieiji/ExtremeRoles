using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Bomber
{
    public readonly record struct BomberSpecificOption(
        int ExplosionRange,
        int ExplosionKillChance,
        float TimerMaxTime,
        float TimerMinTime,
        bool TellExplosion,
        int AbilityUseCount,
        float AbilityActiveTime
    ) : IRoleSpecificOption;

    public class BomberOptionLoader : ISpecificOptionLoader<BomberSpecificOption>
    {
        public BomberSpecificOption Load(IOptionLoader loader)
        {
            return new BomberSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.ExplosionRange),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.ExplosionKillChance),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.TimerMaxTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.TimerMinTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.TellExplosion),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            );
        }
    }

    public class BomberOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 5, 2.5f);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.ExplosionRange,
                2, 1, 5, 1);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.ExplosionKillChance,
                50, 25, 75, 1,
                format: OptionUnit.Percentage);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.TimerMinTime,
                15f, 5.0f, 30f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.TimerMaxTime,
                60f, 45f, 75f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.TellExplosion,
                true);
        }
    }
}
