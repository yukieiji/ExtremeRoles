using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.SandWorm
{
    public readonly record struct SandWormSpecificOption(
        float AssaultKillCoolReduce,
        float KillCoolPenalty,
        float AssaultRange,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class SandWormOptionLoader : ISpecificOptionLoader<SandWormSpecificOption>
    {
        public SandWormSpecificOption Load(IOptionLoader loader)
        {
            return new SandWormSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption.AssaultKillCoolReduce),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption.KillCoolPenalty),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption.AssaultRange),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class SandWormOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption.KillCoolPenalty,
                5.0f, 1.0f, 10.0f, 0.1f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption.AssaultKillCoolReduce,
                3.0f, 1.0f, 5.0f, 0.1f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption.AssaultRange,
                2.0f, 0.1f, 3.0f, 0.1f);

            factory.CreateFloatOption(
                RoleAbilityCommonOption.AbilityCoolTime,
                15.0f, 0.5f, 45.0f, 0.1f,
                format: OptionUnit.Second);
        }
    }
}
