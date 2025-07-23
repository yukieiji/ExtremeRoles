using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.CurseMaker
{
    public readonly record struct CurseMakerSpecificOption(
        float CursingRange,
        float AdditionalKillCool,
        int TaskCurseTimeReduceRate,
        bool IsNotRemoveDeadBodyByTask,
        int NotRemoveDeadBodyTaskGage,
        bool IsDeadBodySearch,
        bool IsMultiDeadBodySearch,
        float SearchDeadBodyTime,
        bool IsReduceSearchForTask,
        int ReduceSearchTaskGage,
        float ReduceSearchDeadBodyTime,
        int AbilityUseCount,
        float AbilityActiveTime
    ) : IRoleSpecificOption;

    public class CurseMakerOptionLoader : ISpecificOptionLoader<CurseMakerSpecificOption>
    {
        public CurseMakerSpecificOption Load(IOptionLoader loader)
        {
            return new CurseMakerSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.CursingRange),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.AdditionalKillCool),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.TaskCurseTimeReduceRate),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.IsNotRemoveDeadBodyByTask),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.NotRemoveDeadBodyTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.IsDeadBodySearch),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.IsMultiDeadBodySearch),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.SearchDeadBodyTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.IsReduceSearchForTask),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.ReduceSearchTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerOption.ReduceSearchDeadBodyTime),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            );
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
