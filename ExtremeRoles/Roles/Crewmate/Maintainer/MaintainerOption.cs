using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Crewmate.Maintainer
{
    public class MaintainerSpecificOption : IRoleSpecificOption
    {
        public int AbilityUseCount { get; set; }
    }

    public class MaintainerOptionLoader : ISpecificOptionLoader<MaintainerSpecificOption>
    {
        public MaintainerSpecificOption Load(IOptionLoader loader)
        {
            return new MaintainerSpecificOption
            {
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            };
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
