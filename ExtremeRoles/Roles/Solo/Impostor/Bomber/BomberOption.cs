using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Bomber
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
                loader.GetValue<BomberOption, int>(
                    BomberOption.ExplosionRange),
                loader.GetValue<BomberOption, int>(
                    BomberOption.ExplosionKillChance),
                loader.GetValue<BomberOption, float>(
                    BomberOption.TimerMaxTime),
                loader.GetValue<BomberOption, float>(
                    BomberOption.TimerMinTime),
                loader.GetValue<BomberOption, bool>(
                    BomberOption.TellExplosion),
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
                BomberOption.ExplosionRange,
                2, 1, 5, 1);
            factory.CreateIntOption(
                BomberOption.ExplosionKillChance,
                50, 25, 75, 1,
                format: OptionUnit.Percentage);
            factory.CreateFloatOption(
                BomberOption.TimerMinTime,
                15f, 5.0f, 30f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                BomberOption.TimerMaxTime,
                60f, 45f, 75f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateBoolOption(
                BomberOption.TellExplosion,
                true);
        }
    }
}
