using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Survivor
{
    public class SurvivorSpecificOption : IRoleSpecificOption
    {
        public int AwakeTaskGage { get; set; }
        public int DeadWinTaskGage { get; set; }
        public bool NoWinSurvivorAssignGhostRole { get; set; }
    }

    public class SurvivorOptionLoader : ISpecificOptionLoader<SurvivorSpecificOption>
    {
        public SurvivorSpecificOption Load(IOptionLoader loader)
        {
            return new SurvivorSpecificOption
            {
                AwakeTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption.AwakeTaskGage),
                DeadWinTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption.DeadWinTaskGage),
                NoWinSurvivorAssignGhostRole = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Survivor.SurvivorOption.NoWinSurvivorAssignGhostRole)
            };
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
