using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Fencer.FencerRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Fencer
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
                loader.GetValue<FencerOption, float>(
                    FencerOption.ResetTime),
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
                FencerOption.ResetTime,
                5.0f, 2.5f, 30.0f, 0.5f,
                format: OptionUnit.Second);
        }
    }
}
