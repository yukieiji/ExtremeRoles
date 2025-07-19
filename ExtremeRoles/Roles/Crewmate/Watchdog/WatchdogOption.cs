using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Crewmate.Watchdog
{
    public class WatchdogSpecificOption : IRoleSpecificOption
    {
        public float AbilityCoolTime { get; set; }
    }

    public class WatchdogOptionLoader : ISpecificOptionLoader<WatchdogSpecificOption>
    {
        public WatchdogSpecificOption Load(IOptionLoader loader)
        {
            return new WatchdogSpecificOption
            {
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
        }
    }

    public class WatchdogOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateCommonAbilityOption(
                factory, 3.0f);
        }
    }
}
