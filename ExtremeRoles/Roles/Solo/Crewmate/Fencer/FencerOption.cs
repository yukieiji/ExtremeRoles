using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Fencer
{
    public readonly record struct FencerSpecificOption(
        float ResetTime,
        int AbilityUseCount,
        float AbilityActiveTime
    ) : IRoleSpecificOption;

    public class FencerOptionLoader : ISpecificOptionLoader<FencerSpecificOption>
    {
        public FencerSpecificOption Load(IOptionLoader loader)
        {
            return new FencerSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Fencer.FencerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Fencer.FencerOption.ResetTime),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            );
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
