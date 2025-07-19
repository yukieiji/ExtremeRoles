using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Fencer
{
    public class FencerSpecificOption : IRoleSpecificOption
    {
        public float ResetTime { get; set; }
        public int AbilityUseCount { get; set; }
        public float AbilityActiveTime { get; set; }
    }

    public class FencerOptionLoader : ISpecificOptionLoader<FencerSpecificOption>
    {
        public FencerSpecificOption Load(IOptionLoader loader)
        {
            return new FencerSpecificOption
            {
                ResetTime = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Fencer.FencerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Fencer.FencerOption.ResetTime),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                AbilityActiveTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            };
        }
    }

    public class FencerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 7, 3.0f);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Fencer.FencerOption.ResetTime,
                5.0f, 2.5f, 30.0f, 0.5f,
                format: OptionUnit.Second);
        }
    }
}
