using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.OverLoader.OverLoaderRole;

namespace ExtremeRoles.Roles.Solo.Impostor.OverLoader
{
    public readonly record struct OverLoaderSpecificOption(
        int AwakeImpostorNum,
        int AwakeKillCount,
        float KillCoolReduceRate,
        float MoveSpeed,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class OverLoaderOptionLoader : ISpecificOptionLoader<OverLoaderSpecificOption>
    {
        public OverLoaderSpecificOption Load(IOptionLoader loader)
        {
            return new OverLoaderSpecificOption(
                loader.GetValue<OverLoaderOption, int>(
                    OverLoaderOption.AwakeImpostorNum),
                loader.GetValue<OverLoaderOption, int>(
                    OverLoaderOption.AwakeKillCount),
                loader.GetValue<OverLoaderOption, float>(
                    OverLoaderOption.KillCoolReduceRate),
                loader.GetValue<OverLoaderOption, float>(
                    OverLoaderOption.MoveSpeed),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class OverLoaderOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                OverLoaderOption.AwakeImpostorNum,
                GameSystem.MaxImposterNum, 1,
                GameSystem.MaxImposterNum, 1);

            factory.CreateIntOption(
                OverLoaderOption.AwakeKillCount,
                0, 0, 3, 1);

            IRoleAbility.CreateCommonAbilityOption(
                factory, 7.5f);

            factory.CreateFloatOption(
                OverLoaderOption.KillCoolReduceRate,
                75.0f, 50.0f, 90.0f, 1.0f,
                format: OptionUnit.Percentage);
            factory.CreateFloatOption(
                OverLoaderOption.MoveSpeed,
                1.5f, 1.0f, 3.0f, 0.1f,
                format: OptionUnit.Multiplier);
        }
    }
}
