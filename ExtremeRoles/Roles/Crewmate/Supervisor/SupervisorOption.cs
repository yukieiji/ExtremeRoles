using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Supervisor
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Supervisor.SuperviosrOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Supervisor.SuperviosrOption.IsBoostTask),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Supervisor.SuperviosrOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Supervisor.SuperviosrOption.TaskGage),
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
                ExtremeRoles.Roles.Solo.Crewmate.Supervisor.SuperviosrOption.IsBoostTask,
                false);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Supervisor.SuperviosrOption.TaskGage,
                100, 50, 100, 5,
                boostOption,
                format: OptionUnit.Percentage);
        }
    }
}
