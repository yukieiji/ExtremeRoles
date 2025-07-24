using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Summoner.SummonerRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Summoner
{
    public readonly record struct SummonerSpecificOption(
        int MarkingCount,
        int SummonCount,
        float Range,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class SummonerOptionLoader : ISpecificOptionLoader<SummonerSpecificOption>
    {
        public SummonerSpecificOption Load(IOptionLoader loader)
        {
            return new SummonerSpecificOption(
                loader.GetValue<Option, int>(
                    Option.MarkingCount),
                loader.GetValue<Option, int>(
                    Option.SummonCount),
                loader.GetValue<Option, float>(
                    Option.Range),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class SummonerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                RoleAbilityCommonOption.AbilityCoolTime,
                IRoleAbility.DefaultCoolTime,
                IRoleAbility.MinCoolTime,
                IRoleAbility.MaxCoolTime,
                IRoleAbility.Step,
                format: OptionUnit.Second);

            factory.CreateIntOption(
                Option.MarkingCount,
                3, 1, 10, 1);

            factory.CreateFloatOption(
                Option.Range,
                2.5f, 0.0f, 7.5f, 0.1f);

            factory.CreateIntOption(
                Option.SummonCount,
                3, 1, 10, 1);
        }
    }
}
