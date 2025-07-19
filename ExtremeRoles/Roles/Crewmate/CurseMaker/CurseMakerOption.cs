using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.CurseMaker
{
    public class CurseMakerSpecificOption : IRoleSpecificOption
    {
        public float CursingRange { get; set; }
        public float AdditionalKillCool { get; set; }
        public int TaskCurseTimeReduceRate { get; set; }
        public bool IsNotRemoveDeadBodyByTask { get; set; }
        public int NotRemoveDeadBodyTaskGage { get; set; }
        public bool IsDeadBodySearch { get; set; }
        public bool IsMultiDeadBodySearch { get; set; }
        public float SearchDeadBodyTime { get; set; }
        public bool IsReduceSearchForTask { get; set; }
        public int ReduceSearchTaskGage { get; set; }
        public float ReduceSearchDeadBodyTime { get; set; }
        public int AbilityUseCount { get; set; }
        public float AbilityActiveTime { get; set; }
    }

    public class CurseMakerOptionLoader : ISpecificOptionLoader<CurseMakerSpecificOption>
    {
        public CurseMakerSpecificOption Load(IOptionLoader loader)
        {
            return new CurseMakerSpecificOption
            {
                CursingRange = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.CursingRange),
                AdditionalKillCool = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.AdditionalKillCool),
                TaskCurseTimeReduceRate = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.TaskCurseTimeReduceRate),
                IsNotRemoveDeadBodyByTask = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.IsNotRemoveDeadBodyByTask),
                NotRemoveDeadBodyTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.NotRemoveDeadBodyTaskGage),
                IsDeadBodySearch = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.IsDeadBodySearch),
                IsMultiDeadBodySearch = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.IsMultiDeadBodySearch),
                SearchDeadBodyTime = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.SearchDeadBodyTime),
                IsReduceSearchForTask = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.IsReduceSearchForTask),
                ReduceSearchTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.ReduceSearchTaskGage),
                ReduceSearchDeadBodyTime = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.ReduceSearchDeadBodyTime),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                AbilityActiveTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            };
        }
    }

    public class CurseMakerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.CursingRange,
                2.5f, 0.5f, 5.0f, 0.5f);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.AdditionalKillCool,
                5.0f, 1.0f, 30.0f, 0.1f,
                format: OptionUnit.Second);

            IRoleAbility.CreateAbilityCountOption(
                factory, 1, 3, 5.0f);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.TaskCurseTimeReduceRate,
                0, 0, 10, 1,
                format: OptionUnit.Percentage);

            var removeDeadBodyOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.IsNotRemoveDeadBodyByTask,
                false);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.NotRemoveDeadBodyTaskGage,
                100, 0, 100, 5, removeDeadBodyOpt,
                format: OptionUnit.Percentage);

            var searchDeadBodyOption = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.IsDeadBodySearch,
                true);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.IsMultiDeadBodySearch,
                false, searchDeadBodyOption,
                invert: true);

            var searchTimeOpt = factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.SearchDeadBodyTime,
                60.0f, 0.5f, 120.0f, 0.5f,
                searchDeadBodyOption, format: OptionUnit.Second,
                invert: true);

            var taskBoostOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.IsReduceSearchForTask,
                false, searchDeadBodyOption,
                invert: true);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.ReduceSearchTaskGage,
                100, 25, 100, 5,
                taskBoostOpt,
                format: OptionUnit.Percentage,
                invert: true);

            var reduceTimeOpt = factory.CreateFloatDynamicOption(
                ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.ReduceSearchDeadBodyTime,
                30f, 0.5f, 0.5f, taskBoostOpt,
                format: OptionUnit.Second,
                invert: true,
                tempMaxValue: 120.0f);

            searchTimeOpt.AddWithUpdate(reduceTimeOpt);
        }
    }
}
