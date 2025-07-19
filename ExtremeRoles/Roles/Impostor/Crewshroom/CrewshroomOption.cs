using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Crewshroom
{
    public class CrewshroomSpecificOption : IRoleSpecificOption
    {
        public float DelaySecond { get; set; }
        public int AbilityUseCount { get; set; }
    }

    public class CrewshroomOptionLoader : ISpecificOptionLoader<CrewshroomSpecificOption>
    {
        public CrewshroomSpecificOption Load(IOptionLoader loader)
        {
            return new CrewshroomSpecificOption
            {
                DelaySecond = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Crewshroom.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Crewshroom.Option.DelaySecond),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            };
        }
    }

    public class CrewshroomOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(factory, 3, 50);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Crewshroom.Option.DelaySecond, 5.0f, 0.5f, 30.0f, 0.5f, format: OptionUnit.Second);
        }
    }
}
