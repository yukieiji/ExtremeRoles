using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.OverLoader
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.OverLoader.OverLoaderOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.OverLoader.OverLoaderOption.AwakeImpostorNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.OverLoader.OverLoaderOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.OverLoader.OverLoaderOption.AwakeKillCount),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.OverLoader.OverLoaderOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.OverLoader.OverLoaderOption.KillCoolReduceRate),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.OverLoader.OverLoaderOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.OverLoader.OverLoaderOption.MoveSpeed),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class OverLoaderOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.OverLoader.OverLoaderOption.AwakeImpostorNum,
                GameSystem.MaxImposterNum, 1,
                GameSystem.MaxImposterNum, 1);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.OverLoader.OverLoaderOption.AwakeKillCount,
                0, 0, 3, 1);

            IRoleAbility.CreateCommonAbilityOption(
                factory, 7.5f);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.OverLoader.OverLoaderOption.KillCoolReduceRate,
                75.0f, 50.0f, 90.0f, 1.0f,
                format: OptionUnit.Percentage);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.OverLoader.OverLoaderOption.MoveSpeed,
                1.5f, 1.0f, 3.0f, 0.1f,
                format: OptionUnit.Multiplier);
        }
    }
}
