using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormRole;

namespace ExtremeRoles.Roles.Solo.Impostor.SandWorm
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
                loader.GetValue<SandWormOption, float>(
                    SandWormOption.AssaultKillCoolReduce),
                loader.GetValue<SandWormOption, float>(
                    SandWormOption.KillCoolPenalty),
                loader.GetValue<SandWormOption, float>(
                    SandWormOption.AssaultRange),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class SandWormOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                SandWormOption.KillCoolPenalty,
                5.0f, 1.0f, 10.0f, 0.1f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                SandWormOption.AssaultKillCoolReduce,
                3.0f, 1.0f, 5.0f, 0.1f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                SandWormOption.AssaultRange,
                2.0f, 0.1f, 3.0f, 0.1f);

            factory.CreateFloatOption(
                RoleAbilityCommonOption.AbilityCoolTime,
                15.0f, 0.5f, 45.0f, 0.1f,
                format: OptionUnit.Second);
        }
    }
}
