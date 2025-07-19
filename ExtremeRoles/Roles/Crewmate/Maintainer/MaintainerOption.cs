using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Crewmate.Maintainer
{
    public readonly record struct MaintainerSpecificOption(
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class MaintainerOptionLoader : ISpecificOptionLoader<MaintainerSpecificOption>
    {
        public MaintainerSpecificOption Load(IOptionLoader loader)
        {
            return new MaintainerSpecificOption(
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class MaintainerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 10);
        }
    }
}
