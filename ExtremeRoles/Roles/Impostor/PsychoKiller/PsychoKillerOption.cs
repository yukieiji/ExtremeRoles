using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.PsychoKiller
{
    public readonly record struct PsychoKillerSpecificOption(
        int KillCoolReduceRate,
        int CombMax,
        bool CombResetWhenMeeting,
        bool HasSelfKillTimer,
        float SelfKillTimerTime,
        bool IsForceRestartWhenMeetingEnd,
        bool IsDiactiveUntilKillWhenMeetingEnd,
        int SelfKillTimerModRate
    ) : IRoleSpecificOption;

    public class PsychoKillerOptionLoader : ISpecificOptionLoader<PsychoKillerSpecificOption>
    {
        public PsychoKillerSpecificOption Load(IOptionLoader loader)
        {
            return new PsychoKillerSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.KillCoolReduceRate),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.CombMax),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.CombResetWhenMeeting),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.HasSelfKillTimer),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.SelfKillTimerTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.IsForceRestartWhenMeetingEnd),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.IsDiactiveUntilKillWhenMeetingEnd),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.SelfKillTimerModRate)
            );
        }
    }

    public class PsychoKillerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.KillCoolReduceRate,
                5, 1, 15, 1,
                format: OptionUnit.Percentage);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.CombMax,
                2, 1, 5, 1);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.CombResetWhenMeeting,
                true);

            var hasSelfKillTimer = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.HasSelfKillTimer,
                false);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.SelfKillTimerTime,
                30.0f, 5.0f, 120.0f, 0.5f,
                hasSelfKillTimer,
                format: OptionUnit.Second);
            var timerOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.IsForceRestartWhenMeetingEnd,
                false, hasSelfKillTimer);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.IsDiactiveUntilKillWhenMeetingEnd,
                false, timerOpt,
                invert: true);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerOption.SelfKillTimerModRate,
                0, -50, 50, 1, hasSelfKillTimer,
                format: OptionUnit.Percentage);
        }
    }
}
