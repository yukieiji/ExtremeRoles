using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.BodyGuard.BodyGuardRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.BodyGuard
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
                loader.GetValue<BodyGuardOption, float>(
                    BodyGuardOption.ShieldRange),
                loader.GetValue<BodyGuardOption, int>(
                    BodyGuardOption.FeatMeetingAbilityTaskGage),
                loader.GetValue<BodyGuardOption, int>(
                    BodyGuardOption.FeatMeetingReportTaskGage),
                loader.GetValue<BodyGuardOption, bool>(
                    BodyGuardOption.IsReportPlayerName),
                loader.GetValue<BodyGuardOption, int>(
                    BodyGuardOption.ReportPlayerMode),
                loader.GetValue<BodyGuardOption, bool>(
                    BodyGuardOption.IsBlockMeetingKill),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class BodyGuardOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                BodyGuardOption.ShieldRange,
                1.0f, 0.0f, 2.0f, 0.1f);

            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 5);

            factory.CreateIntOption(
                BodyGuardOption.FeatMeetingAbilityTaskGage,
                30, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                BodyGuardOption.FeatMeetingReportTaskGage,
                60, 0, 100, 10,
                format: OptionUnit.Percentage);
            var reportPlayerNameOpt = factory.CreateBoolOption(
                BodyGuardOption.IsReportPlayerName,
                false);
            factory.CreateSelectionOption<BodyGuardOption, BodyGuardReportPlayerNameMode>(
                BodyGuardOption.ReportPlayerMode,
                reportPlayerNameOpt);
            factory.CreateBoolOption(
                BodyGuardOption.IsBlockMeetingKill,
                true);
        }
    }
}
