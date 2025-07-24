using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Survivor
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
                loader.GetValue<SurvivorOption, int>(
                    SurvivorOption.AwakeTaskGage),
                loader.GetValue<SurvivorOption, int>(
                    SurvivorOption.DeadWinTaskGage),
                loader.GetValue<SurvivorOption, bool>(
                    SurvivorOption.NoWinSurvivorAssignGhostRole)
            );
        }
    }

    public class SurvivorOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                SurvivorOption.AwakeTaskGage,
                70, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                SurvivorOption.DeadWinTaskGage,
                100, 50, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateBoolOption(
                SurvivorOption.NoWinSurvivorAssignGhostRole,
                true);
        }
    }
}
