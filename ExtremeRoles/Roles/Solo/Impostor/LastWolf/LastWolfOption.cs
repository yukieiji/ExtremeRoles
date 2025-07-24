using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfRole;

namespace ExtremeRoles.Roles.Solo.Impostor.LastWolf
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
                loader.GetValue<LastWolfOption, int>(
                    LastWolfOption.AwakeImpostorNum),
                loader.GetValue<LastWolfOption, float>(
                    LastWolfOption.DeadPlayerNumBonus),
                loader.GetValue<LastWolfOption, float>(
                    LastWolfOption.KillPlayerNumBonus),
                loader.GetValue<LastWolfOption, float>(
                    LastWolfOption.LightOffVision),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            );
        }
    }

    public class LastWolfOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                LastWolfOption.AwakeImpostorNum,
                1, 1, GameSystem.MaxImposterNum, 1);

            factory.CreateFloatOption(
                LastWolfOption.DeadPlayerNumBonus,
                1.0f, 2.0f, 6.5f, 0.1f,
                format: OptionUnit.Percentage);

            factory.CreateFloatOption(
                LastWolfOption.KillPlayerNumBonus,
                2.5f, 4.0f, 10.0f, 0.1f,
                format: OptionUnit.Percentage);

            IRoleAbility.CreateCommonAbilityOption(
                factory, 10.0f);

            factory.CreateFloatOption(
                LastWolfOption.LightOffVision,
                0.1f, 0.0f, 1.0f, 0.1f);
        }
    }
}
