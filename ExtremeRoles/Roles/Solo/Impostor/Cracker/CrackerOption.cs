using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using static ExtremeRoles.Roles.Solo.Impostor.Cracker.CrackerRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Cracker
{
    public readonly record struct CrackerSpecificOption(
        bool RemoveDeadBody,
        float CanCrackDistance,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class CrackerOptionLoader : ISpecificOptionLoader<CrackerSpecificOption>
    {
        public CrackerSpecificOption Load(IOptionLoader loader)
        {
            return new CrackerSpecificOption(
                loader.GetValue<CrackerOption, bool>(
                    CrackerOption.RemoveDeadBody),
                loader.GetValue<CrackerOption, float>(
                    CrackerOption.CanCrackDistance),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class CrackerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 5);

            factory.CreateFloatOption(
                CrackerOption.CanCrackDistance,
                1.0f, 1.0f, 5.0f, 0.5f);

            factory.CreateBoolOption(
                CrackerOption.RemoveDeadBody,
                false);
        }
    }
}
