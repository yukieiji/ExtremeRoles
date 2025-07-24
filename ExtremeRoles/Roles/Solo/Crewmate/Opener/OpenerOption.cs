using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Opener.OpenerRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Opener
{
    public readonly record struct OpenerSpecificOption(
        float Range,
        int ReduceRate,
        int PlusAbility,
        int AbilityUseCount,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class OpenerOptionLoader : ISpecificOptionLoader<OpenerSpecificOption>
    {
        public OpenerSpecificOption Load(IOptionLoader loader)
        {
            return new OpenerSpecificOption(
                loader.GetValue<OpenerOption, float>(
                    OpenerOption.Range),
                loader.GetValue<OpenerOption, int>(
                    OpenerOption.ReduceRate),
                loader.GetValue<OpenerOption, int>(
                    OpenerOption.PlusAbility),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class OpenerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 5);
            factory.CreateFloatOption(
                OpenerOption.Range,
                2.0f, 0.5f, 5.0f, 0.1f);
            factory.CreateIntOption(
                OpenerOption.ReduceRate,
                45, 5, 95, 1,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                OpenerOption.PlusAbility,
                5, 1, 10, 1,
                format: OptionUnit.Shot);
        }
    }
}
