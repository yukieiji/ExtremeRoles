using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.BodyGuard
{
    public readonly record struct BodyGuardSpecificOption(
        float ShieldRange,
        int FeatMeetingAbilityTaskGage,
        int FeatMeetingReportTaskGage,
        bool IsReportPlayerName,
        int ReportPlayerMode,
        bool IsBlockMeetingKill,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class BodyGuardOptionLoader : ISpecificOptionLoader<BodyGuardSpecificOption>
    {
        public BodyGuardSpecificOption Load(IOptionLoader loader)
        {
            return new BodyGuardSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption.ShieldRange),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption.FeatMeetingAbilityTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption.FeatMeetingReportTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption.IsReportPlayerName),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption.ReportPlayerMode),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption.IsBlockMeetingKill),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class BodyGuardOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption.ShieldRange,
                1.0f, 0.0f, 2.0f, 0.1f);

            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 5);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption.FeatMeetingAbilityTaskGage,
                30, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption.FeatMeetingReportTaskGage,
                60, 0, 100, 10,
                format: OptionUnit.Percentage);
            var reportPlayerNameOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption.IsReportPlayerName,
                false);
            factory.CreateSelectionOption<ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption, ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardReportPlayerNameMode>(
                ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption.ReportPlayerMode,
                reportPlayerNameOpt);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardOption.IsBlockMeetingKill,
                true);
        }
    }
}
