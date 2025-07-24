using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Evolver
{
    public readonly record struct EvolverSpecificOption(
        bool IsEvolvedAnimation,
        bool IsEatingEndCleanBody,
        float EatingRange,
        int KillCoolReduceRate,
        float KillCoolResuceRateMulti,
        int AbilityUseCount,
        float AbilityActiveTime
    ) : IRoleSpecificOption;

    public class EvolverOptionLoader : ISpecificOptionLoader<EvolverSpecificOption>
    {
        public EvolverSpecificOption Load(IOptionLoader loader)
        {
            return new EvolverSpecificOption(
                loader.GetValue<EvolverOption, bool>(
                    EvolverOption.IsEvolvedAnimation),
                loader.GetValue<EvolverOption, bool>(
                    EvolverOption.IsEatingEndCleanBody),
                loader.GetValue<EvolverOption, float>(
                    EvolverOption.EatingRange),
                loader.GetValue<EvolverOption, int>(
                    EvolverOption.KillCoolReduceRate),
                loader.GetValue<EvolverOption, float>(
                    EvolverOption.KillCoolResuceRateMulti),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            );
        }
    }

    public class EvolverOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                EvolverOption.IsEvolvedAnimation,
                true);

            factory.CreateBoolOption(
                EvolverOption.IsEatingEndCleanBody,
                true);

            factory.CreateFloatOption(
                EvolverOption.EatingRange,
                2.5f, 0.5f, 5.0f, 0.5f);

            factory.CreateIntOption(
                EvolverOption.KillCoolReduceRate,
                10, 1, 50, 1,
                format: OptionUnit.Percentage);

            factory.CreateFloatOption(
                EvolverOption.KillCoolResuceRateMulti,
                1.0f, 1.0f, 5.0f, 0.1f,
                format: OptionUnit.Multiplier);

            IRoleAbility.CreateAbilityCountOption(
                factory, 5, 10, 5.0f);
        }
    }
}
