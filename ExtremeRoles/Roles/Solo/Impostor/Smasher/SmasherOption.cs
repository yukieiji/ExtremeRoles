using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Smasher
{
    public readonly record struct SmasherSpecificOption(
        float SmashPenaltyKillCool,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class SmasherOptionLoader : ISpecificOptionLoader<SmasherSpecificOption>
    {
        public SmasherSpecificOption Load(IOptionLoader loader)
        {
            return new SmasherSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Smasher.SmasherOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Smasher.SmasherOption.SmashPenaltyKillCool),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class SmasherOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 1, 14);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Smasher.SmasherOption.SmashPenaltyKillCool,
                4.0f, 0.0f, 30f, 0.5f,
                format: OptionUnit.Second);
        }
    }
}
