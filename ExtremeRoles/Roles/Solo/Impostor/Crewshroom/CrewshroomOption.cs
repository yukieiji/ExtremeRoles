using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.Crewshroom.CrewshroomRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Crewshroom
{
    public readonly record struct CrewshroomSpecificOption(
        float DelaySecond,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class CrewshroomOptionLoader : ISpecificOptionLoader<CrewshroomSpecificOption>
    {
        public CrewshroomSpecificOption Load(IOptionLoader loader)
        {
            return new CrewshroomSpecificOption(
                loader.GetValue<Option, float>(
                    Option.DelaySecond),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class CrewshroomOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(factory, 3, 50);
            factory.CreateFloatOption(
                Option.DelaySecond, 5.0f, 0.5f, 30.0f, 0.5f, format: OptionUnit.Second);
        }
    }
}
