using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.SandWorm
{
    public class SandWormSpecificOption : IRoleSpecificOption
    {
        public float AssaultKillCoolReduce { get; set; }
        public float KillCoolPenalty { get; set; }
        public float AssaultRange { get; set; }
        public float AbilityCoolTime { get; set; }
    }

    public class SandWormOptionLoader : ISpecificOptionLoader<SandWormSpecificOption>
    {
        public SandWormSpecificOption Load(IOptionLoader loader)
        {
            return new SandWormSpecificOption
            {
                AssaultKillCoolReduce = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption.AssaultKillCoolReduce),
                KillCoolPenalty = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption.KillCoolPenalty),
                AssaultRange = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.SandWorm.SandWormOption.AssaultRange),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
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
