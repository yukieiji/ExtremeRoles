using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.PsychoKiller.PsychoKillerRole;

namespace ExtremeRoles.Roles.Solo.Impostor.PsychoKiller
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
                loader.GetValue<PsychoKillerOption, int>(
                    PsychoKillerOption.KillCoolReduceRate),
                loader.GetValue<PsychoKillerOption, int>(
                    PsychoKillerOption.CombMax),
                loader.GetValue<PsychoKillerOption, bool>(
                    PsychoKillerOption.CombResetWhenMeeting),
                loader.GetValue<PsychoKillerOption, bool>(
                    PsychoKillerOption.HasSelfKillTimer),
                loader.GetValue<PsychoKillerOption, float>(
                    PsychoKillerOption.SelfKillTimerTime),
                loader.GetValue<PsychoKillerOption, bool>(
                    PsychoKillerOption.IsForceRestartWhenMeetingEnd),
                loader.GetValue<PsychoKillerOption, bool>(
                    PsychoKillerOption.IsDiactiveUntilKillWhenMeetingEnd),
                loader.GetValue<PsychoKillerOption, int>(
                    PsychoKillerOption.SelfKillTimerModRate)
            );
        }
    }

    public class PsychoKillerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                PsychoKillerOption.KillCoolReduceRate,
                5, 1, 15, 1,
                format: OptionUnit.Percentage);

            factory.CreateIntOption(
                PsychoKillerOption.CombMax,
                2, 1, 5, 1);

            factory.CreateBoolOption(
                PsychoKillerOption.CombResetWhenMeeting,
                true);

            var hasSelfKillTimer = factory.CreateBoolOption(
                PsychoKillerOption.HasSelfKillTimer,
                false);
            factory.CreateFloatOption(
                PsychoKillerOption.SelfKillTimerTime,
                30.0f, 5.0f, 120.0f, 0.5f,
                hasSelfKillTimer,
                format: OptionUnit.Second);
            var timerOpt = factory.CreateBoolOption(
                PsychoKillerOption.IsForceRestartWhenMeetingEnd,
                false, hasSelfKillTimer);
            factory.CreateBoolOption(
                PsychoKillerOption.IsDiactiveUntilKillWhenMeetingEnd,
                false, timerOpt,
                invert: true);
            factory.CreateIntOption(
                PsychoKillerOption.SelfKillTimerModRate,
                0, -50, 50, 1, hasSelfKillTimer,
                format: OptionUnit.Percentage);
        }
    }
}
