using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.LastWolf
{
    public readonly record struct LastWolfSpecificOption(
        int AwakeImpostorNum,
        float DeadPlayerNumBonus,
        float KillPlayerNumBonus,
        float LightOffVision,
        float AbilityActiveTime
    ) : IRoleSpecificOption;

    public class LastWolfOptionLoader : ISpecificOptionLoader<LastWolfSpecificOption>
    {
        public LastWolfSpecificOption Load(IOptionLoader loader)
        {
            return new LastWolfSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.AwakeImpostorNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.DeadPlayerNumBonus),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.KillPlayerNumBonus),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.LightOffVision),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            );
        }
    }

    public class LastWolfOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.AwakeImpostorNum,
                1, 1, GameSystem.MaxImposterNum, 1);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.DeadPlayerNumBonus,
                1.0f, 2.0f, 6.5f, 0.1f,
                format: OptionUnit.Percentage);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.KillPlayerNumBonus,
                2.5f, 4.0f, 10.0f, 0.1f,
                format: OptionUnit.Percentage);

            IRoleAbility.CreateCommonAbilityOption(
                factory, 10.0f);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.LightOffVision,
                0.1f, 0.0f, 1.0f, 0.1f);
        }
    }
}
