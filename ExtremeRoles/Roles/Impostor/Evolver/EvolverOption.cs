using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Evolver
{
    public class EvolverSpecificOption : IRoleSpecificOption
    {
        public bool IsEvolvedAnimation { get; set; }
        public bool IsEatingEndCleanBody { get; set; }
        public float EatingRange { get; set; }
        public int KillCoolReduceRate { get; set; }
        public float KillCoolResuceRateMulti { get; set; }
        public int AbilityUseCount { get; set; }
        public float AbilityActiveTime { get; set; }
    }

    public class EvolverOptionLoader : ISpecificOptionLoader<EvolverSpecificOption>
    {
        public EvolverSpecificOption Load(IOptionLoader loader)
        {
            return new EvolverSpecificOption
            {
                IsEvolvedAnimation = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption.IsEvolvedAnimation),
                IsEatingEndCleanBody = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption.IsEatingEndCleanBody),
                EatingRange = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption.EatingRange),
                KillCoolReduceRate = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption.KillCoolReduceRate),
                KillCoolResuceRateMulti = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption.KillCoolResuceRateMulti),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                AbilityActiveTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            };
        }
    }

    public class EvolverOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption.IsEvolvedAnimation,
                true);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption.IsEatingEndCleanBody,
                true);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption.EatingRange,
                2.5f, 0.5f, 5.0f, 0.5f);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption.KillCoolReduceRate,
                10, 1, 50, 1,
                format: OptionUnit.Percentage);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Evolver.EvolverOption.KillCoolResuceRateMulti,
                1.0f, 1.0f, 5.0f, 0.1f,
                format: OptionUnit.Multiplier);

            IRoleAbility.CreateAbilityCountOption(
                factory, 5, 10, 5.0f);
        }
    }
}
