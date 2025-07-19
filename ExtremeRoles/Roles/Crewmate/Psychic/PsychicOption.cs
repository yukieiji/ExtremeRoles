using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Psychic
{
    public readonly record struct PsychicSpecificOption(
        int AwakeTaskGage,
        int AwakeDeadPlayerNum,
        bool IsUpgradeAbility,
        int UpgradeTaskGage,
        int UpgradeDeadPlayerNum,
        int AbilityUseCount,
        float AbilityActiveTime
    ) : IRoleSpecificOption;

    public class PsychicOptionLoader : ISpecificOptionLoader<PsychicSpecificOption>
    {
        public PsychicSpecificOption Load(IOptionLoader loader)
        {
            return new PsychicSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.AwakeTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.AwakeDeadPlayerNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.IsUpgradeAbility),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.UpgradeTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Psychic.PsychicOption.UpgradeDeadPlayerNum),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            );
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
