using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using static ExtremeRoles.Roles.Solo.Impostor.Mery.MeryRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Mery
{
    public readonly record struct MerySpecificOption(
        int ActiveNum,
        float ActiveRange,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class MeryOptionLoader : ISpecificOptionLoader<MerySpecificOption>
    {
        public MerySpecificOption Load(IOptionLoader loader)
        {
            return new MerySpecificOption(
                loader.GetValue<MeryOption, int>(
                    MeryOption.ActiveNum),
                loader.GetValue<MeryOption, float>(
                    MeryOption.ActiveRange),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class MeryOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 3, 5);

            factory.CreateIntOption(
                MeryOption.ActiveNum,
                3, 1, 5, 1);
            factory.CreateFloatOption(
                MeryOption.ActiveRange,
                2.0f, 0.1f, 3.0f, 0.1f);
        }
    }
}
