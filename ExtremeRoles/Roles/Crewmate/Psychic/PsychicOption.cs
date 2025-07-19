using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Psychic
{
    public class PsychicSpecificOption : IRoleSpecificOption
    {
        public int AwakeTaskGage { get; set; }
        public int AwakeDeadPlayerNum { get; set; }
        public bool IsUpgradeAbility { get; set; }
        public int UpgradeTaskGage { get; set; }
        public int UpgradeDeadPlayerNum { get; set; }
        public int AbilityUseCount { get; set; }
        public float AbilityActiveTime { get; set; }
    }

    public class PsychicOptionLoader : ISpecificOptionLoader<PsychicSpecificOption>
    {
        public PsychicSpecificOption Load(IOptionLoader loader)
        {
            return new PsychicSpecificOption
            {
                AwakeTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.AwakeTaskGage),
                AwakeDeadPlayerNum = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.AwakeDeadPlayerNum),
                IsUpgradeAbility = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.IsUpgradeAbility),
                UpgradeTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.UpgradeTaskGage),
                UpgradeDeadPlayerNum = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.UpgradeDeadPlayerNum),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                AbilityActiveTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            };
        }
    }

    public class PsychicOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.AwakeTaskGage,
                30, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
               ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.AwakeDeadPlayerNum,
               2, 0, 7, 1);

            IRoleAbility.CreateAbilityCountOption(
                factory, 1, 5, 3.0f);

            var isUpgradeOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.IsUpgradeAbility,
                false);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.UpgradeTaskGage,
                70, 0, 100, 10,
                isUpgradeOpt,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
               ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.UpgradeDeadPlayerNum,
               5, 0, 15, 1, isUpgradeOpt);
        }
    }
}
