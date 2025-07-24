using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Supervisor.SupervisorRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Supervisor
{
    public readonly record struct SupervisorSpecificOption(
        bool IsBoostTask,
        int TaskGage,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class SupervisorOptionLoader : ISpecificOptionLoader<SupervisorSpecificOption>
    {
        public SupervisorSpecificOption Load(IOptionLoader loader)
        {
            return new SupervisorSpecificOption(
                loader.GetValue<SuperviosrOption, bool>(
                    SuperviosrOption.IsBoostTask),
                loader.GetValue<SuperviosrOption, int>(
                    SuperviosrOption.TaskGage),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class SupervisorOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateCommonAbilityOption(
                factory, 3.0f);

            var boostOption = factory.CreateBoolOption(
                SuperviosrOption.IsBoostTask,
                false);
            factory.CreateIntOption(
                SuperviosrOption.TaskGage,
                100, 50, 100, 5,
                boostOption,
                format: OptionUnit.Percentage);
        }
    }
}
