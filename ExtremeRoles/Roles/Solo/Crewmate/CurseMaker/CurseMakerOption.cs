using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.CurseMaker.CurseMakerRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.CurseMaker
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
                loader.GetValue<CurseMakerOption, float>(
                    CurseMakerOption.CursingRange),
                loader.GetValue<CurseMakerOption, float>(
                    CurseMakerOption.AdditionalKillCool),
                loader.GetValue<CurseMakerOption, int>(
                    CurseMakerOption.TaskCurseTimeReduceRate),
                loader.GetValue<CurseMakerOption, bool>(
                    CurseMakerOption.IsNotRemoveDeadBodyByTask),
                loader.GetValue<CurseMakerOption, int>(
                    CurseMakerOption.NotRemoveDeadBodyTaskGage),
                loader.GetValue<CurseMakerOption, bool>(
                    CurseMakerOption.IsDeadBodySearch),
                loader.GetValue<CurseMakerOption, bool>(
                    CurseMakerOption.IsMultiDeadBodySearch),
                loader.GetValue<CurseMakerOption, float>(
                    CurseMakerOption.SearchDeadBodyTime),
                loader.GetValue<CurseMakerOption, bool>(
                    CurseMakerOption.IsReduceSearchForTask),
                loader.GetValue<CurseMakerOption, int>(
                    CurseMakerOption.ReduceSearchTaskGage),
                loader.GetValue<CurseMakerOption, float>(
                    CurseMakerOption.ReduceSearchDeadBodyTime),
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
                CurseMakerOption.CursingRange,
                2.5f, 0.5f, 5.0f, 0.5f);

            factory.CreateFloatOption(
                CurseMakerOption.AdditionalKillCool,
                5.0f, 1.0f, 30.0f, 0.1f,
                format: OptionUnit.Second);

            IRoleAbility.CreateAbilityCountOption(
                factory, 1, 3, 5.0f);

            factory.CreateIntOption(
                CurseMakerOption.TaskCurseTimeReduceRate,
                0, 0, 10, 1,
                format: OptionUnit.Percentage);

            var removeDeadBodyOpt = factory.CreateBoolOption(
                CurseMakerOption.IsNotRemoveDeadBodyByTask,
                false);

            factory.CreateIntOption(
                CurseMakerOption.NotRemoveDeadBodyTaskGage,
                100, 0, 100, 5, removeDeadBodyOpt,
                format: OptionUnit.Percentage);

            var searchDeadBodyOption = factory.CreateBoolOption(
                CurseMakerOption.IsDeadBodySearch,
                true);

            factory.CreateBoolOption(
                CurseMakerOption.IsMultiDeadBodySearch,
                false, searchDeadBodyOption,
                invert: true);

            var searchTimeOpt = factory.CreateFloatOption(
                CurseMakerOption.SearchDeadBodyTime,
                60.0f, 0.5f, 120.0f, 0.5f,
                searchDeadBodyOption, format: OptionUnit.Second,
                invert: true);

            var taskBoostOpt = factory.CreateBoolOption(
                CurseMakerOption.IsReduceSearchForTask,
                false, searchDeadBodyOption,
                invert: true);

            factory.CreateIntOption(
                CurseMakerOption.ReduceSearchTaskGage,
                100, 25, 100, 5,
                taskBoostOpt,
                format: OptionUnit.Percentage,
                invert: true);

            var reduceTimeOpt = factory.CreateFloatDynamicOption(
                CurseMakerOption.ReduceSearchDeadBodyTime,
                30f, 0.5f, 0.5f, taskBoostOpt,
                format: OptionUnit.Second,
                invert: true,
                tempMaxValue: 120.0f);

            searchTimeOpt.AddWithUpdate(reduceTimeOpt);
        }
    }
}
