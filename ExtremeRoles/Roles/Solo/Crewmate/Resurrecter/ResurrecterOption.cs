using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Resurrecter
{
    public readonly record struct ResurrecterSpecificOption(
        int AwakeTaskGage,
        int ResurrectTaskGage,
        float ResurrectDelayTime,
        bool IsMeetingCoolResetOnResurrect,
        float ResurrectMeetingCooltime,
        int ResurrectTaskResetMeetingNum,
        int ResurrectTaskResetGage,
        bool CanResurrectAfterDeath,
        bool CanResurrectOnExil
    ) : IRoleSpecificOption;

    public class ResurrecterOptionLoader : ISpecificOptionLoader<ResurrecterSpecificOption>
    {
        public ResurrecterSpecificOption Load(IOptionLoader loader)
        {
            return new ResurrecterSpecificOption(
                loader.GetValue<ResurrecterOption, int>(
                    ResurrecterOption.AwakeTaskGage),
                loader.GetValue<ResurrecterOption, int>(
                    ResurrecterOption.ResurrectTaskGage),
                loader.GetValue<ResurrecterOption, float>(
                    ResurrecterOption.ResurrectDelayTime),
                loader.GetValue<ResurrecterOption, bool>(
                    ResurrecterOption.IsMeetingCoolResetOnResurrect),
                loader.GetValue<ResurrecterOption, float>(
                    ResurrecterOption.ResurrectMeetingCooltime),
                loader.GetValue<ResurrecterOption, int>(
                    ResurrecterOption.ResurrectTaskResetMeetingNum),
                loader.GetValue<ResurrecterOption, int>(
                    ResurrecterOption.ResurrectTaskResetGage),
                loader.GetValue<ResurrecterOption, bool>(
                    ResurrecterOption.CanResurrectAfterDeath),
                loader.GetValue<ResurrecterOption, bool>(
                    ResurrecterOption.CanResurrectOnExil)
            );
        }
    }

    public class ResurrecterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ResurrecterOption.AwakeTaskGage,
                100, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                ResurrecterOption.ResurrectTaskGage,
                100, 50, 100, 10,
                format: OptionUnit.Percentage);

            factory.CreateFloatOption(
                ResurrecterOption.ResurrectDelayTime,
                5.0f, 4.0f, 60.0f, 0.1f,
                format: OptionUnit.Second);

            var meetingResetOpt = factory.CreateBoolOption(
                ResurrecterOption.IsMeetingCoolResetOnResurrect,
                true);

            factory.CreateFloatOption(
                ResurrecterOption.ResurrectMeetingCooltime,
                20.0f, 5.0f, 60.0f, 0.25f,
                meetingResetOpt,
                format: OptionUnit.Second,
                invert: true);

            factory.CreateIntOption(
                ResurrecterOption.ResurrectTaskResetMeetingNum,
                1, 1, 5, 1);

            factory.CreateIntOption(
                ResurrecterOption.ResurrectTaskResetGage,
                20, 10, 50, 5,
                format: OptionUnit.Percentage);
            factory.CreateBoolOption(
                ResurrecterOption.CanResurrectAfterDeath,
                false);
            factory.CreateBoolOption(
                ResurrecterOption.CanResurrectOnExil,
                false);
        }
    }
}
