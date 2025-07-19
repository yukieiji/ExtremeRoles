using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Survivor
{
    public readonly record struct SurvivorSpecificOption(
        int AwakeTaskGage,
        int DeadWinTaskGage,
        bool NoWinSurvivorAssignGhostRole
    ) : IRoleSpecificOption;

    public class SurvivorOptionLoader : ISpecificOptionLoader<SurvivorSpecificOption>
    {
        public SurvivorSpecificOption Load(IOptionLoader loader)
        {
            return new SurvivorSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption.AwakeTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption.DeadWinTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption.NoWinSurvivorAssignGhostRole)
            );
        }
    }

    public class SurvivorOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption.AwakeTaskGage,
                70, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption.DeadWinTaskGage,
                100, 50, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption.NoWinSurvivorAssignGhostRole,
                true);
        }
    }
}
