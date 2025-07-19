using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Resurrecter
{
    public class ResurrecterSpecificOption : IRoleSpecificOption
    {
        public int AwakeTaskGage { get; set; }
        public int ResurrectTaskGage { get; set; }
        public float ResurrectDelayTime { get; set; }
        public bool IsMeetingCoolResetOnResurrect { get; set; }
        public float ResurrectMeetingCooltime { get; set; }
        public int ResurrectTaskResetMeetingNum { get; set; }
        public int ResurrectTaskResetGage { get; set; }
        public bool CanResurrectAfterDeath { get; set; }
        public bool CanResurrectOnExil { get; set; }
    }

    public class ResurrecterOptionLoader : ISpecificOptionLoader<ResurrecterSpecificOption>
    {
        public ResurrecterSpecificOption Load(IOptionLoader loader)
        {
            return new ResurrecterSpecificOption
            {
                AwakeTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.AwakeTaskGage),
                ResurrectTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.ResurrectTaskGage),
                ResurrectDelayTime = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.ResurrectDelayTime),
                IsMeetingCoolResetOnResurrect = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.IsMeetingCoolResetOnResurrect),
                ResurrectMeetingCooltime = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.ResurrectMeetingCooltime),
                ResurrectTaskResetMeetingNum = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.ResurrectTaskResetMeetingNum),
                ResurrectTaskResetGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.ResurrectTaskResetGage),
                CanResurrectAfterDeath = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.CanResurrectAfterDeath),
                CanResurrectOnExil = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.CanResurrectOnExil)
            };
        }
    }

    public class ResurrecterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.AwakeTaskGage,
                100, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.ResurrectTaskGage,
                100, 50, 100, 10,
                format: OptionUnit.Percentage);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.ResurrectDelayTime,
                5.0f, 4.0f, 60.0f, 0.1f,
                format: OptionUnit.Second);

            var meetingResetOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.IsMeetingCoolResetOnResurrect,
                true);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.ResurrectMeetingCooltime,
                20.0f, 5.0f, 60.0f, 0.25f,
                meetingResetOpt,
                format: OptionUnit.Second,
                invert: true);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.ResurrectTaskResetMeetingNum,
                1, 1, 5, 1);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.ResurrectTaskResetGage,
                20, 10, 50, 5,
                format: OptionUnit.Percentage);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.CanResurrectAfterDeath,
                false);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Resurrecter.ResurrecterOption.CanResurrectOnExil,
                false);
        }
    }
}
